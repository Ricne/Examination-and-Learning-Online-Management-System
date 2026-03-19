using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Student;

[Authorize(Roles = "student")]
[Route("student/lessons/[action]")]
public class LessonsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public LessonsController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId = null)
    {
        ViewData["Title"] = "Bài giảng";
        ViewData["SubTitle"] = "Danh sách bài giảng đã publish";

        var studentId = CurrentUserId();

        var myCourses = await _db.Coursestudents.AsNoTracking()
            .Where(x => x.Studentid == studentId)
            .Select(x => x.Course)
            .OrderBy(c => c.Coursename)
            .ToListAsync();

        ViewBag.Courses = myCourses;
        ViewBag.CourseId = courseId;

        var query = _db.Lessons.AsNoTracking()
            .Where(l =>
                l.Isdeleted == false &&
                l.Ispublished == true &&
                l.Course.Coursestudents.Any(cs => cs.Studentid == studentId))
            .Include(l => l.Course)
            .AsQueryable();

        if (courseId.HasValue)
            query = query.Where(l => l.Courseid == courseId.Value);

        var lessons = await query
            .OrderBy(l => l.Courseid)
            .ThenBy(l => l.Lessonorder)
            .Select(l => new
            {
                l.Lessonid,
                l.Courseid,
                l.Title,
                l.Content,
                l.Videourl,
                l.Attachmenturl,
                l.Lessonorder,
                CourseName = l.Course.Coursename,
                IsCompleted = l.Lessonprogresses.Any(lp => lp.Studentid == studentId && lp.Iscompleted == true)
            })
            .ToListAsync();

        return View("~/Views/Student/Lessons/Index.cshtml", lessons);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        ViewData["Title"] = "Học bài";
        ViewData["SubTitle"] = "Chi tiết bài giảng";

        var studentId = CurrentUserId();

        var lesson = await _db.Lessons.AsNoTracking()
            .Where(l =>
                l.Lessonid == id &&
                l.Isdeleted == false &&
                l.Ispublished == true &&
                l.Course.Coursestudents.Any(cs => cs.Studentid == studentId))
            .Select(l => new
            {
                l.Lessonid,
                l.Courseid,
                l.Title,
                l.Content,
                l.Videourl,
                l.Attachmenturl,
                l.Lessonorder,
                CourseName = l.Course.Coursename,
                IsCompleted = l.Lessonprogresses.Any(lp => lp.Studentid == studentId && lp.Iscompleted == true)
            })
            .FirstOrDefaultAsync();

        if (lesson == null) return NotFound();

        return View("~/Views/Student/Lessons/Detail.cshtml", lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkComplete(int lessonId)
    {
        var studentId = CurrentUserId();

        var lesson = await _db.Lessons
            .FirstOrDefaultAsync(l =>
                l.Lessonid == lessonId &&
                l.Isdeleted == false &&
                l.Ispublished == true &&
                l.Course.Coursestudents.Any(cs => cs.Studentid == studentId));

        if (lesson == null) return NotFound();

        var progress = await _db.Lessonprogresses
            .FirstOrDefaultAsync(lp => lp.Lessonid == lessonId && lp.Studentid == studentId);

        if (progress == null)
        {
            progress = new OMS.Models.Entities.Lessonprogress
            {
                Lessonid = lessonId,
                Studentid = studentId,
                Iscompleted = true,
                Completedat = DateTime.Now
            };
            _db.Lessonprogresses.Add(progress);
        }
        else
        {
            progress.Iscompleted = true;
            progress.Completedat = DateTime.Now;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã đánh dấu hoàn thành bài học.";
        return RedirectToAction(nameof(Detail), new { id = lessonId });
    }
}