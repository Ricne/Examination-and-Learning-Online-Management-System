using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Student;
using System.Security.Claims;

namespace OMS.Controllers.Student;

[Authorize(Roles = "student")]
[Route("student/examroom/[action]")]
public class ExamRoomController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public ExamRoomController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    private int CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    [HttpGet]
    public IActionResult EnterCode()
    {
        ViewData["Title"] = "Vào phòng thi";
        ViewData["SubTitle"] = "Nhập access code để bắt đầu bài thi";

        return View("~/Views/Student/ExamRoom/EnterCode.cshtml", new EnterCodeVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterCode(EnterCodeVm vm)
    {
        ViewData["Title"] = "Vào phòng thi";
        ViewData["SubTitle"] = "Nhập access code để bắt đầu bài thi";

        if (!ModelState.IsValid)
            return View("~/Views/Student/ExamRoom/EnterCode.cshtml", vm);

        var studentId = CurrentUserId();
        var code = vm.Code.Trim().ToUpper();

        var accessCode = await _db.Examaccesscodes
            .Include(x => x.Exam)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(x => x.Accesscode == code && x.Isactive == true);

        if (accessCode == null)
        {
            ModelState.AddModelError(nameof(vm.Code), "Access code không hợp lệ hoặc đã bị đóng.");
            return View("~/Views/Student/ExamRoom/EnterCode.cshtml", vm);
        }

        var exam = accessCode.Exam;

        if (exam.Isdeleted == true || exam.Ispublished != true)
        {
            ModelState.AddModelError(nameof(vm.Code), "Bài thi hiện không khả dụng.");
            return View("~/Views/Student/ExamRoom/EnterCode.cshtml", vm);
        }

        var enrolled = await _db.Coursestudents.AnyAsync(x =>
            x.Studentid == studentId && x.Courseid == exam.Courseid);

        if (!enrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return Redirect("/student/courses/available");
        }

        var now = DateTime.Now;
        if (exam.Starttime.HasValue && now < exam.Starttime.Value)
        {
            ModelState.AddModelError(nameof(vm.Code), "Bài thi chưa đến thời gian bắt đầu.");
            return View("~/Views/Student/ExamRoom/EnterCode.cshtml", vm);
        }

        if (exam.Endtime.HasValue && now > exam.Endtime.Value)
        {
            ModelState.AddModelError(nameof(vm.Code), "Bài thi đã hết thời gian làm bài.");
            return View("~/Views/Student/ExamRoom/EnterCode.cshtml", vm);
        }

        return Redirect($"/student/examroom/startexam?examId={exam.Examid}");
    }

    [HttpGet]
    public async Task<IActionResult> StartExam(int examId)
    {
        ViewData["Title"] = "Bắt đầu làm bài";
        ViewData["SubTitle"] = "Xác nhận bắt đầu bài thi";

        var studentId = CurrentUserId();

        var exam = await _db.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Examid == examId && e.Isdeleted == false);

        if (exam == null) return NotFound();

        var enrolled = await _db.Coursestudents.AnyAsync(x =>
            x.Studentid == studentId && x.Courseid == exam.Courseid);

        if (!enrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return Redirect("/student/courses/available");
        }

        if (exam.Ispublished != true)
        {
            TempData["Error"] = "Bài thi chưa được publish.";
            return Redirect("/student/dashboard/index");
        }

        var attemptCount = await _db.Examattempts.CountAsync(a =>
            a.Examid == examId && a.Studentid == studentId);

        if (exam.Maxattempts > 0 && attemptCount >= exam.Maxattempts)
        {
            TempData["Error"] = "Bạn đã vượt quá số lần làm bài cho phép.";
            return Redirect("/student/results/my");
        }

        var inprogress = await _db.Examattempts
            .FirstOrDefaultAsync(a => a.Examid == examId && a.Studentid == studentId && a.Status == "inprogress");

        if (inprogress != null)
        {
            return Redirect($"/student/examroom/doexam?attemptId={inprogress.Attemptid}");
        }

        var attempt = new Examattempt
        {
            Examid = examId,
            Studentid = studentId,
            Starttime = DateTime.Now,
            Status = "inprogress"
        };

        _db.Examattempts.Add(attempt);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã bắt đầu bài thi.";
        return Redirect($"/student/examroom/doexam?attemptId={attempt.Attemptid}");
    }

    [HttpGet]
    public async Task<IActionResult> DoExam(int attemptId)
    {
        ViewData["Title"] = "Làm bài thi";
        ViewData["SubTitle"] = "Hoàn thành các câu hỏi và nộp bài";
        ViewData["Badge"] = "Student";
        ViewData["Breadcrumb"] = "Student / ExamRoom / DoExam";

        var studentId = CurrentUserId();

        var attempt = await _db.Examattempts
            .Include(a => a.Exam)
                .ThenInclude(e => e.Questions)
                    .ThenInclude(q => q.Choices)
            .Include(a => a.Studentanswers)
            .FirstOrDefaultAsync(a => a.Attemptid == attemptId && a.Studentid == studentId);

        if (attempt == null) return NotFound();

        if (attempt.Status != "inprogress")
        {
            TempData["Error"] = "Bài thi này không còn ở trạng thái làm bài.";
            return Redirect("/student/results/my");
        }

        var now = DateTime.Now;
        var expireAtByDuration = attempt.Starttime.AddMinutes(attempt.Exam.Durationminutes);

        DateTime expireAt;
        if (attempt.Exam.Endtime.HasValue && attempt.Exam.Endtime.Value < expireAtByDuration)
            expireAt = attempt.Exam.Endtime.Value;
        else
            expireAt = expireAtByDuration;

        if (now >= expireAt)
        {
            foreach (var sa in attempt.Studentanswers)
            {
                if (sa.Questionid == 0) continue;
            }

            var studentAnswers = await _db.Studentanswers
                .Include(sa => sa.Question)
                .Where(sa => sa.Attemptid == attempt.Attemptid)
                .ToListAsync();

            foreach (var sa in studentAnswers)
            {
                if (sa.Question.Questiontype == "mcq" || sa.Question.Questiontype == "true_false")
                {
                    var correctChoice = await _db.Choices
                        .FirstOrDefaultAsync(c => c.Questionid == sa.Questionid && c.Iscorrect == true);

                    sa.Score = (correctChoice != null && sa.Selectedchoiceid == correctChoice.Choiceid)
                        ? sa.Question.Marks
                        : 0;
                }
            }

            attempt.Submittime = now;
            attempt.Status = "submitted";
            attempt.Totalscore = studentAnswers.Sum(x => x.Score ?? 0);

            await _db.SaveChangesAsync();

            TempData["Error"] = "Đã hết thời gian làm bài. Hệ thống đã tự nộp bài.";
            return Redirect("/student/results/my");
        }

        var vm = new DoExamVm
        {
            Attemptid = attempt.Attemptid,
            Examid = attempt.Examid,
            ExamTitle = attempt.Exam.Title,
            Durationminutes = attempt.Exam.Durationminutes,
            Starttime = attempt.Starttime,
            ExpireAt = expireAt,
            ServerNow = now,
            Questions = attempt.Exam.Questions
                .OrderBy(q => q.Questionid)
                .Select(q =>
                {
                    var existing = attempt.Studentanswers.FirstOrDefault(sa => sa.Questionid == q.Questionid);
                    return new DoExamQuestionVm
                    {
                        Questionid = q.Questionid,
                        Questioncontent = q.Questioncontent,
                        Questiontype = q.Questiontype,
                        Marks = q.Marks,
                        Selectedchoiceid = existing?.Selectedchoiceid,
                        Essayanswer = existing?.Essayanswer,
                        Choices = q.Choices
                            .OrderBy(c => c.Choiceid)
                            .Select(c => new DoExamChoiceVm
                            {
                                Choiceid = c.Choiceid,
                                Choicetext = c.Choicetext
                            }).ToList()
                    };
                }).ToList()
        };

        return View("~/Views/Student/ExamRoom/DoExam.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoExam(DoExamVm vm)
    {
        var studentId = CurrentUserId();

        var attempt = await _db.Examattempts
            .Include(a => a.Exam)
                .ThenInclude(e => e.Questions)
            .Include(a => a.Studentanswers)
            .FirstOrDefaultAsync(a => a.Attemptid == vm.Attemptid && a.Studentid == studentId);

        if (attempt == null) return NotFound();

        if (attempt.Status != "inprogress")
        {
            TempData["Error"] = "Bài thi này không còn ở trạng thái làm bài.";
            return Redirect("/student/results/my");
        }

        var now = DateTime.Now;
        var expireAtByDuration = attempt.Starttime.AddMinutes(attempt.Exam.Durationminutes);
        var expireAt = attempt.Exam.Endtime.HasValue && attempt.Exam.Endtime.Value < expireAtByDuration
            ? attempt.Exam.Endtime.Value
            : expireAtByDuration;

        if (now >= expireAt)
        {
            TempData["Error"] = "Đã hết thời gian làm bài, không thể lưu thêm.";
            return Redirect($"/student/examroom/doexam?attemptId={attempt.Attemptid}");
        }

        foreach (var q in vm.Questions)
        {
            var question = attempt.Exam.Questions.FirstOrDefault(x => x.Questionid == q.Questionid);
            if (question == null) continue;

            var answer = attempt.Studentanswers.FirstOrDefault(a => a.Questionid == q.Questionid);

            if (answer == null)
            {
                answer = new Studentanswer
                {
                    Attemptid = attempt.Attemptid,
                    Questionid = q.Questionid
                };
                _db.Studentanswers.Add(answer);
            }

            if (question.Questiontype == "essay")
            {
                answer.Selectedchoiceid = null;
                answer.Essayanswer = q.Essayanswer?.Trim();
            }
            else
            {
                answer.Selectedchoiceid = q.Selectedchoiceid;
                answer.Essayanswer = null;
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã lưu câu trả lời.";
        return Redirect($"/student/examroom/doexam?attemptId={attempt.Attemptid}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitExam(DoExamVm vm)
    {
        var studentId = CurrentUserId();

        var attempt = await _db.Examattempts
            .Include(a => a.Exam)
                .ThenInclude(e => e.Questions)
                    .ThenInclude(q => q.Choices)
            .Include(a => a.Studentanswers)
                .ThenInclude(sa => sa.Question)
            .FirstOrDefaultAsync(a => a.Attemptid == vm.Attemptid && a.Studentid == studentId);

        if (attempt == null) return NotFound();

        if (attempt.Status != "inprogress")
        {
            TempData["Error"] = "Bài thi này đã được nộp.";
            return Redirect("/student/results/my");
        }

        var now = DateTime.Now;
        var expireAtByDuration = attempt.Starttime.AddMinutes(attempt.Exam.Durationminutes);
        var expireAt = attempt.Exam.Endtime.HasValue && attempt.Exam.Endtime.Value < expireAtByDuration
            ? attempt.Exam.Endtime.Value
            : expireAtByDuration;

        // luôn lưu đáp án mới nhất trước khi submit
        foreach (var q in vm.Questions)
        {
            var question = attempt.Exam.Questions.FirstOrDefault(x => x.Questionid == q.Questionid);
            if (question == null) continue;

            var answer = attempt.Studentanswers.FirstOrDefault(a => a.Questionid == q.Questionid);

            if (answer == null)
            {
                answer = new Studentanswer
                {
                    Attemptid = attempt.Attemptid,
                    Questionid = q.Questionid
                };
                _db.Studentanswers.Add(answer);
                attempt.Studentanswers.Add(answer);
            }

            if (question.Questiontype == "essay")
            {
                answer.Selectedchoiceid = null;
                answer.Essayanswer = q.Essayanswer?.Trim();
            }
            else
            {
                answer.Selectedchoiceid = q.Selectedchoiceid;
                answer.Essayanswer = null;
            }
        }

        await _db.SaveChangesAsync();

        foreach (var sa in attempt.Studentanswers)
        {
            if (sa.Question.Questiontype == "mcq" || sa.Question.Questiontype == "true_false")
            {
                var correctChoice = await _db.Choices
                    .FirstOrDefaultAsync(c => c.Questionid == sa.Questionid && c.Iscorrect == true);

                sa.Score = (correctChoice != null && sa.Selectedchoiceid == correctChoice.Choiceid)
                    ? sa.Question.Marks
                    : 0;
            }
        }

        attempt.Submittime = now;
        attempt.Status = "submitted";
        attempt.Totalscore = attempt.Studentanswers.Sum(x => x.Score ?? 0);

        await _db.SaveChangesAsync();

        if (now >= expireAt)
            TempData["Success"] = "Đã hết thời gian làm bài. Hệ thống đã tự nộp bài.";
        else
            TempData["Success"] = "Nộp bài thành công.";

        return Redirect("/student/results/my");
    }
}