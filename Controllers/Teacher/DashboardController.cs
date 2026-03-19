using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/dashboard/[action]")]
public class DashboardController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    public DashboardController(OnlineLearningExamSystemContext db) => _db = db;

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Teacher Dashboard";
        ViewData["SubTitle"] = "Tổng quan giảng viên";

        var teacherId = CurrentUserId();

        var totalCourses = await _db.Courses.AsNoTracking()
            .CountAsync(c => c.Teacherid == teacherId && c.Isdeleted == false);

        var activeCourses = await _db.Courses.AsNoTracking()
            .CountAsync(c => c.Teacherid == teacherId && c.Isdeleted == false && c.Isactive == true);

        var totalQuestions = await _db.Questions.AsNoTracking()
            .Include(q => q.Course)
            .CountAsync(q => q.Isdeleted == false && q.Course.Teacherid == teacherId);

        var totalExams = await _db.Exams.AsNoTracking()
            .Include(e => e.Course)
            .CountAsync(e => e.Isdeleted == false && e.Course.Teacherid == teacherId);

        var publishedExams = await _db.Exams.AsNoTracking()
            .Include(e => e.Course)
            .CountAsync(e => e.Isdeleted == false && e.Ispublished == true && e.Course.Teacherid == teacherId);

        var pendingToGrade = await _db.Examattempts.AsNoTracking()
            .Include(a => a.Exam).ThenInclude(e => e.Course)
            .CountAsync(a =>
                (a.Status == "submitted") &&
                a.Exam.Course.Teacherid == teacherId);

        var avgFinalScore = await _db.Examresults.AsNoTracking()
            .Where(r => r.Attempt.Exam.Course.Teacherid == teacherId)
            .Select(r => (decimal?)r.Finalscore)
            .AverageAsync() ?? 0m;

        var topCourses = await _db.Courses.AsNoTracking()
            .Where(c => c.Teacherid == teacherId && c.Isdeleted == false)
            .Select(c => new
            {
                c.Courseid,
                c.Coursename,
                Students = c.Coursestudents.Count,
                c.Isactive
            })
            .OrderByDescending(x => x.Students)
            .ThenBy(x => x.Coursename)
            .Take(5)
            .ToListAsync();

        var latestAttempts = await _db.Examattempts.AsNoTracking()
            .Where(a => a.Exam.Course.Teacherid == teacherId && (a.Status == "submitted" || a.Status == "graded"))
            .OrderByDescending(a => a.Submittime ?? a.Starttime)
            .Select(a => new
            {
                a.Attemptid,
                ExamTitle = a.Exam.Title,
                StudentName = a.Student.Fullname,
                a.Status,
                a.Submittime,
                a.Totalscore
            })
            .Take(10)
            .ToListAsync();

        ViewBag.TotalCourses = totalCourses;
        ViewBag.ActiveCourses = activeCourses;
        ViewBag.TotalQuestions = totalQuestions;
        ViewBag.TotalExams = totalExams;
        ViewBag.PublishedExams = publishedExams;
        ViewBag.PendingToGrade = pendingToGrade;
        ViewBag.AvgFinalScore = avgFinalScore;

        ViewBag.TopCourses = topCourses;
        ViewBag.LatestAttempts = latestAttempts;

        return View("~/Views/Teacher/Dashboard/Index.cshtml");
    }
}