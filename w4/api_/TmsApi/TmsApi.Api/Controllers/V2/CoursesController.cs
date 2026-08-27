using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[Authorize(Roles = "Instructor,Admin")]
[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    TmsDbContext context,
    ICachedCourseService cachedCourseService,
    IAuthorizationService authorizationService) : ControllerBase
{
    private readonly TmsDbContext _context = context;
    private readonly ICachedCourseService _cachedCourseService = cachedCourseService;
    private readonly IAuthorizationService _authorizationService = authorizationService;


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        var authResult = await _authorizationService
            .AuthorizeAsync(User, course, "CanEditCourse");

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        course.Title = dto.Title;

        await _context.SaveChangesAsync();

        return NoContent();
    }


    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var baseQuery = _context.Courses.AsNoTracking();

        var totalCount = await baseQuery.CountAsync(ct);

        var rows = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Code,
                c.Capacity,
                EnrollmentCount = c.Enrollments.Count,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        return Ok(
            new
            {
                data = rows,
                meta = new
                {
                    totalCount,
                    page,
                    pageSize,
                    totalPages,
                    hasNext,
                    hasPrevious,
                },
                links = new
                {
                    self =
                        $"/api/v2/courses?page={page}&pageSize={pageSize}",

                    next = hasNext
                        ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                        : (string?)null,

                    prev = hasPrevious
                        ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                        : (string?)null,

                    enroll = "/api/v2/enrollments",
                },
            }
        );
    }


    [HttpGet("all")]
    public async Task<IActionResult> GetAllCourses(
        CancellationToken ct)
    {
        var courses =
            await _cachedCourseService.GetAllCoursesAsync(ct);

        return Ok(courses);
    }
}