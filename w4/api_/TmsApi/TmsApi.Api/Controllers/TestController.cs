using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
namespace TmsApi.Api.Controllers;
[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
[HttpGet("deferred")]
public IActionResult TestDeferred()
{
Console.WriteLine("\n>>> STEP 1: Building the query object (nodatabase contact)...");
var query = context.Students.Where(s => s.GPA >= 3.0m);
Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
var orderedQuery = query.OrderBy(s => s.Name);
Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
var results = orderedQuery.ToList(); // Execution is triggeredhere
Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
return Ok(results);


}
// Non-translatable helper method
private static bool IsHonorRoll(decimal gpa)
{
return gpa >= 3.5m;
}
[HttpGet("translation-fail")]
public IActionResult TestTranslationFail()
{
Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
try
{
var students = context.Students.AsEnumerable()
.Where(s => IsHonorRoll(s.GPA)) // EF Core does not know how to map this method to SQL
.ToList();

return Ok(students);
}
catch (Exception ex)
{
Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
return BadRequest(new { Message = ex.Message });
}
}

[HttpGet("active-students-count")]
public async Task<IActionResult>  ActiveStudentsCount()
    {
        var count = await context.Students
        .Where(s=> s.IsActive && s.GPA >= 3.0m)
        .CountAsync();
        return Ok(count);
    }

     [HttpGet("courses-by-enrollment")]
    public async Task<IActionResult> CoursesByEnrollment()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

   
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> AverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("students-with-no-enrollments")]
    public async Task<IActionResult> StudentsWithNoEnrollments()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    } 
}