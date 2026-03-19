using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Student;

[Authorize(Roles = "student")]
[Route("student/courses/[action]")]
public class CoursesController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public CoursesController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Available(string? keyword = null)
    {
        ViewData["Title"] = "Khóa học có thể đăng ký";
        ViewData["SubTitle"] = "Danh sách khóa học đang mở cho học viên";

        var studentId = CurrentUserId();
        ViewBag.Keyword = keyword;

        var enrolledCourseIds = await _db.Coursestudents.AsNoTracking()
            .Where(x => x.Studentid == studentId)
            .Select(x => x.Courseid)
            .ToListAsync();

        var query = _db.Courses.AsNoTracking()
            .Where(c =>
                c.Isdeleted == false &&
                c.Isactive == true &&
                !enrolledCourseIds.Contains(c.Courseid))
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            query = query.Where(c =>
                c.Coursename.Contains(keyword) ||
                (c.Description != null && c.Description.Contains(keyword)) ||
                c.Subject.Subjectname.Contains(keyword) ||
                c.Teacher.Fullname.Contains(keyword));
        }

        var list = await query
            .OrderBy(c => c.Coursename)
            .Select(c => new
            {
                c.Courseid,
                c.Coursename,
                c.Description,
                SubjectName = c.Subject.Subjectname,
                TeacherName = c.Teacher.Fullname,
                LessonsCount = c.Lessons.Count(l => l.Isdeleted == false && l.Ispublished == true),
                ExamsCount = c.Exams.Count(e => e.Isdeleted == false && e.Ispublished == true)
            })
            .ToListAsync();

        return View("~/Views/Student/Courses/Available.cshtml", list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var studentId = CurrentUserId();

        var course = await _db.Courses
            .FirstOrDefaultAsync(c => c.Courseid == courseId && c.Isdeleted == false && c.Isactive == true);

        if (course == null)
        {
            TempData["Error"] = "Khóa học không tồn tại hoặc không còn hoạt động.";
            return Redirect("/student/courses/available");
        }

        var existed = await _db.Coursestudents
            .AnyAsync(x => x.Courseid == courseId && x.Studentid == studentId);

        if (existed)
        {
            TempData["Error"] = "Bạn đã đăng ký khóa học này rồi.";
            return Redirect("/student/courses/index");
        }

        _db.Coursestudents.Add(new OMS.Models.Entities.Coursestudent
        {
            Courseid = courseId,
            Studentid = studentId,
            Enrolledat = DateTime.Now
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Đăng ký khóa học thành công.";
        return Redirect("/student/courses/index");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Khóa học của tôi";
        ViewData["SubTitle"] = "Danh sách khóa học đã đăng ký";

        var studentId = CurrentUserId();

        var courses = await _db.Coursestudents.AsNoTracking()
            .Where(x => x.Studentid == studentId)
            .Select(x => new
            {
                x.Courseid,
                x.Course.Coursename,
                SubjectName = x.Course.Subject.Subjectname,
                TeacherName = x.Course.Teacher.Fullname,
                LessonsCount = x.Course.Lessons.Count(l => l.Isdeleted == false && l.Ispublished == true),
                ProgressCount = x.Course.Lessons.Count(l =>
                    l.Isdeleted == false &&
                    l.Ispublished == true &&
                    l.Lessonprogresses.Any(lp => lp.Studentid == studentId && lp.Iscompleted == true))
            })
            .OrderBy(x => x.Coursename)
            .ToListAsync();

        return View("~/Views/Student/Courses/Index.cshtml", courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var studentId = CurrentUserId();

        var enrolled = await _db.Coursestudents
            .FirstOrDefaultAsync(x => x.Courseid == courseId && x.Studentid == studentId);

        if (enrolled == null)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return Redirect("/student/courses/index");
        }

        _db.Coursestudents.Remove(enrolled);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã hủy đăng ký khóa học.";
        return Redirect("/student/courses/index");
    }
}