using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;

namespace OMS.Controllers.Admin;

[Authorize(Roles = "admin")]
[Route("admin/subjects/[action]")]
public class SubjectsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    public SubjectsController(OnlineLearningExamSystemContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(bool showDeleted = false)
    {
        ViewData["Title"] = "Môn học";
        ViewData["SubTitle"] = "Quản lý môn học";

        var query = _db.Subjects.AsNoTracking().AsQueryable();

        if (!showDeleted)
            query = query.Where(s => s.Isdeleted == false);

        var subjects = await query
            .OrderByDescending(s => s.Createdat)
            .ToListAsync();

        ViewBag.ShowDeleted = showDeleted;
        return View("~/Views/Admin/Subjects/Index.cshtml", subjects);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Môn học";
        ViewData["SubTitle"] = "Thêm môn học mới";

        return View("~/Views/Admin/Subjects/Create.cshtml", new Subject());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subject model)
    {
        ViewData["Title"] = "Môn học";
        ViewData["SubTitle"] = "Thêm môn học mới";

        model.Subjectname = (model.Subjectname ?? "").Trim();
        model.Description = (model.Description ?? "").Trim();

        if (string.IsNullOrWhiteSpace(model.Subjectname))
            ModelState.AddModelError(nameof(Subject.Subjectname), "Tên môn học không được để trống.");

        var exists = await _db.Subjects.AnyAsync(s => s.Subjectname == model.Subjectname);
        if (exists)
            ModelState.AddModelError(nameof(Subject.Subjectname), "Tên môn học đã tồn tại.");

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Subjects/Create.cshtml", model);

        model.Isactive = true;
        model.Isdeleted = false;
        model.Deletedat = null;
        model.Createdat = DateTime.Now;

        _db.Subjects.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Tạo môn học thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Môn học";
        ViewData["SubTitle"] = "Cập nhật thông tin môn học";

        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Subjectid == id);
        if (subject == null) return NotFound();

        return View("~/Views/Admin/Subjects/Edit.cshtml", subject);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Subject model)
    {
        ViewData["Title"] = "Môn học";
        ViewData["SubTitle"] = "Cập nhật thông tin môn học";

        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Subjectid == model.Subjectid);
        if (subject == null) return NotFound();

        var name = (model.Subjectname ?? "").Trim();
        var desc = (model.Description ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError(nameof(Subject.Subjectname), "Tên môn học không được để trống.");

        var exists = await _db.Subjects.AnyAsync(s => s.Subjectid != model.Subjectid && s.Subjectname == name);
        if (exists)
            ModelState.AddModelError(nameof(Subject.Subjectname), "Tên môn học đã tồn tại.");

        if (!ModelState.IsValid)
        {
            model.Description = desc;
            model.Subjectname = name;
            return View("~/Views/Admin/Subjects/Edit.cshtml", model);
        }

        subject.Subjectname = name;
        subject.Description = desc;
        subject.Isactive = model.Isactive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật môn học thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Subjectid == id);
        if (subject == null) return NotFound();

        subject.Isdeleted = true;
        subject.Deletedat = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã chuyển môn học vào thùng rác.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Subjectid == id);
        if (subject == null) return NotFound();

        subject.Isdeleted = false;
        subject.Deletedat = null;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Khôi phục môn học thành công.";
        return RedirectToAction(nameof(Index), new { showDeleted = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Subjectid == id);
        if (subject == null) return NotFound();

        if (subject.Isdeleted == true)
        {
            TempData["Error"] = "Môn học đang ở thùng rác, không thể bật/tắt.";
            return RedirectToAction(nameof(Index), new { showDeleted = true });
        }

        subject.Isactive = !subject.Isactive;
        await _db.SaveChangesAsync();

        TempData["Success"] = subject.Isactive == true ? "Đã bật môn học." : "Đã tắt môn học.";
        return RedirectToAction(nameof(Index));
    }
}