// this is the code and what should i do bro i did no what is going on 
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Middleware;
using TmsApi.Services;
using TmsApi.TmsApi.Api;
var builder = WebApplication.CreateBuilder(args);


// --- 1. SERVICES (BUILDER SECTION) ---

builder.Services.AddProblemDetails(); // Required for Exercise 6
builder.Services.AddOpenApi(); // Required for Exercise 7
builder.Services.AddControllers(); // Required for Exercise 5
builder.Services.AddExceptionHandler(options => { }); // Required to prevent startup crash

// Exercise 2 Services & DI Validation
builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();


builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude=description => description.GroupName == "v1";
});

 builder.Services.AddOpenApi("v2", options =>
 {
    options.ShouldInclude=description => description.GroupName == "v2";
 });

 builder.Services.AddApiVersioning(options =>
 {
        options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
  options.ReportApiVersions = true;
  options.ApiVersionReader= new UrlSegmentApiVersionReader();
options.ApiVersionReader = ApiVersionReader.Combine(
new UrlSegmentApiVersionReader(),
new HeaderApiVersionReader("X-Api-Version"));
  
 }).AddApiExplorer(options =>
 {
    options.GroupNameFormat="'v'VVV";
    options.SubstituteApiVersionInUrl=true;
 });

// Register TmsDbContext scoped for incoming HTTP requests

builder.Services.AddDbContext<TmsDbContext>(options =>options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
     // Show parameters in querylogs (dev only)

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Exercise 3: Options Pattern
builder
    .Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Session 1: Auth
builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// --- 2. MIDDLEWARE PIPELINE (ORDER MATTERS) ---

// 1. Logging is the outer wrapper (Session 1B)
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception handling (Session 3 / Exercise 6)
app.UseExceptionHandler();
app.UseStatusCodePages(); //( Exercise 6 TODO 3) Turns 404s into JSON ProblemDetails

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 3. Environment Toggle (Exercise 7)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
  app.MapScalarApiReference(options =>
{
options.WithTitle("TMS API Reference")
.WithTheme(ScalarTheme.DeepSpace)
.WithDefaultHttpClient(ScalarTarget.CSharp,
ScalarClient.HttpClient);
// Tell Scalar to pull both documents into its sidebar dropdown
options
.AddDocument("v1", "API Version 1.0")
.AddDocument("v2", "API Version 2.0");
});
}

app.UseMiddleware<V1DeprecationMiddleware>();

// 4. Map Controllers (Exercise 5)
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}


// --- 3. MINIMAL API ENDPOINTS (FOR TESTING) ---

app.MapGet(
        "/api/assessments/results",
        () =>
            Results.Ok(
                new
                {
                    courseCode = "CS-101",
                    studentId = "S-001",
                    letterGrade = "A",
                }
            )
    )
    .RequireAuthorization();

app.MapGet(
    "/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
    {
        worker.ProcessBatch();
        return Results.Ok("processed");
    }
);

app.MapGet(
    "/api/error",
    () =>
    {
        throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
    }
);

app.Run();