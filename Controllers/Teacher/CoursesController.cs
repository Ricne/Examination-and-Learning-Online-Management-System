using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Teacher;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/courses/[action]")]
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
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Courses";
        ViewData["SubTitle"] = "Khóa học do tôi phụ trách";

        var teacherId = CurrentUserId();

        var courses = await _db.Courses.AsNoTracking()
            .Include(c => c.Subject)
            .Where(c => c.Teacherid == teacherId && c.Isdeleted == false)
            .OrderByDescending(c => c.Createdat)
            .ToListAsync();

        return View("~/Views/Teacher/Courses/Index.cshtml", courses);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Course";
        ViewData["SubTitle"] = "Chỉnh sửa khóa học của tôi";

        var teacherId = CurrentUserId();

        var course = await _db.Courses
            .Include(c => c.Subject)
            .FirstOrDefaultAsync(c =>
                c.Courseid == id &&
                c.Teacherid == teacherId &&
                c.Isdeleted == false);

        if (course == null) return NotFound();

        var vm = new TeacherCourseEditVm
        {
            Courseid = course.Courseid,
            Coursename = course.Coursename,
            Subjectid = course.Subjectid,
            Subjectname = course.Subject?.Subjectname ?? "",
            Description = course.Description,
            Isactive = course.Isactive = !course.Isactive
        };

        return View("~/Views/Teacher/Courses/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TeacherCourseEditVm vm)
    {
        ViewData["Title"] = "Edit Course";
        ViewData["SubTitle"] = "Chỉnh sửa khóa học của tôi";

        var teacherId = CurrentUserId();

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Courses/Edit.cshtml", vm);

        var course = await _db.Courses
            .Include(c => c.Subject)
            .FirstOrDefaultAsync(c =>
                c.Courseid == vm.Courseid &&
                c.Teacherid == teacherId &&
                c.Isdeleted == false);

        if (course == null) return NotFound();

        course.Coursename = vm.Coursename.Trim();
        course.Description = vm.Description;
        course.Isactive = vm.Isactive;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Cập nhật khóa học thành công.";

        return Redirect("/teacher/courses/index");
    }
}