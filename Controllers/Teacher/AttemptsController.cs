using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.ViewModels.Teacher;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/attempts/[action]")]
public class AttemptsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public AttemptsController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }
    private async Task<List<OMS.Models.Entities.Exam>> LoadTeacherExams(int teacherId)
    {
        return await _db.Exams.AsNoTracking()
            .Where(e => e.Isdeleted == false && e.Course.Teacherid == teacherId)
            .OrderByDescending(e => e.Createdat)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? examId = null, string? status = null)
    {
        ViewData["Title"] = "Attempts";
        ViewData["SubTitle"] = "Danh sách bài làm của học sinh";

        var teacherId = CurrentUserId();

        ViewBag.Exams = await LoadTeacherExams(teacherId);
        ViewBag.ExamId = examId;
        ViewBag.Status = status;

        var query = _db.Examattempts.AsNoTracking()
            .Where(a => a.Exam.Course.Teacherid == teacherId)
            .Select(a => new AttemptListItemVm
            {
                Attemptid = a.Attemptid,
                Examid = a.Examid,
                ExamTitle = a.Exam.Title,
                StudentName = a.Student.Fullname,
                Starttime = a.Starttime,
                Submittime = a.Submittime,
                Totalscore = a.Totalscore,
                Status = a.Status
            });

        if (examId.HasValue)
            query = query.Where(x => x.Examid == examId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        var list = await query
            .OrderByDescending(x => x.Submittime ?? x.Starttime)
            .ToListAsync();

        return View("~/Views/Teacher/Attempts/Index.cshtml", list);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        ViewData["Title"] = "Attempt Detail";
        ViewData["SubTitle"] = "Chi tiết bài làm";

        var teacherId = CurrentUserId();

        var attempt = await _db.Examattempts.AsNoTracking()
            .Where(a => a.Attemptid == id && a.Exam.Course.Teacherid == teacherId)
            .Select(a => new AttemptGradeVm
            {
                Attemptid = a.Attemptid,
                Examid = a.Examid,
                ExamTitle = a.Exam.Title,
                StudentName = a.Student.Fullname,
                Status = a.Status,
                Starttime = a.Starttime,
                Submittime = a.Submittime,
                Totalscore = a.Totalscore,
                Answers = a.Studentanswers
                    .OrderBy(sa => sa.Questionid)
                    .Select(sa => new AnswerGradeVm
                    {
                        Answerid = sa.Answerid,
                        Questionid = sa.Questionid,
                        Questioncontent = sa.Question.Questioncontent,
                        Questiontype = sa.Question.Questiontype,
                        Marks = sa.Question.Marks,
                        SelectedChoiceText = sa.Selectedchoice != null ? sa.Selectedchoice.Choicetext : null,
                        Essayanswer = sa.Essayanswer,
                        Score = sa.Score
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (attempt == null) return NotFound();

        return View("~/Views/Teacher/Attempts/Detail.cshtml", attempt);
    }
}