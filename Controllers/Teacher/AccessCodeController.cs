using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/accesscode/[action]")]
public class AccessCodeController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public AccessCodeController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    private async Task<Exam?> GetTeacherExam(int teacherId, int examId)
    {
        return await _db.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e =>
                e.Examid == examId &&
                e.Isdeleted == false &&
                e.Course.Teacherid == teacherId);
    }

    private string GenerateCode(int length = 6)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Access Codes";
        ViewData["SubTitle"] = "Quản lý mã vào phòng thi";

        var teacherId = CurrentUserId();

        var exams = await _db.Exams.AsNoTracking()
            .Where(e => e.Isdeleted == false && e.Course.Teacherid == teacherId)
            .Include(e => e.Course)
            .OrderByDescending(e => e.Createdat)
            .ToListAsync();

        return View("~/Views/Teacher/AccessCode/Index.cshtml", exams);
    }

    [HttpGet]
    public async Task<IActionResult> Manage(int examId)
    {
        ViewData["Title"] = "Manage Access Code";
        ViewData["SubTitle"] = "Tạo / đóng mã vào phòng thi";

        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, examId);
        if (exam == null) return NotFound();

        var codes = await _db.Examaccesscodes
            .Where(x => x.Examid == examId)
            .OrderByDescending(x => x.Createdat)
            .ToListAsync();

        ViewBag.Exam = exam;
        return View("~/Views/Teacher/AccessCode/Manage.cshtml", codes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int examId)
    {
        var teacherId = CurrentUserId();
        var exam = await GetTeacherExam(teacherId, examId);
        if (exam == null) return NotFound();

        var activeCodes = await _db.Examaccesscodes
            .Where(x => x.Examid == examId && x.Isactive == true)
            .ToListAsync();

        foreach (var c in activeCodes)
        {
            c.Isactive = false;
        }

        string newCode;
        do
        {
            newCode = GenerateCode(6);
        }
        while (await _db.Examaccesscodes.AnyAsync(x => x.Accesscode == newCode && x.Isactive == true));

        var entity = new Examaccesscode
        {
            Examid = examId,
            Accesscode = newCode,
            Isactive = true,
            Createdby = teacherId,
            Createdat = DateTime.Now
        };

        _db.Examaccesscodes.Add(entity);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Đã tạo mã code mới: {newCode}";
        return RedirectToAction(nameof(Manage), new { examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int codeId)
    {
        var teacherId = CurrentUserId();

        var code = await _db.Examaccesscodes
            .Include(x => x.Exam)
            .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(x => x.Codeid == codeId);

        if (code == null) return NotFound();
        if (code.Exam.Course.Teacherid != teacherId) return Forbid();

        code.Isactive = false;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã đóng mã code.";
        return RedirectToAction(nameof(Manage), new { examId = code.Examid });
    }
}