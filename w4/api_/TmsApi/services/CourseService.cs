using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public record CourseRecord(
    int Id,
    string Code,
    string Title,
    int Capacity,
    int EnrolledCount
);

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, int capacity);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        TmsDbContext context,
        ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CourseRecord> CreateAsync(
        string code,
        string title,
        int capacity)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Course code is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title is required.");

        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0.");

        var exists = await _context.Courses
            .AnyAsync(c => c.Code == code);

        if (exists)
            throw new ArgumentException($"Course {code} already exists.");

        var course = new Course
        {
            Code = code,
            Title = title,
            Capacity = capacity
        };

        _context.Courses.Add(course);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created course {CourseCode} with title {CourseTitle}",
            code,
            title);

        return new CourseRecord(
            course.Id,
            course.Code,
            course.Title,
            course.Capacity,
            0
        );
    }

    public async Task<CourseRecord?> GetByCodeAsync(string code)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code);

        if (course is null)
        {
            _logger.LogWarning(
                "Course {CourseCode} not found",
                code);

            return null;
        }

        return new CourseRecord(
            course.Id,
            course.Code,
            course.Title,
            course.Capacity,
            course.Enrollments.Count
        );
    }

    public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        return await _context.Courses
            .Include(c => c.Enrollments)
            .Select(c => new CourseRecord(
                c.Id,
                c.Code,
                c.Title,
                c.Capacity,
                c.Enrollments.Count
            ))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(string code)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code);

        if (course is null)
        {
            _logger.LogWarning(
                "Delete failed: course {CourseCode} not found",
                code);

            return false;
        }

        if (course.Enrollments.Any())
        {
            _logger.LogWarning(
                "Cannot delete course {CourseCode}, active enrollments exist",
                code);

            return false;
        }

        _context.Courses.Remove(course);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted course {CourseCode}",
            code);

        return true;
    }
}