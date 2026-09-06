namespace TmsApi.Application.DTOs;

public record EnrollmentDetailDto
(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,

    string CourseTitle,
        string Status,
    DateTime EnrolledAt
);