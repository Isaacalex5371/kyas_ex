using System.Text;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Tms.Api.Authorization;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Hubs;
using TmsApi.Api.Notifications;
using TmsApi.Api.RateLimiting;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Notifications;
using TmsApi.Application.Trasnscripts;
using TmsApi.Filters;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Middleware;
using TmsApi.TmsApi.Api;

var builder = WebApplication.CreateBuilder(args);

var services = new CryptoDemoService();
string hash1 = services.HashUserPassword("Password123!");
string hash2 =services.HashUserPassword("Password123!");

Console.WriteLine("*********************************************");
Console.WriteLine($"Hash 1: {hash1}");
Console.WriteLine($"Hash 2: {hash2}");
Console.WriteLine($"Verify 1: {services.VerifyUserPassword("Password123!",hash1)}");
Console.WriteLine($"Verify 2: {services.VerifyUserPassword("Password123!",hash2)}");
Console.WriteLine("*********************************************");
builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditCourse", policy =>
        policy.Requirements.Add(new CourseInstructorRequirement()));

builder.Services.AddSingleton<IAuthorizationHandler, CourseInstructorHandler>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});
// --- 1. SERVICES (BUILDER SECTION) ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly)
);
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.ShouldInclude = description => description.GroupName == "v1";
    }
);

builder.Services.AddOpenApi(
    "v2",
    options =>
    {
        options.ShouldInclude = description => description.GroupName == "v2";
    }
);

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true; // Tells the user which versions exist in the headers
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers(options =>
{
    // This applies the filter to EVERY controller in the project
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddRateLimiter(options =>
{
    // This is a "Global Limiter" - it applies to every single request
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        // Return a bucket based on the user's tier
        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"paid:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 200,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    AutoReplenishment = true,
                }
            ),

            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"free:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 30,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    AutoReplenishment = true,
                }
            ),

            _ => RateLimitPartition.GetTokenBucketLimiter( // Anonymous
                partitionKey: $"anon:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    AutoReplenishment = true,
                }
            ),
        };
    });

    // Custom "Too Many Requests" response
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                Title = "Rate limit exceeded",
                Detail = $"Too many requests. Retry after {retryAfter} seconds.",
                Status = 429,
            },
            ct
        );
    };
    options.AddConcurrencyLimiter(
        "transcripts",
        opt =>
        {
            opt.PermitLimit = 5; // Only 5 transcripts can be processed AT THE SAME TIME
            opt.QueueLimit = 10; // 10 more can wait in line
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        }
    );
});

// Exercise 2 Services & DI Validation
builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();

// Register TmsDbContext scoped for incoming HTTP requests

builder.Services.AddDbContext<TmsDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information) // Log SQLto output window
        .EnableSensitiveDataLogging()
); // Show parameters in querylogs (dev only)

builder.Services.AddIdentityCore<TmsUser>(options =>
    {
        // Password Policy
        options.Password.RequiredLength = 12;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        // Lockout Policy
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>() // Allows you to have "Admin", "Instructor", etc.
    .AddEntityFrameworkStores<TmsDbContext>(); // Connects Identity to your DB tables

builder.Services.AddControllers();


builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Exercise 3: Options Pattern
builder.Services.AddOptions<PaymentOptions>().BindConfiguration("Payments").ValidateDataAnnotations().ValidateOnStart();

// Session 1: Auth
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2),
    };
});
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<TranscriptWorker>();
// Replace 'TranscriptStatusStore' with the actual name of your implementation class
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();

builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();

builder.Services.AddCors(options =>
{
options.AddPolicy("AllowAngular", policy =>
policy.WithOrigins("http://localhost:4200")
.AllowAnyHeader()
.AllowAnyMethod());
});

var allowedOrigins = builder.Configuration
                         .GetSection("AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:4200"];
// Register the CORS policy in the Dependency Injection container
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // Vital for HttpOnly auth cook ies in Session 2
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

var app = builder.Build();

// --- 2. MIDDLEWARE PIPELINE (ORDER MATTERS) ---

app.UseStatusCodePages();
// 1. Logging is the outer wrapper (Session 1B)
app.MapHub<TmsHub>("/hubs/tms");
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<V1DeprecationMiddleware>();

// 2. Exception handling (Session 3 / Exercise 6)
app.UseExceptionHandler();
app.UseStatusCodePages(); //( Exercise 6 TODO 3) Turns 404s into JSON ProblemDetails

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.MapHealthChecks("/health/live").DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();
app.UseCors("TmsClient");
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true || context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices
            .GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
        new CookieOptions
        {
            HttpOnly = true, // MUST be false so Angular Jav aScript can read it!
            Secure = !builder.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict
        });
    }

    await next(context);
});
app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");
// 3. Environment Toggle (Exercise 7)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        // This adds the dropdown for V1 and V2
        options.AddDocument("v1", "API Version 1.0");
        options.AddDocument("v2", "API Version 2.0");
    });
}

// 4. Map Controllers (Exercise 5)
app.MapControllers();

// if (app.Environment.IsDevelopment())
// {
//     using var scope = app.Services.CreateScope();
//     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
//     await TmsApi.Persistence.DataSeeder.SeedAsync(context);
// }


app.Run();