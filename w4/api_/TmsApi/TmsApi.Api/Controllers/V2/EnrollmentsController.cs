using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.DTOs;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;
// For IEnrollmentService

// For EnrollmentQueueResponseDto

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(IMediator mediator, IEnrollmentService enrollmentService ,IHubContext<TmsHub,
    ITmsHubClient> hubContext) : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService = enrollmentService;


    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentCreated), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course (V2)")]
    public async Task<IActionResult> Enroll(EnrollStudentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Match<IActionResult>(
            onSuccess: created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new { studentId = created.StudentId },
                    created
                ),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "course_full" or "already_enrolled" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest,
                };
                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}"
                );
            }
        );
    }
    // GET /api/v2/enrollments/{studentId}/schedule
    [HttpGet("{studentId}/schedule")]
    [HttpGet("{studentId:int}/schedule", Name = nameof(GetSchedule))]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get student's enrolled schedule (V2)")]
    public async Task<IActionResult> GetSchedule(int studentId, CancellationToken ct)
    {
        var schedule = await mediator.Send(new GetStudentScheduleQuery(studentId), ct);
        return Ok(schedule);
    }

    // GET /api/v2/enrollments
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentQueueResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all enrollment requests (V2 Queue)")]
    // public async Task<IActionResult> GetAll(CancellationToken ct)
    // {
    //     // Go through the service layer to retrieve the data
    //     var list = await enrollmentService.GetEnrollmentQueueAsync(ct);
    //     return Ok(list);
    // }
    public async Task<IActionResult> GetAll([FromQuery]PagedRequest request ,CancellationToken ct)
    {
        // Go through the service layer to retrieve the data
        var list= await enrollmentService.GetEnrollmentQueueAsync(ct);
        var res = new PagedResponse<EnrollmentQueueResponseDto>
        {
            Items = (IReadOnlyList<EnrollmentQueueResponseDto>)list,
            TotalCount = 0,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Ok(res);
    }
    

    // POST /api/v2/enrollments/{id}/approve
    [IgnoreAntiforgeryToken]
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Approve an enrollment request (V2 Queue)")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var success = await enrollmentService.ApproveEnrollmentAsync(id, ct);

        if (!success)
        {
            return NotFound();
        }

        await hubContext.Clients.All
            .ReceiveEnrollmentStatusUpdated(id, "Approved");

        return NoContent();
    }

    [IgnoreAntiforgeryToken]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateEnrollmentStatus(
        int id,
        [FromBody] UpdateEnrollmentStatusRequest request,
        CancellationToken ct)
    {
        Console.WriteLine("🔥 CONTROLLER REACHED");
        Console.WriteLine($"🔥 ID: {id}");
        Console.WriteLine($"🔥 STATUS: {request.Status}");

        return Ok(new
        {
            id,
            status = request.Status
        });
    }


}