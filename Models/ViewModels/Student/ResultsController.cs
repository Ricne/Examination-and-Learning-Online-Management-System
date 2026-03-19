using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Student;

[Authorize(Roles = "student")]
[Route("student/results/[action]")]
public class ResultsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public ResultsController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> My()
    {
        ViewData["Title"] = "Kết quả của tôi";
        ViewData["SubTitle"] = "Danh sách bài thi đã làm";

        var studentId = CurrentUserId();

        var list = await _db.Examattempts.AsNoTracking()
            .Where(a => a.Studentid == studentId)
            .OrderByDescending(a => a.Submittime ?? a.Starttime)
            .Select(a => new
            {
                a.Attemptid,
                a.Examid,
                ExamTitle = a.Exam.Title,
                CourseName = a.Exam.Course.Coursename,
                a.Starttime,
                a.Submittime,
                a.Status,
                a.Totalscore,
                Finalscore = a.Examresult != null ? a.Examresult.Finalscore : (decimal?)null,
                Allowreview = a.Exam.Allowreview
            })
            .ToListAsync();

        return View("~/Views/Student/Results/My.cshtml", list);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int attemptId)
    {
        ViewData["Title"] = "Chi tiết kết quả";
        ViewData["SubTitle"] = "Thông tin bài thi";

        var studentId = CurrentUserId();

        var result = await _db.Examattempts.AsNoTracking()
            .Where(a => a.Attemptid == attemptId && a.Studentid == studentId)
            .Select(a => new
            {
                a.Attemptid,
                a.Examid,
                ExamTitle = a.Exam.Title,
                CourseName = a.Exam.Course.Coursename,
                a.Status,
                a.Starttime,
                a.Submittime,
                a.Totalscore,
                Finalscore = a.Examresult != null ? a.Examresult.Finalscore : (decimal?)null,
                Gradedat = a.Examresult != null ? a.Examresult.Gradedat : (DateTime?)null,
                Allowreview = a.Exam.Allowreview
            })
            .FirstOrDefaultAsync();

        if (result == null) return NotFound();

        return View("~/Views/Student/Results/Detail.cshtml", result);
    }
}