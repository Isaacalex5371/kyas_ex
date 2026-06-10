////string? yitsna = null;
////string? x = yitsna ?? "";
////Console.WriteLine(x.ToUpper());


//// BAD for money
//double grantPerStudent = 1999.99;

//double totalAllocation = grantPerStudent * 100000;

//Console.WriteLine(totalAllocation);

using TmsCore;
using System.Diagnostics;
List<Student> students = [
new Student { Id = "S1", Name = "Abeba", Age = 22, GPA = 3.8m },
    new Student { Id = "S2", Name = "Kidane", Age = 21, GPA = 2.4m }, 
    new Student { Id = "S3", Name = "Dawit", Age = 20, GPA = 3.1m },
    new Student { Id = "S4", Name = "Sara", Age = 23, GPA = 3.9m },
    new Student { Id = "S5", Name = "Frehiwot", Age = 19, GPA = 2.0m },
    new Student { Id = "S6", Name = "Yonas", Age = 24, GPA = 3.5m }, 
    new Student { Id = "S7", Name = "Meron", Age = 22, GPA = 1.8m }, 
    new Student { Id = "S8", Name = "Tesfaye", Age = 21, GPA = 2.9m }
];
// var leaderboard = students.Where(student => student.GPA >= 3.5m).OrderByDescending(student => student.GPA).Select(student => student.Name).ToList();
// Console.WriteLine($"Found {leaderboard.Count()} Honors Students:");
// foreach (var name in leaderboard)
// {
//     Console.WriteLine($"- {name}");
// }

var enrollment = new EnrollmentRecord(
    "STU-001",
    "CS-401",
    DateTime.UtcNow
);
// enrollment.CourseCode = "HACKED";

var correceted = enrollment with
{
    CourseCode="cs-402"
};
var duplcated= new EnrollmentRecord(
    "STU-001",
    "CS-401",
    DateTime.UtcNow
);
var course = new Course { Code = "CS-401", Title = "Advanced C#", Capacity = 30 };
try
{
  course.Capacity=-5;
}
catch (ArgumentOutOfRangeException ex)
{
// Console.WriteLine($"Caught: {ex.Message}");
}

try
{
course.Title = "";
}
catch (ArgumentException ex)
{
// Console.WriteLine($"Caught: {ex.Message}");
}

var s = new Student { Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m };

// Console.WriteLine(enrollment);
// Console.WriteLine(correceted);
// Console.WriteLine($"same data?{enrollment==duplcated}");

// Console.WriteLine($"Student: {s.Name}, GPA: {s.GPA}");


void PrintGradeReport(IEnumerable<IGradable> assessments)
{
// Console.WriteLine("--- Grade Report ---");
foreach (var item in assessments)
{
// Console.WriteLine($"{item.Title}: {item.CalculateGrade():F2}%");
}
}
// Test it — one array holds two completely different types
IGradable[] cohortAssessments = [
new Quiz { Title = "C# Basics", CorrectAnswers = 18, TotalQuestions = 20 }, new LabAssignment { Title = "Registration API", FunctionalityScore = 90m, CodeQualityScore = 85m }
];
// PrintGradeReport(cohortAssessments);

var service = new EnrollmentService();

// Test 1: Valid registration
var validStudent = new Student { Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m };
var validCourse = new Course { Code = "CS-401", Title = "Advanced C#", Capacity = 30 };
var result = service.ProcessRegistration(validStudent, validCourse);
// Console.WriteLine($"Enrolled: {result.StudentId} in {result.CourseCode}");

// Test 2: Null student should throw
try
{
service.ProcessRegistration(null, validCourse);
}
catch (ArgumentNullException ex)
{
// Console.WriteLine($"Guard caught: {ex.ParamName}");
}

// Test 3: Full course should throw
var fullCourse = new Course { Code = "CS-402", Title = "Full Course", Capacity = 1 };
fullCourse.EnrolledCount = 1;
try
{
service.ProcessRegistration(validStudent, fullCourse);
}
catch (InvalidOperationException ex)
{
// Console.WriteLine($"Business rule: {ex.Message}");
}

var leaderboard = students.Where(s=> s.GPA >=3.5m).OrderByDescending(s=> s.GPA).Select(s=> s.Name).ToList();

decimal averageGpa= students.Average(s=> s.GPA);

var standingGroups = students.GroupBy(s=> s.GPA switch
{
    >= 3.5m=> "Honors",
    >= 2.5m =>  "Good Standing",

     >= 2.0m => "Probation",
      < 2.0m => "Academic Warning"
});

// Console.WriteLine($"Found {leaderboard.Count} Honors Students:");
// foreach (var name in leaderboard)
{
// Console.WriteLine($"- {name}");
}

// Console.WriteLine($"\nClass Average GPA: {averageGpa:F2}");

// Console.WriteLine("\n--- Academic Standing Report ---");
foreach (var group in standingGroups)
{
// Console.WriteLine($"\n{group.Key} ({group.Count()}):");
foreach (var b in group)
{
// Console.WriteLine($" {b.Name} GPA: {b.GPA}");
}
}

string[] backendCourses = ["C#", "ASP.NET Core"];
string[] frontendCourses = ["TypeScript", "Angular"];
string[] allCourses = [..backendCourses,..frontendCourses];
// Console.WriteLine($"\nFull curriculum: {string.Join(", ", allCourses)}");




var sw = Stopwatch.StartNew();
for (int i = 0; i < 5; i++)
{
Thread.Sleep(300); // Thread is HELD for 300ms cannot serve anyone else
}
Console.WriteLine($"Blocking sequential: {sw.ElapsedMilliseconds}ms");
// ASYNC BUT STILL SEQUENTIAL: Thread released, but calls are one-at-a-time
sw.Restart();
for (int i = 0; i < 5; i++)
{
await Task.Delay(300); // Thread released while waiting but still sequential
}
Console.WriteLine($"Async sequential: {sw.ElapsedMilliseconds}ms");
// THE RIGHT WAY: Async parallel all 5 start simultaneously
sw.Restart();
var tasks = Enumerable.Range(0, 5).Select(_ => Task.Delay(300));
await Task.WhenAll(tasks);
Console.WriteLine($"Async parallel: {sw.ElapsedMilliseconds}ms");



////////////////////////////////////////////////////////////////////////////////////////////////////////////////


async Task<Student> FetchStudentAsync(string id)
{
Console.WriteLine($" Fetching {id}...");
await Task.Delay(300); // Simulate database latency
return new Student
{
Id = id,
Name = $"Student-{id}", Age = 20, GPA = id switch
{ "S1" => 3.8m, "S2" => 2.4m, "S3" => 3.5m, "S4" => 1.9m, "S5" => 3.2m, _ => 2.5m
}
};
}

async Task<Course> FetchCourseAsync(string code)
{
Console.WriteLine($" Fetching course {code}...");
await Task.Delay(200); // Simulate database latency
return new Course
{
Code = code, Title = $"Course-{code}", Capacity = code switch
{ "CRS-101" => 2, "CRS-201" => 30, "CRS-301" => 15, _ => 25
}
};
}


sw.Restart();
// Start all fetches simultaneously students AND courses
string[] studentIds = ["S1", "S2", "S3", "S4", "S5"];
string[] courseCodes = ["CRS-101", "CRS-201", "CRS-301"];
var studentTasks = studentIds.Select(id => FetchStudentAsync(id));
var courseTasks = courseCodes.Select(code => FetchCourseAsync(code));
// Both arrays load concurrently
Student[] student = await Task.WhenAll(studentTasks);
Course[] courses = await Task.WhenAll(courseTasks);
Console.WriteLine($"\nLoaded {student.Length} students and {courses.Length} courses in {sw.ElapsedMilliseconds}ms");
foreach (var a in students)
{
Console.WriteLine($" {s.Name} GPA: {s.GPA}");
}


var enrollCourse = new Course { Code = "CRS-101", Title = "C# Mastery", Capacity = 2 };
var enrollService = new EnrollmentService();
var enrollments = new List<EnrollmentRecord>();
var failures = new List<string>();
sw.Restart();
foreach (var studentss in students)
{
try
{
var record = enrollService.ProcessRegistration(studentss, enrollCourse);
enrollCourse.EnrolledCount++;
enrollments.Add(record);
Console.WriteLine($" Enrolled: {studentss.Name}");
}
catch (InvalidOperationException ex)
{
failures.Add($"{studentss.Name}: {ex.Message}");
Console.WriteLine($" Rejected: {studentss.Name} {ex.Message}");
}
}


async Task SendConfirmationAsync(Student student)
{
try
{
await Task.Delay(100); // Simulate sending email
Console.WriteLine($" Email sent to {student.Name}");
}
catch (Exception ex)
{
// Log the failure do NOT re-throw. // This is intentional fire-and-forget.
 Console.WriteLine($" Email failed for {student.Name}: {ex.Message}");
}
}

////module 3 

Student studentsss = new()
{
    Id = "S1",
    Name = "Abeba",
    Age = 22,
    GPA = 3.8m
};

EnrollmentService services = new();

service.FinalizeEnrollment(studentsss);