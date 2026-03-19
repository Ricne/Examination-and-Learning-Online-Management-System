using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Teacher;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/lessons/[action]")]
public class LessonsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    private readonly IWebHostEnvironment _env;

    public LessonsController(OnlineLearningExamSystemContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    private async Task<List<Course>> LoadTeacherCourses(int teacherId)
    {
        return await _db.Courses.AsNoTracking()
            .Where(c => c.Teacherid == teacherId && c.Isdeleted == false)
            .OrderBy(c => c.Coursename)
            .ToListAsync();
    }

    private async Task<bool> TeacherOwnsCourse(int teacherId, int courseId)
    {
        return await _db.Courses.AsNoTracking()
            .AnyAsync(c => c.Courseid == courseId && c.Teacherid == teacherId && c.Isdeleted == false);
    }

    private async Task<Lesson?> GetTeacherLesson(int teacherId, int lessonId)
    {
        return await _db.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l =>
                l.Lessonid == lessonId &&
                l.Isdeleted == false &&
                l.Course.Teacherid == teacherId);
    }

    private async Task<string?> SaveAttachmentAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var allowed = new[] { ".pdf", ".ppt", ".pptx", ".doc", ".docx" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowed.Contains(ext))
            throw new InvalidOperationException("Chỉ chấp nhận file pdf, ppt, pptx, doc, docx.");

        if (file.Length > 20 * 1024 * 1024)
            throw new InvalidOperationException("File tối đa 20MB.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", "lessons");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/lessons/{fileName}";
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId = null)
    {
        ViewData["Title"] = "Lessons";
        ViewData["SubTitle"] = "Quản lý bài giảng";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);
        ViewBag.CourseId = courseId;

        var query = _db.Lessons.AsNoTracking()
            .Include(l => l.Course)
            .Where(l => l.Isdeleted == false && l.Course.Teacherid == teacherId);

        if (courseId.HasValue)
            query = query.Where(l => l.Courseid == courseId.Value);

        var list = await query
            .OrderBy(l => l.Courseid)
            .ThenBy(l => l.Lessonorder)
            .ToListAsync();

        return View("~/Views/Teacher/Lessons/Index.cshtml", list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? courseId = null)
    {
        ViewData["Title"] = "Create Lesson";
        ViewData["SubTitle"] = "Tạo bài giảng mới";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        var vm = new LessonVm();

        if (courseId.HasValue)
        {
            vm.Courseid = courseId.Value;

            var nextOrder = await _db.Lessons
                .Where(l => l.Courseid == courseId.Value && l.Isdeleted == false)
                .Select(l => (int?)l.Lessonorder)
                .MaxAsync() ?? 0;

            vm.Lessonorder = nextOrder + 1;
        }
        else
        {
            vm.Lessonorder = 1;
        }

        return View("~/Views/Teacher/Lessons/Create.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonVm vm)
    {
        ViewData["Title"] = "Create Lesson";
        ViewData["SubTitle"] = "Tạo bài giảng mới";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Lessons/Create.cshtml", vm);

        if (!await TeacherOwnsCourse(teacherId, vm.Courseid))
        {
            ModelState.AddModelError(nameof(vm.Courseid), "Khóa học không hợp lệ hoặc không thuộc bạn.");
            return View("~/Views/Teacher/Lessons/Create.cshtml", vm);
        }

        var existsOrder = await _db.Lessons.AnyAsync(l =>
            l.Courseid == vm.Courseid &&
            l.Isdeleted == false &&
            l.Lessonorder == vm.Lessonorder);

        if (existsOrder)
        {
            ModelState.AddModelError(nameof(vm.Lessonorder), "Thứ tự bài học đã tồn tại trong khóa học này.");
            return View("~/Views/Teacher/Lessons/Create.cshtml", vm);
        }

        string? attachmentUrl = null;
        try
        {
            attachmentUrl = await SaveAttachmentAsync(vm.AttachmentFile);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(vm.AttachmentFile), ex.Message);
            return View("~/Views/Teacher/Lessons/Create.cshtml", vm);
        }

        var lesson = new Lesson
        {
            Courseid = vm.Courseid,
            Title = vm.Title.Trim(),
            Content = vm.Content,
            Videourl = vm.Videourl,
            Attachmenturl = attachmentUrl,
            Lessonorder = vm.Lessonorder,
            Ispublished = vm.Ispublished,
            Isdeleted = false,
            Deletedat = null,
            Createdat = DateTime.Now
        };

        _db.Lessons.Add(lesson);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(vm.Lessonorder), "Thứ tự bài học bị trùng trong khóa học này. Vui lòng chọn số khác.");
            return View("~/Views/Teacher/Lessons/Create.cshtml", vm);
        }

        TempData["Success"] = "Tạo bài giảng thành công.";
        return Redirect($"/teacher/lessons/index?courseId={vm.Courseid}");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Lesson";
        ViewData["SubTitle"] = "Chỉnh sửa bài giảng";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        var lesson = await GetTeacherLesson(teacherId, id);
        if (lesson == null) return NotFound();

        var vm = new LessonVm
        {
            Lessonid = lesson.Lessonid,
            Courseid = lesson.Courseid,
            Title = lesson.Title,
            Content = lesson.Content,
            Videourl = lesson.Videourl,
            Attachmenturl = lesson.Attachmenturl,
            Lessonorder = lesson.Lessonorder,
            Ispublished = lesson.Ispublished
        };

        return View("~/Views/Teacher/Lessons/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonVm vm)
    {
        ViewData["Title"] = "Edit Lesson";
        ViewData["SubTitle"] = "Chỉnh sửa bài giảng";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Lessons/Edit.cshtml", vm);

        var lesson = await GetTeacherLesson(teacherId, vm.Lessonid);
        if (lesson == null) return NotFound();

        if (!await TeacherOwnsCourse(teacherId, vm.Courseid))
        {
            ModelState.AddModelError(nameof(vm.Courseid), "Khóa học không hợp lệ hoặc không thuộc bạn.");
            return View("~/Views/Teacher/Lessons/Edit.cshtml", vm);
        }

        var existsOrder = await _db.Lessons.AnyAsync(l =>
            l.Lessonid != vm.Lessonid &&
            l.Courseid == vm.Courseid &&
            l.Isdeleted == false &&
            l.Lessonorder == vm.Lessonorder);

        if (existsOrder)
        {
            ModelState.AddModelError(nameof(vm.Lessonorder), "Thứ tự bài học đã tồn tại trong khóa học này.");
            return View("~/Views/Teacher/Lessons/Edit.cshtml", vm);
        }

        if (vm.AttachmentFile != null)
        {
            try
            {
                var newFile = await SaveAttachmentAsync(vm.AttachmentFile);
                if (!string.IsNullOrWhiteSpace(newFile))
                    lesson.Attachmenturl = newFile;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(vm.AttachmentFile), ex.Message);
                return View("~/Views/Teacher/Lessons/Edit.cshtml", vm);
            }
        }

        lesson.Courseid = vm.Courseid;
        lesson.Title = vm.Title.Trim();
        lesson.Content = vm.Content;
        lesson.Videourl = vm.Videourl;
        lesson.Lessonorder = vm.Lessonorder;
        lesson.Ispublished = vm.Ispublished;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(vm.Lessonorder), "Thứ tự bài học bị trùng trong khóa học này. Vui lòng chọn số khác.");
            return View("~/Views/Teacher/Lessons/Edit.cshtml", vm);
        }

        TempData["Success"] = "Cập nhật bài giảng thành công.";
        return Redirect($"/teacher/lessons/index?courseId={vm.Courseid}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var teacherId = CurrentUserId();
        var lesson = await GetTeacherLesson(teacherId, id);
        if (lesson == null) return NotFound();

        lesson.Ispublished = !lesson.Ispublished;
        await _db.SaveChangesAsync();

        TempData["Success"] = lesson.Ispublished ? "Đã publish bài giảng." : "Đã ẩn bài giảng.";
        return Redirect($"/teacher/lessons/index?courseId={lesson.Courseid}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var teacherId = CurrentUserId();
        var lesson = await GetTeacherLesson(teacherId, id);
        if (lesson == null) return NotFound();

        lesson.Isdeleted = true;
        lesson.Deletedat = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bài giảng.";
        return Redirect($"/teacher/lessons/index?courseId={lesson.Courseid}");
    }
}