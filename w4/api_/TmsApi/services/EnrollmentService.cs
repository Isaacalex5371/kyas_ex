using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public record EnrollmentRecord(
    int Id,
    int StudentId,
    int CourseId,
    DateTime EnrolledAt
);

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(int studentId, int courseId);
    Task<EnrollmentRecord?> GetByIdAsync(int id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        TmsDbContext context,
        ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentRecord> EnrollAsync(
        int studentId,
        int courseId)
    {
        // Student exists
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student is null)
        {
            throw new ArgumentException(
                $"Student {studentId} does not exist.");
        }

        // Course exists
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
        {
            throw new ArgumentException(
                $"Course {courseId} does not exist.");
        }

        // Duplicate enrollment check
        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e =>
                e.StudentId == studentId &&
                e.CourseId == courseId);

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt Student {StudentId} Course {CourseId}",
                studentId,
                courseId);

            return new EnrollmentRecord(
                existing.Id,
                existing.StudentId,
                existing.CourseId,
                DateTime.UtcNow);
        }

        // Capacity check
        if (course.Enrollments.Count >= course.Capacity)
        {
            throw new ArgumentException(
                $"Course {course.Title} is full.");
        }

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            Grade = 0m
        };

        _context.Enrollments.Add(enrollment);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Enrolled Student {StudentId} in Course {CourseId}",
            studentId,
            courseId);

        return new EnrollmentRecord(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            DateTime.UtcNow);
    }

    public async Task<EnrollmentRecord?> GetByIdAsync(int id)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
        {
            return null;
        }

        return new EnrollmentRecord(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        return await _context.Enrollments
            .Select(e => new EnrollmentRecord(
                e.Id,
                e.StudentId,
                e.CourseId,
                DateTime.UtcNow))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
        {
            return false;
        }

        _context.Enrollments.Remove(enrollment);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted enrollment {EnrollmentId}",
            id);

        return true;
    }
}

