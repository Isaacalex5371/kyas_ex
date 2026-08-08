
namespace TmsApi.Application.DTOs;

public record EnrollmentQueueResponseDto(
    string Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    DateTime EnrolledAt
);