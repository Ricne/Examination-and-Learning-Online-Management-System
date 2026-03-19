using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;

namespace OMS.Controllers.Admin;

[Authorize(Roles = "admin")]
[Route("admin/dashboard/[action]")]
public class DashboardController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    public DashboardController(OnlineLearningExamSystemContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Admin Dashboard";
        ViewData["SubTitle"] = "Tổng quan hệ thống";
        ViewData["Badge"] = "Admin";
        ViewData["Breadcrumb"] = "Admin / Dashboard";

        var roleTeacherId = await _db.Roles.AsNoTracking()
            .Where(r => r.Rolename == "teacher").Select(r => r.Roleid).FirstOrDefaultAsync();

        var roleStudentId = await _db.Roles.AsNoTracking()
            .Where(r => r.Rolename == "student").Select(r => r.Roleid).FirstOrDefaultAsync();

        var totalUsers = await _db.Users.AsNoTracking().CountAsync();
        var totalTeachers = await _db.Users.AsNoTracking().CountAsync(u => u.Roleid == roleTeacherId);
        var totalStudents = await _db.Users.AsNoTracking().CountAsync(u => u.Roleid == roleStudentId);

        var totalSubjectsActive = await _db.Subjects.AsNoTracking()
            .CountAsync(s => s.Isdeleted == false && s.Isactive == true);

        var totalCoursesActive = await _db.Courses.AsNoTracking()
            .CountAsync(c => c.Isdeleted == false && c.Isactive == true);

        var totalExamsPublished = await _db.Exams.AsNoTracking()
            .CountAsync(e => e.Isdeleted == false && e.Ispublished == true);

        var totalProgressRows = await _db.Lessonprogresses.AsNoTracking().CountAsync();
        var completedRows = await _db.Lessonprogresses.AsNoTracking().CountAsync(x => x.Iscompleted == true);
        var completionRate = totalProgressRows == 0 ? 0 : (completedRows * 100.0m / totalProgressRows);

        var avgScore = await _db.Examresults.AsNoTracking()
            .Select(r => (decimal?)r.Finalscore)
            .AverageAsync() ?? 0m;

        var topCourses = await _db.Courses.AsNoTracking()
            .Where(c => c.Isdeleted == false)
            .Select(c => new
            {
                c.Courseid,
                c.Coursename,
                SubjectName = c.Subject.Subjectname,
                TeacherName = c.Teacher.Fullname,
                Students = c.Coursestudents.Count
            })
            .OrderByDescending(x => x.Students)
            .ThenBy(x => x.Coursename)
            .Take(5)
            .ToListAsync();

        var latestAttempts = await _db.Examattempts.AsNoTracking()
            .Where(a => a.Status == "submitted" || a.Status == "graded")
            .OrderByDescending(a => a.Submittime ?? a.Starttime)
            .Select(a => new
            {
                a.Attemptid,
                a.Examid,
                ExamTitle = a.Exam.Title,
                StudentName = a.Student.Fullname,
                a.Status,
                a.Submittime,
                a.Totalscore
            })
            .Take(10)
            .ToListAsync();

        ViewBag.TotalUsers = totalUsers;
        ViewBag.TotalTeachers = totalTeachers;
        ViewBag.TotalStudents = totalStudents;
        ViewBag.TotalSubjectsActive = totalSubjectsActive;
        ViewBag.TotalCoursesActive = totalCoursesActive;
        ViewBag.TotalExamsPublished = totalExamsPublished;
        ViewBag.CompletionRate = completionRate;
        ViewBag.AvgScore = avgScore;

        ViewBag.TopCourses = topCourses;
        ViewBag.LatestAttempts = latestAttempts;

        return View("~/Views/Admin/Dashboard/Index.cshtml");
    }
}