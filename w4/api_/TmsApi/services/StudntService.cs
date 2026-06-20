using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

public interface IStudentService
{
    Task<Student> RegisterAsync(string name, decimal gpa);
    Task<Student?> GetByIdAsync(int id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}

public class StudentService : IStudentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        TmsDbContext context,
        ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Student> RegisterAsync(
        string name,
        decimal gpa)
    {
        var student = new Student
        {
            RegistrationNumber = $"TMS-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..4]}",
            Name = name,
            GPA = gpa,
            IsActive = true
        };

        _context.Students.Add(student);

        await _context.SaveChangesAsync();

        return student;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        return await _context.Students
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return false;

        if (student.Enrollments.Any())
            return false;

        _context.Students.Remove(student);

        await _context.SaveChangesAsync();

        return true;
    }
}