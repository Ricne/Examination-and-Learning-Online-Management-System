using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.ViewModels.Teacher;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/exambuilder/[action]")]
public class ExamBuilderController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    public ExamBuilderController(OnlineLearningExamSystemContext db) => _db = db;

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    private async Task<Models.Entities.Exam?> GetTeacherExam(int teacherId, int examId)
    {
        return await _db.Exams
            .Include(e => e.Course)
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Examid == examId && e.Isdeleted == false && e.Course.Teacherid == teacherId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int examId)
    {
        ViewData["Title"] = "Exam Builder";
        ViewData["SubTitle"] = "Thêm câu hỏi vào đề thi";

        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, examId);
        if (exam == null) return NotFound();

        var bankQuestions = await _db.Questions.AsNoTracking()
            .Where(q => q.Isdeleted == false && q.Courseid == exam.Courseid)
            .OrderByDescending(q => q.Createdat)
            .ToListAsync();

        var selectedIds = exam.Questions.Select(q => q.Questionid).ToHashSet();

        ViewBag.Exam = exam;
        ViewBag.SelectedIds = selectedIds;

        return View("~/Views/Teacher/ExamBuilder/Index.cshtml", bankQuestions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int examId, int questionId)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, examId);
        if (exam == null) return NotFound();

        var q = await _db.Questions.FirstOrDefaultAsync(x =>
            x.Questionid == questionId &&
            x.Courseid == exam.Courseid &&
            x.Isdeleted == false);

        if (q == null) return NotFound();

        var exists = await _db.Exams
            .Where(e => e.Examid == examId)
            .SelectMany(e => e.Questions)
            .AnyAsync(x => x.Questionid == questionId);

        if (!exists)
        {
            exam.Questions.Add(q);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm câu hỏi vào đề.";
        }

        return RedirectToAction(nameof(Index), new { examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuestion(int examId, int questionId)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, examId);
        if (exam == null) return NotFound();

        var q = exam.Questions.FirstOrDefault(x => x.Questionid == questionId);
        if (q != null)
        {
            exam.Questions.Remove(q);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa câu hỏi khỏi đề.";
        }

        return RedirectToAction(nameof(Index), new { examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RandomAdd(ExamBuilderRandomVm vm)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, vm.Examid);
        if (exam == null) return NotFound();

        if (vm.NumberOfQuestions <= 0)
        {
            TempData["Error"] = "Số lượng câu hỏi phải > 0.";
            return RedirectToAction(nameof(Index), new { examId = vm.Examid });
        }

        var selectedIds = exam.Questions.Select(q => q.Questionid).ToHashSet();

        var candidates = await _db.Questions
            .Where(q => q.Isdeleted == false &&
                        q.Courseid == exam.Courseid &&
                        !selectedIds.Contains(q.Questionid))
            .OrderBy(q => Guid.NewGuid())
            .Take(vm.NumberOfQuestions)
            .ToListAsync();

        foreach (var q in candidates)
            exam.Questions.Add(q);

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã random thêm {candidates.Count} câu hỏi.";

        return RedirectToAction(nameof(Index), new { examId = vm.Examid });
    }
}