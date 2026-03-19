using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Student;

[Authorize(Roles = "student")]
[Route("student/dashboard/[action]")]
public class DashboardController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public DashboardController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Student Dashboard";
        ViewData["SubTitle"] = "Tổng quan học tập của tôi";

        var studentId = CurrentUserId();

        var totalCourses = await _db.Coursestudents.AsNoTracking()
            .CountAsync(x => x.Studentid == studentId);

        var completedLessons = await _db.Lessonprogresses.AsNoTracking()
            .CountAsync(x => x.Studentid == studentId && x.Iscompleted == true);

        var totalAttempts = await _db.Examattempts.AsNoTracking()
            .CountAsync(x => x.Studentid == studentId);

        var avgScore = await _db.Examresults.AsNoTracking()
            .Where(x => x.Attempt.Studentid == studentId)
            .Select(x => (decimal?)x.Finalscore)
            .AverageAsync() ?? 0m;

        var myCourses = await _db.Coursestudents.AsNoTracking()
            .Where(x => x.Studentid == studentId)
            .Select(x => new
            {
                x.Courseid,
                x.Course.Coursename,
                SubjectName = x.Course.Subject.Subjectname,
                TeacherName = x.Course.Teacher.Fullname,
                Lessons = x.Course.Lessons.Count(l => l.Isdeleted == false && l.Ispublished == true)
            })
            .OrderBy(x => x.Coursename)
            .Take(6)
            .ToListAsync();

        var recentExams = await _db.Examattempts.AsNoTracking()
            .Where(x => x.Studentid == studentId)
            .OrderByDescending(x => x.Submittime ?? x.Starttime)
            .Select(x => new
            {
                x.Attemptid,
                x.Examid,
                ExamTitle = x.Exam.Title,
                x.Status,
                x.Totalscore,
                x.Submittime
            })
            .Take(8)
            .ToListAsync();

        ViewBag.TotalCourses = totalCourses;
        ViewBag.CompletedLessons = completedLessons;
        ViewBag.TotalAttempts = totalAttempts;
        ViewBag.AvgScore = avgScore;
        ViewBag.MyCourses = myCourses;
        ViewBag.RecentExams = recentExams;

        return View("~/Views/Student/Dashboard/Index.cshtml");
    }
}