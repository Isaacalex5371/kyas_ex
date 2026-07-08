using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs; // For EnrollmentRecord DTO
using TmsApi.Services; // For IEnrollmentService

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService _courseService,
    IEnrollmentService _enrollmentService)
    : ControllerBase
{
    // GET /api/enrollments


    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]

    [ProducesResponseType(
    typeof(EnrollmentResponseDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("Get one enrolment for a course")]
[EndpointDescription("Returns a single enrolment by its ID.")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]

    [ProducesResponseType(
    typeof(EnrollmentResponseDto),
    StatusCodes.Status201Created)]
[ProducesResponseType(
    typeof(ValidationProblemDetails),
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status409Conflict)]
[EndpointSummary("Enrol a student in a course")]
[EndpointDescription(
    "Returns 404 if the course does not exist, and 409 if the course has reached its capacity.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    )
    {
        // Rule 1: 404 before 409. Does the course exist?
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course == null)
            return NotFound();

        // Rule 2: Check capacity
        if (course.EnrollmentCount >= course.Capacity)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Course is full",
                    Detail =
                        $"Course '{course.Title}' has reached its maximum capacity of {course.Capacity}.",
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }

        var result = await _enrollmentService.CreateAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetEnrollment), new { courseId, id = result.Id }, result);
    }

    // DELETE /api/enrollments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Changed id type to int
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(
    typeof(IReadOnlyList<EnrollmentResponseDto>),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status404NotFound)]
[EndpointSummary("List enrolments for a course")]
[EndpointDescription("Returns all enrolments belonging to the specified course.")]
public async Task<IActionResult> GetEnrollments(
    int courseId,
    CancellationToken ct)
{
    // Check if the course exists

    var course = await _courseService.GetByIdAsync(courseId, ct);

    if (course is null)
        return NotFound();

    var enrollments = await _enrollmentService.GetByCourseAsync(courseId, ct);

    return Ok(enrollments);
}
}

// Request Model
public record CreateEnrollmentRequest(string StudentId, string CourseCode);