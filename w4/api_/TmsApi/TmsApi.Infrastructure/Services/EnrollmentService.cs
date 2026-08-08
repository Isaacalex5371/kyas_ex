using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For ToListAsync, FirstOrDefaultAsync, Include, AnyAsync, etc.

using TmsApi.Domain.Entities;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Infrastructure.Services;

public class EnrollmentService(TmsDbContext _context, ILogger<EnrollmentService> _logger)
    : IEnrollmentService
{
    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    )
    {
        // TODO 2: Insert, Save, Re-read
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct
    )
    {
        return await _context
            .Enrollments.AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        PagedRequest request,
        CancellationToken ct
    )
    {
        //start with no tracking
        var query = _context.Enrollments.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var sortBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "EnrolledAt" : request.OrderBy;

        query = sortBy switch
        {
            "Grade" => request.Descending
                ? query.OrderByDescending(e => e.Grade)
                : query.OrderBy(e => e.Grade),
            "EnrolledAt" or _ => request.Descending
                ? query.OrderByDescending(e => e.EnrolledAt)
                : query.OrderBy(e => e.EnrolledAt),
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);

        return new PagedResponse<EnrollmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollmentToDelete = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id);

        if (enrollmentToDelete == null)
        {
            _logger.LogWarning("Delete failed: Enrollment with ID '{EnrollmentId}' not found.", id);
            return false;
        }

        _context.Enrollments.Remove(enrollmentToDelete); // Stage for deletion
        await _context.SaveChangesAsync(); // Commit deletion to the database

        _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);

        return true;
    }

    public async Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct)
{
    return await _context.Enrollments
        .AnyAsync(e => e.StudentId == studentId && e.Course.Code == courseCode, ct);
}

public async Task AddAsync(Enrollment enrollment, CancellationToken ct)
{
    _context.Enrollments.Add(enrollment);
    await _context.SaveChangesAsync(ct);
}

public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct)
{
    return await _context.Enrollments
        .AsNoTracking()
        .Include(e => e.Course)
        .Where(e => e.StudentId == studentId)
        .ToListAsync(ct);
}
// M9: Get all enrollments projected into the secure Queue DTO
    public async Task<IEnumerable<EnrollmentQueueResponseDto>> GetEnrollmentQueueAsync(CancellationToken ct)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsNoTracking()
            .Select(e => new EnrollmentQueueResponseDto(
                e.Id.ToString(),
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status,
                e.EnrolledAt
            ))
            .ToListAsync(ct);
    }

    // M9: Approve a specific pending registration
    public async Task<bool> ApproveEnrollmentAsync(int id, CancellationToken ct)
    {
        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (enrollment == null)
        {
            return false;
        }

        enrollment.Status = "Approved";
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

public class TmsDatabaseException(string message) : Exception(message);