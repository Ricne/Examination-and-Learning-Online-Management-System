using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Admin;
using System.Text.Json;

namespace OMS.Controllers.Admin;

[Authorize(Roles = "admin")]
[Route("admin/courses/[action]")]
public class CoursesController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    public CoursesController(OnlineLearningExamSystemContext db) => _db = db;

    private const string DraftKey = "COURSE_DRAFT";

    private void SaveDraftToTempData(CourseCreateVm vm)
    {
        var draft = new CourseCreateVm
        {
            CourseName = vm.CourseName,
            SubjectId = vm.SubjectId,
            TeacherId = vm.TeacherId,
            Description = vm.Description,
            IsActive = vm.IsActive
        };
        TempData[DraftKey] = JsonSerializer.Serialize(draft);
    }

    private CourseCreateVm? ReadDraftFromTempData()
    {
        if (TempData.TryGetValue(DraftKey, out var raw) && raw is string json && !string.IsNullOrWhiteSpace(json))
        {
            try { return JsonSerializer.Deserialize<CourseCreateVm>(json); }
            catch { return null; }
        }
        return null;
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.Subjects = await _db.Subjects
            .AsNoTracking()
            .Where(s => s.Isdeleted == false)
            .OrderBy(s => s.Subjectname)
            .ToListAsync();

        var teacherRoleId = await _db.Roles
            .AsNoTracking()
            .Where(r => r.Rolename == "teacher")
            .Select(r => r.Roleid)
            .FirstOrDefaultAsync();

        ViewBag.Teachers = await _db.Users
            .AsNoTracking()
            .Where(u => u.Roleid == teacherRoleId && u.Isactive == true)
            .OrderBy(u => u.Fullname)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showDeleted = false)
    {
        ViewData["Title"] = "Khóa học";
        ViewData["SubTitle"] = "Quản lý khóa học";
        ViewData["Badge"] = "Admin";

        var query = _db.Courses
            .AsNoTracking()
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .AsQueryable();

        if (!showDeleted)
            query = query.Where(c => c.Isdeleted == false);

        var list = await query
            .OrderByDescending(c => c.Createdat)
            .ToListAsync();

        ViewBag.ShowDeleted = showDeleted;
        return View("~/Views/Admin/Courses/Index.cshtml", list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? subjectId = null, int? teacherId = null)
    {
        ViewData["Title"] = "Khóa học";
        ViewData["SubTitle"] = "Tạo khóa học mới";
        ViewData["Badge"] = "Create";

        await LoadDropdownsAsync();

        var vm = ReadDraftFromTempData() ?? new CourseCreateVm();

        if (subjectId.HasValue) vm.SubjectId = subjectId.Value;
        if (teacherId.HasValue) vm.TeacherId = teacherId.Value;

        return View("~/Views/Admin/Courses/Create.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveDraft(CourseCreateVm vm, string redirectTo)
    {
        vm.CourseName = (vm.CourseName ?? "").Trim();
        vm.Description = (vm.Description ?? "").Trim();

        SaveDraftToTempData(vm);
        return Redirect(redirectTo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateVm vm)
    {
        ViewData["Title"] = "Khóa học";
        ViewData["SubTitle"] = "Tạo khóa học mới";
        ViewData["Badge"] = "Create";

        await LoadDropdownsAsync();

        vm.CourseName = (vm.CourseName ?? "").Trim();
        vm.Description = (vm.Description ?? "").Trim();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Courses/Create.cshtml", vm);

        var subjectOk = await _db.Subjects.AnyAsync(s => s.Subjectid == vm.SubjectId && s.Isdeleted == false);
        if (!subjectOk)
        {
            ModelState.AddModelError(nameof(vm.SubjectId), "Môn học không hợp lệ hoặc đã bị xóa.");
            return View("~/Views/Admin/Courses/Create.cshtml", vm);
        }

        var teacherOk = await _db.Users.AnyAsync(u => u.Userid == vm.TeacherId);
        if (!teacherOk)
        {
            ModelState.AddModelError(nameof(vm.TeacherId), "Giảng viên không hợp lệ.");
            return View("~/Views/Admin/Courses/Create.cshtml", vm);
        }

        var course = new Course
        {
            Coursename = vm.CourseName,
            Subjectid = vm.SubjectId,
            Teacherid = vm.TeacherId,
            Description = vm.Description,
            Isactive = vm.IsActive,
            Isdeleted = false,
            Deletedat = null,
            Createdat = DateTime.Now
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Tạo khóa học thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Khóa học";
        ViewData["SubTitle"] = "Cập nhật khóa học";
        ViewData["Badge"] = "Edit";

        await LoadDropdownsAsync();

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Courseid == id);
        if (course == null) return NotFound();

        var vm = new CourseEditVm
        {
            CourseId = course.Courseid,
            CourseName = course.Coursename,
            SubjectId = course.Subjectid,
            TeacherId = course.Teacherid,
            Description = course.Description,
            IsActive = course.Isactive = course.Isactive
        };

        return View("~/Views/Admin/Courses/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseEditVm vm)
    {
        ViewData["Title"] = "Khóa học";
        ViewData["SubTitle"] = "Cập nhật khóa học";
        ViewData["Badge"] = "Edit";

        await LoadDropdownsAsync();

        vm.CourseName = (vm.CourseName ?? "").Trim();
        vm.Description = (vm.Description ?? "").Trim();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Courses/Edit.cshtml", vm);

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Courseid == vm.CourseId);
        if (course == null) return NotFound();

        var subjectOk = await _db.Subjects.AnyAsync(s => s.Subjectid == vm.SubjectId && s.Isdeleted == false);
        if (!subjectOk)
        {
            ModelState.AddModelError(nameof(vm.SubjectId), "Môn học không hợp lệ hoặc đã bị xóa.");
            return View("~/Views/Admin/Courses/Edit.cshtml", vm);
        }

        var teacherOk = await _db.Users.AnyAsync(u => u.Userid == vm.TeacherId);
        if (!teacherOk)
        {
            ModelState.AddModelError(nameof(vm.TeacherId), "Giảng viên không hợp lệ.");
            return View("~/Views/Admin/Courses/Edit.cshtml", vm);
        }

        course.Coursename = vm.CourseName;
        course.Subjectid = vm.SubjectId;
        course.Teacherid = vm.TeacherId;
        course.Description = vm.Description;
        course.Isactive = vm.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật khóa học thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Courseid == id);
        if (course == null) return NotFound();

        if (course.Isdeleted == true)
        {
            TempData["Error"] = "Khóa học đang ở thùng rác, không thể bật/tắt.";
            return RedirectToAction(nameof(Index), new { showDeleted = true });
        }

        course.Isactive = !(course.Isactive = course.Isactive);
        await _db.SaveChangesAsync();

        TempData["Success"] = course.Isactive == true ? "Đã bật khóa học." : "Đã tắt khóa học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Courseid == id);
        if (course == null) return NotFound();

        course.Isdeleted = true;
        course.Deletedat = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã chuyển khóa học vào thùng rác.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Courseid == id);
        if (course == null) return NotFound();

        course.Isdeleted = false;
        course.Deletedat = null;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Khôi phục khóa học thành công.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }
}