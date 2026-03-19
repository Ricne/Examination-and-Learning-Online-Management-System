using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using System.Security.Claims;

namespace OMS.Controllers.Student;

[Authorize(Roles = "student")]
[Route("student/review/[action]")]
public class ReviewController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public ReviewController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int attemptId)
    {
        ViewData["Title"] = "Review bài thi";
        ViewData["SubTitle"] = "Xem đáp án và kết quả chi tiết";

        var studentId = CurrentUserId();

        var attempt = await _db.Examattempts.AsNoTracking()
            .Where(a => a.Attemptid == attemptId && a.Studentid == studentId)
            .Select(a => new
            {
                a.Attemptid,
                a.Examid,
                ExamTitle = a.Exam.Title,
                CourseName = a.Exam.Course.Coursename,
                a.Status,
                a.Totalscore,
                Finalscore = a.Examresult != null ? a.Examresult.Finalscore : (decimal?)null,
                Allowreview = a.Exam.Allowreview,
                Answers = a.Studentanswers
                    .OrderBy(sa => sa.Questionid)
                    .Select(sa => new
                    {
                        sa.Answerid,
                        sa.Questionid,
                        Questioncontent = sa.Question.Questioncontent,
                        Questiontype = sa.Question.Questiontype,
                        Marks = sa.Question.Marks,
                        SelectedChoiceId = sa.Selectedchoiceid,
                        SelectedChoiceText = sa.Selectedchoice != null ? sa.Selectedchoice.Choicetext : null,
                        Essayanswer = sa.Essayanswer,
                        Score = sa.Score,
                        CorrectChoices = sa.Question.Choices
                            .Where(c => c.Iscorrect == true)
                            .OrderBy(c => c.Choiceid)
                            .Select(c => new
                            {
                                c.Choiceid,
                                c.Choicetext
                            }).ToList(),
                        AllChoices = sa.Question.Choices
                            .OrderBy(c => c.Choiceid)
                            .Select(c => new
                            {
                                c.Choiceid,
                                c.Choicetext,
                                c.Iscorrect
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (attempt == null) return NotFound();

        if (attempt.Allowreview != true)
        {
            TempData["Error"] = "Bài thi này chưa được giáo viên cho phép review.";
            return Redirect($"/student/results/detail?attemptId={attemptId}");
        }

        return View("~/Views/Student/Review/Detail.cshtml", attempt);
    }
}