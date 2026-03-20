using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Teacher;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/exams/[action]")]
public class ExamsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    public ExamsController(OnlineLearningExamSystemContext db) => _db = db;

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

    private async Task<Exam?> GetTeacherExam(int teacherId, int examId)
    {
        return await _db.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Examid == examId && e.Isdeleted == false && e.Course.Teacherid == teacherId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId = null)
    {
        ViewData["Title"] = "Exams";
        ViewData["SubTitle"] = "Quản lý đề thi";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);
        ViewBag.CourseId = courseId;

        var query = _db.Exams.AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.Isdeleted == false && e.Course.Teacherid == teacherId);

        if (courseId.HasValue)
            query = query.Where(e => e.Courseid == courseId.Value);

        var list = await query
            .OrderByDescending(e => e.Createdat)
            .ToListAsync();

        return View("~/Views/Teacher/Exams/Index.cshtml", list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create Exam";
        ViewData["SubTitle"] = "Tạo đề thi mới";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        return View("~/Views/Teacher/Exams/Create.cshtml", new ExamCreateVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamCreateVm vm)
    {
        ViewData["Title"] = "Create Exam";
        ViewData["SubTitle"] = "Tạo đề thi mới";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Exams/Create.cshtml", vm);

        var courseOk = await _db.Courses.AnyAsync(c =>
            c.Courseid == vm.Courseid &&
            c.Teacherid == teacherId &&
            c.Isdeleted == false);

        if (!courseOk)
        {
            ModelState.AddModelError(nameof(vm.Courseid), "Khóa học không hợp lệ hoặc không thuộc bạn.");
            return View("~/Views/Teacher/Exams/Create.cshtml", vm);
        }

        if (vm.Starttime.HasValue && vm.Endtime.HasValue && vm.Starttime >= vm.Endtime)
        {
            ModelState.AddModelError("", "StartTime phải nhỏ hơn EndTime.");
            return View("~/Views/Teacher/Exams/Create.cshtml", vm);
        }

        if (vm.Ispublished)
        {
            ModelState.AddModelError(nameof(vm.Ispublished), "Chưa thể publish đề thi khi chưa thêm câu hỏi. Hãy tạo đề trước, thêm câu hỏi rồi publish sau.");
            return View("~/Views/Teacher/Exams/Create.cshtml", vm);
        }

        var exam = new Exam
        {
            Courseid = vm.Courseid,
            Title = vm.Title.Trim(),
            Durationminutes = vm.Durationminutes,
            Starttime = vm.Starttime,
            Endtime = vm.Endtime,
            Totalmarks = 0,
            Maxattempts = vm.Maxattempts,
            Ispublished = false, 
            Allowreview = vm.Allowreview,
            Createdby = teacherId,
            Createdat = DateTime.Now,
            Isdeleted = false,
            Deletedat = null
        };

        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Tạo đề thi thành công. Hãy thêm câu hỏi trước khi publish.";
        return Redirect($"/teacher/exambuilder/index?examId={exam.Examid}");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Exam";
        ViewData["SubTitle"] = "Chỉnh sửa đề thi";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        var exam = await GetTeacherExam(teacherId, id);
        if (exam == null) return NotFound();

        var vm = new ExamEditVm
        {
            Examid = exam.Examid,
            Courseid = exam.Courseid,
            Title = exam.Title,
            Durationminutes = exam.Durationminutes,
            Starttime = exam.Starttime,
            Endtime = exam.Endtime,
            Maxattempts = exam.Maxattempts,
            Ispublished = exam.Ispublished,
            Allowreview = exam.Allowreview
        };

        return View("~/Views/Teacher/Exams/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ExamEditVm vm)
    {
        ViewData["Title"] = "Edit Exam";
        ViewData["SubTitle"] = "Chỉnh sửa đề thi";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Exams/Edit.cshtml", vm);

        var exam = await GetTeacherExam(teacherId, vm.Examid);
        if (exam == null) return NotFound();

        var courseOk = await _db.Courses.AnyAsync(c =>
            c.Courseid == vm.Courseid &&
            c.Teacherid == teacherId &&
            c.Isdeleted == false);

        if (!courseOk)
        {
            ModelState.AddModelError(nameof(vm.Courseid), "Khóa học không hợp lệ hoặc không thuộc bạn.");
            return View("~/Views/Teacher/Exams/Edit.cshtml", vm);
        }

        if (vm.Starttime.HasValue && vm.Endtime.HasValue && vm.Starttime >= vm.Endtime)
        {
            ModelState.AddModelError("", "StartTime phải nhỏ hơn EndTime.");
            return View("~/Views/Teacher/Exams/Edit.cshtml", vm);
        }

        var questionCount = await _db.Exams
            .Where(e => e.Examid == vm.Examid)
            .SelectMany(e => e.Questions)
            .CountAsync();

        if (vm.Ispublished && questionCount == 0)
        {
            ModelState.AddModelError(nameof(vm.Ispublished), "Không thể publish đề thi khi chưa có câu hỏi.");
            return View("~/Views/Teacher/Exams/Edit.cshtml", vm);
        }

        exam.Courseid = vm.Courseid;
        exam.Title = vm.Title.Trim();
        exam.Durationminutes = vm.Durationminutes;
        exam.Starttime = vm.Starttime;
        exam.Endtime = vm.Endtime;
        exam.Maxattempts = vm.Maxattempts;
        exam.Ispublished = vm.Ispublished;
        exam.Allowreview = vm.Allowreview;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể cập nhật đề thi. Nếu muốn publish, hãy chắc chắn đề đã có câu hỏi và tổng điểm > 0.");
            return View("~/Views/Teacher/Exams/Edit.cshtml", vm);
        }

        TempData["Success"] = "Cập nhật đề thi thành công.";
        return Redirect("/teacher/exams/index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, id);
        if (exam == null) return NotFound();

        if (!exam.Ispublished)
        {
            var questionCount = await _db.Exams
                .Where(e => e.Examid == id)
                .SelectMany(e => e.Questions)
                .CountAsync();

            if (questionCount == 0 || exam.Totalmarks <= 0)
            {
                TempData["Error"] = "Không thể publish đề thi khi chưa có câu hỏi hoặc tổng điểm chưa hợp lệ.";
                return Redirect("/teacher/exams/index");
            }
        }

        exam.Ispublished = !exam.Ispublished;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể thay đổi trạng thái publish. Hãy kiểm tra đề đã có câu hỏi và tổng điểm > 0.";
            return Redirect("/teacher/exams/index");
        }

        TempData["Success"] = exam.Ispublished ? "Đã publish đề thi." : "Đã unpublish đề thi.";
        return Redirect("/teacher/exams/index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleReview(int id)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, id);
        if (exam == null) return NotFound();

        exam.Allowreview = !exam.Allowreview;
        await _db.SaveChangesAsync();

        TempData["Success"] = exam.Allowreview ? "Đã bật review đáp án." : "Đã tắt review đáp án.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, id);
        if (exam == null) return NotFound();

        exam.Isdeleted = true;
        exam.Deletedat = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa đề thi.";
        return RedirectToAction(nameof(Index));
    }
}