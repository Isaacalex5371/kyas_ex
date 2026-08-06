using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetStudentScheduleHandler(IEnrollmentService repo)
    : IRequestHandler<GetStudentScheduleQuery, ScheduleDto>
{
    public async Task<ScheduleDto> Handle(
        GetStudentScheduleQuery query, CancellationToken ct)
    {
        // 1. Fetch all enrollments for this student (eagerly loading the Courses)
        var enrollments = await repo.GetByStudentIdAsync(query.StudentId, ct);

        // 2. Project the enrollments into flat Schedule DTOs
        var items = enrollments.Select(e => new ScheduleItemDto(
            e.Course.Code,
            e.Course.Title,
            "TBD" // Placeholder timetable string until scheduled times are introduced
        )).ToList();

        return new ScheduleDto(query.StudentId, items);
    }
}