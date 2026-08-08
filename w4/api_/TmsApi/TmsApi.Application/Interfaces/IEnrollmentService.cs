using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;
public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    );
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
        Task<PagedResponse<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        PagedRequest request,
        CancellationToken ct
    );
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
Task AddAsync(Enrollment enrollment, CancellationToken ct);

Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);

Task<IEnumerable<EnrollmentQueueResponseDto>> GetEnrollmentQueueAsync(CancellationToken ct);
    Task<bool> ApproveEnrollmentAsync(int id, CancellationToken ct);
}