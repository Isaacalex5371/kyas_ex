using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Tms.Api.Dtos;
using TmsApi.Dtos;
using TmsApi.DTOs;
using TmsApi.Services; // For ICourseService
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService _courseService,
    LinkGenerator linkGenerator)
    : ControllerBase
{
    // GET /api/courses
       [HttpGet]
       [ProducesResponseType(
    typeof(PagedResponse<CourseResponseDto>),
    StatusCodes.Status200OK)]
[EndpointSummary("List courses with pagination")]
[EndpointDescription(
    "Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
public async Task<IActionResult> GetCourses(
    [FromQuery] PagedRequest request,
    CancellationToken ct)
{
    var result = await _courseService.GetCoursesAsync(request, ct);

    return Ok(result);
}
    // GET /api/courses/{code}
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var record = await _courseService.GetByCodeAsync(code);
        return record is not null ? Ok(record) : NotFound(); // Returns 200 OK or 404 Not Found
    }


    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(
    typeof(CourseDetailDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Get a course by ID")]
[EndpointDescription(
    "Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
{
    var course = await _courseService.GetByIdAsync(id, ct);

    if (course is null)
        return NotFound();

    var selfPath = linkGenerator.GetPathByName(
        HttpContext,
        nameof(GetCourseById),
        new { id });

    var enrollmentsPath = linkGenerator.GetPathByName(
        HttpContext,
        "ListCourseEnrollments",
        new { courseId = id });

    var links = new List<LinkDto>
    {
        new(selfPath!, "self", "GET"),
        new(selfPath!, "update", "PUT"),
        new(selfPath!, "delete", "DELETE"),
        new(enrollmentsPath!, "enrollments", "GET")
    };

    if (course.EnrollmentCount < course.Capacity)
    {
        links.Add(
            new LinkDto(
                enrollmentsPath!,
                "enroll",
                "POST"
            ));
    }

    var detail = new CourseDetailDto
    {
        Id = course.Id,
        Code = course.Code,
        Title = course.Title,
        Capacity = course.Capacity,
        EnrollmentCount = course.EnrollmentCount,
        Links = links
    };

    return Ok(detail);
}
    [HttpPost]

    [ProducesResponseType(
    typeof(CourseResponseDto),
    StatusCodes.Status201Created)]
[ProducesResponseType(
    typeof(ValidationProblemDetails),
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
[EndpointSummary("Create a new course")]
[EndpointDescription(
    "Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // Check business rule BEFORE trying to save
        if (await _courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Course code already exists",
                    Detail = $"A course with code '{request.Code}' is already registered.",
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }
        // TODO 4: Call CreateAsync and return CreatedAtAction
        var result = await _courseService.CreateAsync(request, ct);

        // This pattern is required for the 'Location' header in the response
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

    // // POST /api/courses
    // [HttpPost]
    // public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    // {
    //     try
    //     {
    //         var record = await _courseService.CreateAsync(
    //             request.Code,
    //             request.Title,
    //             request.Capacity
    //         );
    //         // Returns 201 Created with Location header and the created CourseRecord DTO
    //         return CreatedAtAction(nameof(GetByCode), new { code = record.Code }, record);
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         // Catch specific validation exceptions for better error messages
    //         return BadRequest(new { Message = ex.Message });
    //     }
    // }

    // DELETE /api/courses/{code}
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var deleted = await _courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound(); // Returns 204 No Content or 404 Not Found
    }

    // GET /api/courses/top-by-enrollment
    [HttpGet("top-by-enrollment")]
    public async Task<IActionResult> GetTopCoursesByEnrollment([FromQuery] int topCount = 5)
    {
        var topCourses = await _courseService.GetTopCoursesByEnrollmentAsync(topCount);
        return Ok(topCourses);
    }
}