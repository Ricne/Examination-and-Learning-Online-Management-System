using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Teacher;
using System.Security.Claims;

namespace OMS.Controllers.Teacher;

[Authorize(Roles = "teacher")]
[Route("teacher/questions/[action]")]
public class QuestionsController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    private readonly IWebHostEnvironment _env;

    public QuestionsController(OnlineLearningExamSystemContext db, IWebHostEnvironment env)
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

    private async Task<Question?> GetTeacherQuestionById(int teacherId, int questionId)
    {
        return await _db.Questions
            .Include(q => q.Course)
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Questionid == questionId && q.Isdeleted == false && q.Course.Teacherid == teacherId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId = null)
    {
        ViewData["Title"] = "Question Bank";
        ViewData["SubTitle"] = "Ngân hàng câu hỏi";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);
        ViewBag.CourseId = courseId;

        var query = _db.Questions.AsNoTracking()
            .Where(q => q.Isdeleted == false)
            .Include(q => q.Course)
            .Where(q => q.Course.Teacherid == teacherId);

        if (courseId.HasValue)
            query = query.Where(q => q.Courseid == courseId.Value);

        var list = await query.OrderByDescending(q => q.Createdat).ToListAsync();
        return View("~/Views/Teacher/Questions/Index.cshtml", list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create Question";
        ViewData["SubTitle"] = "Tạo câu hỏi thủ công";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        var vm = new QuestionEditVm
        {
            Questiontype = "mcq",
            Marks = 1
        };

        return View("~/Views/Teacher/Questions/Create.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionEditVm vm)
    {
        ViewData["Title"] = "Create Question";
        ViewData["SubTitle"] = "Tạo câu hỏi thủ công";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (vm.Questiontype != "mcq" && vm.Questiontype != "true_false" && vm.Questiontype != "essay")
            ModelState.AddModelError(nameof(vm.Questiontype), "Type phải là mcq/true_false/essay.");

        if (vm.Marks <= 0)
            ModelState.AddModelError(nameof(vm.Marks), "Marks phải > 0.");

        if (!await TeacherOwnsCourse(teacherId, vm.Courseid))
            ModelState.AddModelError(nameof(vm.Courseid), "Course không hợp lệ hoặc không thuộc bạn.");

        if (vm.Questiontype == "mcq")
        {
            if (string.IsNullOrWhiteSpace(vm.ChoiceA) || string.IsNullOrWhiteSpace(vm.ChoiceB))
                ModelState.AddModelError("", "MCQ phải có ít nhất Choice A và Choice B.");

            if (!(vm.Correct == "A" || vm.Correct == "B" || vm.Correct == "C" || vm.Correct == "D"))
                ModelState.AddModelError(nameof(vm.Correct), "Correct phải là A/B/C/D.");
        }
        else if (vm.Questiontype == "true_false")
        {
            if (!(vm.Correct == "TRUE" || vm.Correct == "FALSE"))
                ModelState.AddModelError(nameof(vm.Correct), "Correct phải là TRUE/FALSE.");
        }
        else
        {
            vm.Correct = null;
            vm.ChoiceA = vm.ChoiceB = vm.ChoiceC = vm.ChoiceD = null;
        }

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Questions/Create.cshtml", vm);

        var q = new Question
        {
            Courseid = vm.Courseid,
            Questiontype = vm.Questiontype,
            Questioncontent = vm.Questioncontent.Trim(),
            Marks = vm.Marks,
            Createdby = teacherId,
            Isdeleted = false,
            Deletedat = null,
            Createdat = DateTime.Now
        };

        _db.Questions.Add(q);
        await _db.SaveChangesAsync();

        if (vm.Questiontype == "mcq")
        {
            var list = new List<(string txt, string key)>
            {
                (vm.ChoiceA!.Trim(), "A"),
                (vm.ChoiceB!.Trim(), "B"),
            };

            if (!string.IsNullOrWhiteSpace(vm.ChoiceC)) list.Add((vm.ChoiceC.Trim(), "C"));
            if (!string.IsNullOrWhiteSpace(vm.ChoiceD)) list.Add((vm.ChoiceD.Trim(), "D"));

            foreach (var (txt, key) in list)
            {
                _db.Choices.Add(new Choice
                {
                    Questionid = q.Questionid,
                    Choicetext = txt,
                    Iscorrect = (vm.Correct == key)
                });
            }
        }
        else if (vm.Questiontype == "true_false")
        {
            _db.Choices.Add(new Choice { Questionid = q.Questionid, Choicetext = "TRUE", Iscorrect = (vm.Correct == "TRUE") });
            _db.Choices.Add(new Choice { Questionid = q.Questionid, Choicetext = "FALSE", Iscorrect = (vm.Correct == "FALSE") });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Tạo câu hỏi thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        ViewData["Title"] = "Question Detail";
        ViewData["SubTitle"] = "Chi tiết câu hỏi";

        var teacherId = CurrentUserId();
        var q = await GetTeacherQuestionById(teacherId, id);
        if (q == null) return NotFound();

        return View("~/Views/Teacher/Questions/Detail.cshtml", q);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Question";
        ViewData["SubTitle"] = "Chỉnh sửa câu hỏi trong ngân hàng";

        var teacherId = CurrentUserId();
        var q = await GetTeacherQuestionById(teacherId, id);
        if (q == null) return NotFound();

        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        var vm = new QuestionEditVm
        {
            Questionid = q.Questionid,
            Courseid = q.Courseid,
            Questiontype = q.Questiontype,
            Questioncontent = q.Questioncontent,
            Marks = q.Marks
        };

        if (q.Questiontype == "mcq")
        {
            var ordered = q.Choices.OrderBy(c => c.Choiceid).ToList();
            vm.ChoiceA = ordered.ElementAtOrDefault(0)?.Choicetext;
            vm.ChoiceB = ordered.ElementAtOrDefault(1)?.Choicetext;
            vm.ChoiceC = ordered.ElementAtOrDefault(2)?.Choicetext;
            vm.ChoiceD = ordered.ElementAtOrDefault(3)?.Choicetext;

            var correctChoice = q.Choices.FirstOrDefault(c => c.Iscorrect);
            if (correctChoice != null)
            {
                var idx = ordered.FindIndex(x => x.Choiceid == correctChoice.Choiceid);
                vm.Correct = idx switch { 0 => "A", 1 => "B", 2 => "C", 3 => "D", _ => null };
            }
        }
        else if (q.Questiontype == "true_false")
        {
            var t = q.Choices.FirstOrDefault(c => c.Choicetext.ToUpper() == "TRUE");
            var f = q.Choices.FirstOrDefault(c => c.Choicetext.ToUpper() == "FALSE");
            if (t != null && t.Iscorrect) vm.Correct = "TRUE";
            if (f != null && f.Iscorrect) vm.Correct = "FALSE";
        }

        return View("~/Views/Teacher/Questions/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuestionEditVm vm)
    {
        ViewData["Title"] = "Edit Question";
        ViewData["SubTitle"] = "Chỉnh sửa câu hỏi trong ngân hàng";

        var teacherId = CurrentUserId();
        var q = await GetTeacherQuestionById(teacherId, vm.Questionid);
        if (q == null) return NotFound();

        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (vm.Questiontype != "mcq" && vm.Questiontype != "true_false" && vm.Questiontype != "essay")
            ModelState.AddModelError(nameof(vm.Questiontype), "Type phải là mcq/true_false/essay.");

        if (vm.Marks <= 0)
            ModelState.AddModelError(nameof(vm.Marks), "Marks phải > 0.");

        if (!await TeacherOwnsCourse(teacherId, vm.Courseid))
            ModelState.AddModelError(nameof(vm.Courseid), "Course không hợp lệ hoặc không thuộc bạn.");

        if (vm.Questiontype == "mcq")
        {
            if (string.IsNullOrWhiteSpace(vm.ChoiceA) || string.IsNullOrWhiteSpace(vm.ChoiceB))
                ModelState.AddModelError("", "MCQ phải có ít nhất Choice A và Choice B.");

            if (!(vm.Correct == "A" || vm.Correct == "B" || vm.Correct == "C" || vm.Correct == "D"))
                ModelState.AddModelError(nameof(vm.Correct), "Correct phải là A/B/C/D.");
        }
        else if (vm.Questiontype == "true_false")
        {
            if (!(vm.Correct == "TRUE" || vm.Correct == "FALSE"))
                ModelState.AddModelError(nameof(vm.Correct), "Correct phải là TRUE/FALSE.");
        }
        else
        {
            vm.Correct = null;
            vm.ChoiceA = vm.ChoiceB = vm.ChoiceC = vm.ChoiceD = null;
        }

        if (!ModelState.IsValid)
            return View("~/Views/Teacher/Questions/Edit.cshtml", vm);

        q.Courseid = vm.Courseid;
        q.Questiontype = vm.Questiontype;
        q.Questioncontent = vm.Questioncontent.Trim();
        q.Marks = vm.Marks;

        if (q.Choices.Any())
            _db.Choices.RemoveRange(q.Choices);

        if (vm.Questiontype == "mcq")
        {
            var list = new List<(string txt, string key)>
            {
                (vm.ChoiceA!.Trim(), "A"),
                (vm.ChoiceB!.Trim(), "B"),
            };

            if (!string.IsNullOrWhiteSpace(vm.ChoiceC)) list.Add((vm.ChoiceC.Trim(), "C"));
            if (!string.IsNullOrWhiteSpace(vm.ChoiceD)) list.Add((vm.ChoiceD.Trim(), "D"));

            foreach (var (txt, key) in list)
            {
                _db.Choices.Add(new Choice
                {
                    Questionid = q.Questionid,
                    Choicetext = txt,
                    Iscorrect = (vm.Correct == key)
                });
            }
        }
        else if (vm.Questiontype == "true_false")
        {
            _db.Choices.Add(new Choice { Questionid = q.Questionid, Choicetext = "TRUE", Iscorrect = (vm.Correct == "TRUE") });
            _db.Choices.Add(new Choice { Questionid = q.Questionid, Choicetext = "FALSE", Iscorrect = (vm.Correct == "FALSE") });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật câu hỏi thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var teacherId = CurrentUserId();

        var q = await _db.Questions
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Questionid == id && x.Isdeleted == false && x.Course.Teacherid == teacherId);

        if (q == null) return NotFound();

        q.Isdeleted = true;
        q.Deletedat = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa câu hỏi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Import()
    {
        ViewData["Title"] = "Import Excel";
        ViewData["SubTitle"] = "Nhập câu hỏi hàng loạt từ file Excel";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        return View("~/Views/Teacher/Questions/Import.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file, int? selectedCourseId)
    {
        ViewData["Title"] = "Import Excel";
        ViewData["SubTitle"] = "Nhập câu hỏi hàng loạt từ file Excel";

        var teacherId = CurrentUserId();
        ViewBag.Courses = await LoadTeacherCourses(teacherId);

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file Excel.";
            return View("~/Views/Teacher/Questions/Import.cshtml");
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
        {
            TempData["Error"] = "Chỉ hỗ trợ file .xlsx";
            return View("~/Views/Teacher/Questions/Import.cshtml");
        }

        if (selectedCourseId.HasValue)
        {
            var ok = await TeacherOwnsCourse(teacherId, selectedCourseId.Value);
            if (!ok)
            {
                TempData["Error"] = "Khóa học đã chọn không hợp lệ hoặc không thuộc bạn.";
                return View("~/Views/Teacher/Questions/Import.cshtml");
            }
        }

        var folder = Path.Combine(_env.WebRootPath, "uploads", "imports", "questions");
        Directory.CreateDirectory(folder);
        var savedName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.xlsx";
        var savedPath = Path.Combine(folder, savedName);

        using (var stream = System.IO.File.Create(savedPath))
        {
            await file.CopyToAsync(stream);
        }

        var result = new QuestionImportResultVm();

        using var wb = new XLWorkbook(savedPath);
        var ws = wb.Worksheets.First();

        string[] headers = { "CourseId", "QuestionType", "QuestionContent", "Marks", "ChoiceA", "ChoiceB", "ChoiceC", "ChoiceD", "Correct", "Note" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1).GetString().Trim();
            if (!string.Equals(cell, headers[i], StringComparison.OrdinalIgnoreCase))
                result.Errors.Add($"Header sai tại cột {(char)('A' + i)}: cần '{headers[i]}' nhưng thấy '{cell}'.");
        }

        if (result.Errors.Any())
        {
            result.Failed = 1;
            return View("~/Views/Teacher/Questions/ImportResult.cshtml", result);
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        result.TotalRows = Math.Max(0, lastRow - 1);

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            for (int r = 2; r <= lastRow; r++)
            {
                var rowErrors = new List<string>();

                var courseIdStr = ws.Cell(r, 1).GetString().Trim();
                var type = ws.Cell(r, 2).GetString().Trim().ToLower();
                var content = ws.Cell(r, 3).GetString().Trim();
                var marksStr = ws.Cell(r, 4).GetString().Trim();

                var choiceA = ws.Cell(r, 5).GetString().Trim();
                var choiceB = ws.Cell(r, 6).GetString().Trim();
                var choiceC = ws.Cell(r, 7).GetString().Trim();
                var choiceD = ws.Cell(r, 8).GetString().Trim();
                var correct = ws.Cell(r, 9).GetString().Trim().ToUpper();

                int courseId;
                if (selectedCourseId.HasValue)
                {
                    courseId = selectedCourseId.Value;
                }
                else
                {
                    if (!int.TryParse(courseIdStr, out courseId))
                        rowErrors.Add("CourseId không hợp lệ (hoặc bạn chưa chọn khóa học).");
                }

                if (type != "mcq" && type != "true_false" && type != "essay")
                    rowErrors.Add("QuestionType phải là mcq/true_false/essay.");

                if (string.IsNullOrWhiteSpace(content))
                    rowErrors.Add("QuestionContent không được rỗng.");

                if (!decimal.TryParse(marksStr, out var marks) || marks <= 0)
                    rowErrors.Add("Marks phải là số > 0.");

                if (rowErrors.Count == 0)
                {
                    var owns = await TeacherOwnsCourse(teacherId, courseId);
                    if (!owns) rowErrors.Add("CourseId không thuộc giảng viên hoặc course bị xóa.");
                }

                if (rowErrors.Count == 0)
                {
                    if (type == "mcq")
                    {
                        if (string.IsNullOrWhiteSpace(choiceA) || string.IsNullOrWhiteSpace(choiceB))
                            rowErrors.Add("MCQ cần ít nhất ChoiceA và ChoiceB.");

                        if (!(correct == "A" || correct == "B" || correct == "C" || correct == "D"))
                            rowErrors.Add("MCQ Correct phải là A/B/C/D.");
                    }
                    else if (type == "true_false")
                    {
                        if (!(correct == "TRUE" || correct == "FALSE"))
                            rowErrors.Add("True/False Correct phải là TRUE hoặc FALSE.");
                    }
                }

                if (rowErrors.Count > 0)
                {
                    result.Errors.Add($"Row {r}: " + string.Join(" | ", rowErrors));
                    result.Failed++;
                    continue;
                }

                var q = new Question
                {
                    Courseid = courseId,
                    Questioncontent = content.Trim(),
                    Questiontype = type,
                    Marks = marks,
                    Createdby = teacherId,
                    Isdeleted = false,
                    Deletedat = null,
                    Createdat = DateTime.Now
                };

                _db.Questions.Add(q);
                await _db.SaveChangesAsync();

                if (type == "mcq")
                {
                    var choices = new List<(string text, bool isCorrect)>
                    {
                        (choiceA, correct == "A"),
                        (choiceB, correct == "B"),
                    };

                    if (!string.IsNullOrWhiteSpace(choiceC)) choices.Add((choiceC, correct == "C"));
                    if (!string.IsNullOrWhiteSpace(choiceD)) choices.Add((choiceD, correct == "D"));

                    foreach (var c in choices)
                    {
                        _db.Choices.Add(new Choice
                        {
                            Questionid = q.Questionid,
                            Choicetext = c.text.Trim(),
                            Iscorrect = c.isCorrect
                        });
                    }
                }
                else if (type == "true_false")
                {
                    _db.Choices.Add(new Choice { Questionid = q.Questionid, Choicetext = "TRUE", Iscorrect = (correct == "TRUE") });
                    _db.Choices.Add(new Choice { Questionid = q.Questionid, Choicetext = "FALSE", Iscorrect = (correct == "FALSE") });
                }

                result.Inserted++;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = $"Import xong! Thành công {result.Inserted}, lỗi {result.Failed}.";
            return View("~/Views/Teacher/Questions/ImportResult.cshtml", result);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            result.Failed = result.TotalRows;
            return View("~/Views/Teacher/Questions/ImportResult.cshtml", result);
        }
    }

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        var path = Path.Combine(_env.WebRootPath, "uploads", "templates", "QuestionBank_Import_Template.xlsx");
        if (!System.IO.File.Exists(path)) return NotFound();

        var bytes = System.IO.File.ReadAllBytes(path);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "QuestionBank_Import_Template.xlsx");
    }
}