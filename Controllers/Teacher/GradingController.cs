using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/grading/[action]")]
public class GradingController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public GradingController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GradeEssay(int answerId, decimal score)
    {
        var teacherId = CurrentUserId();

        var answer = await _db.Studentanswers
            .Include(a => a.Attempt)
                .ThenInclude(at => at.Exam)
                    .ThenInclude(ex => ex.Course)
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Answerid == answerId);

        if (answer == null) return NotFound();
        if (answer.Attempt.Exam.Course.Teacherid != teacherId) return Forbid();

        if (answer.Question.Questiontype != "essay")
        {
            TempData["Error"] = "Chỉ chấm tay cho câu tự luận.";
            return RedirectToAction("Detail", "Attempts", new { id = answer.Attemptid });
        }

        if (score < 0 || score > answer.Question.Marks)
        {
            TempData["Error"] = $"Điểm phải nằm trong khoảng 0 đến {answer.Question.Marks}.";
            return RedirectToAction("Detail", "Attempts", new { id = answer.Attemptid });
        }

        answer.Score = score;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã lưu điểm câu tự luận.";
        return RedirectToAction("Detail", "Attempts", new { id = answer.Attemptid });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeAttempt(int attemptId)
    {
        var teacherId = CurrentUserId();

        var attempt = await _db.Examattempts
            .Include(a => a.Exam)
                .ThenInclude(e => e.Course)
            .Include(a => a.Studentanswers)
                .ThenInclude(sa => sa.Question)
            .FirstOrDefaultAsync(a => a.Attemptid == attemptId);

        if (attempt == null) return NotFound();
        if (attempt.Exam.Course.Teacherid != teacherId) return Forbid();

        if (attempt.Status == "inprogress")
        {
            TempData["Error"] = "Học sinh chưa submit bài.";
            return RedirectToAction("Detail", "Attempts", new { id = attemptId });
        }

        var ungradedEssay = attempt.Studentanswers.Any(sa =>
            sa.Question.Questiontype == "essay" && sa.Score == null);

        if (ungradedEssay)
        {
            TempData["Error"] = "Vẫn còn câu tự luận chưa chấm.";
            return RedirectToAction("Detail", "Attempts", new { id = attemptId });
        }

        var finalScore = attempt.Studentanswers.Sum(sa => sa.Score ?? 0);

        attempt.Totalscore = finalScore;
        attempt.Status = "graded";

        var existingResult = await _db.Examresults.FirstOrDefaultAsync(r => r.Attemptid == attemptId);
        if (existingResult == null)
        {
            _db.Examresults.Add(new OMS.Models.Entities.Examresult
            {
                Attemptid = attemptId,
                Finalscore = finalScore,
                Gradedby = teacherId,
                Gradedat = DateTime.Now
            });
        }
        else
        {
            existingResult.Finalscore = finalScore;
            existingResult.Gradedby = teacherId;
            existingResult.Gradedat = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã chấm xong và chốt kết quả bài thi.";
        return RedirectToAction("Detail", "Attempts", new { id = attemptId });
    }
}