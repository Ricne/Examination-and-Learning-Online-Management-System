using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.Entities;
using OMS.Models.ViewModels.Admin;

namespace OMS.Controllers.Admin;

[Authorize(Roles = "admin")]
[Route("admin/users/[action]")]
public class UsersController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;
    private readonly IWebHostEnvironment _env;

    public UsersController(OnlineLearningExamSystemContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private async Task<string?> SaveAvatarAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException("Avatar chỉ chấp nhận .jpg/.jpeg/.png/.webp");

        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("Avatar tối đa 2MB.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/avatars/{fileName}";
    }

    private async Task<bool> CanHardDeleteUser(int userId)
    {
        var usedAsTeacher = await _db.Courses.AnyAsync(c => c.Teacherid == userId);
        if (usedAsTeacher) return false;

        var hasEnroll = await _db.Coursestudents.AnyAsync(x => x.Studentid == userId);
        if (hasEnroll) return false;

        var createdQuestions = await _db.Questions.AnyAsync(q => q.Createdby == userId);
        if (createdQuestions) return false;

        var createdExams = await _db.Exams.AnyAsync(e => e.Createdby == userId);
        if (createdExams) return false;

        var hasAttempts = await _db.Examattempts.AnyAsync(a => a.Studentid == userId);
        if (hasAttempts) return false;

        var gradedResults = await _db.Examresults.AnyAsync(r => r.Gradedby == userId);
        if (gradedResults) return false;

        var accessCodes = await _db.Examaccesscodes.AnyAsync(c => c.Createdby == userId);
        if (accessCodes) return false;

        return true;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Người dùng";
        ViewData["Badge"] = "Admin";

        var users = await _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderByDescending(u => u.Createdat)
            .ToListAsync();

        return View("~/Views/Admin/Users/Index.cshtml", users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Người dùng";
        ViewData["SubTitle"] = "Tạo tài khoản mới";
        ViewData["Badge"] = "Create";
        ViewData["Breadcrumb"] = "Admin / Users / Create";

        ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Rolename).ToListAsync();
        return View("~/Views/Admin/Users/Create.cshtml", new UserCreateVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateVm vm)
    {
        ViewData["Title"] = "Người dùng";
        ViewData["SubTitle"] = "Tạo tài khoản mới";
        ViewData["Badge"] = "Create";
        ViewData["Breadcrumb"] = "Admin / Users / Create";

        ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Rolename).ToListAsync();

        vm.Username = (vm.Username ?? "").Trim();
        vm.FullName = (vm.FullName ?? "").Trim();
        vm.Email = (vm.Email ?? "").Trim();

        if (!ModelState.IsValid) return View("~/Views/Admin/Users/Create.cshtml", vm);

        if (await _db.Users.AnyAsync(u => u.Username == vm.Username))
        {
            ModelState.AddModelError(nameof(vm.Username), "Username đã tồn tại.");
            return View("~/Views/Admin/Users/Create.cshtml", vm);
        }

        if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
        {
            ModelState.AddModelError(nameof(vm.Email), "Email đã tồn tại.");
            return View("~/Views/Admin/Users/Create.cshtml", vm);
        }

        string? avatarUrl = null;
        try
        {
            avatarUrl = await SaveAvatarAsync(vm.AvatarFile);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(vm.AvatarFile), ex.Message);
            return View("~/Views/Admin/Users/Create.cshtml", vm);
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(vm.Password);

        var user = new User
        {
            Username = vm.Username,
            Fullname = vm.FullName,
            Email = vm.Email,
            Passwordhash = hash,
            Roleid = vm.RoleId,
            Avatarurl = avatarUrl,
            Isactive = vm.IsActive,
            Createdat = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Tạo user thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Người dùng";
        ViewData["SubTitle"] = "Cập nhật thông tin user";
        ViewData["Badge"] = "Edit";
        ViewData["Breadcrumb"] = "Admin / Users / Edit";

        ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Rolename).ToListAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Userid == id);
        if (user == null) return NotFound();

        var vm = new UserEditVm
        {
            UserId = user.Userid,
            Username = user.Username,
            FullName = user.Fullname,
            Email = user.Email,
            RoleId = user.Roleid,
            AvatarUrl = user.Avatarurl,
            IsActive = user.Isactive
        };

        return View("~/Views/Admin/Users/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditVm vm)
    {
        ViewData["Title"] = "Người dùng";
        ViewData["SubTitle"] = "Cập nhật thông tin user";
        ViewData["Badge"] = "Edit";
        ViewData["Breadcrumb"] = "Admin / Users / Edit";

        ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Rolename).ToListAsync();

        vm.Username = (vm.Username ?? "").Trim();
        vm.FullName = (vm.FullName ?? "").Trim();
        vm.Email = (vm.Email ?? "").Trim();

        if (!ModelState.IsValid) return View("~/Views/Admin/Users/Edit.cshtml", vm);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Userid == vm.UserId);
        if (user == null) return NotFound();

        if (await _db.Users.AnyAsync(u => u.Userid != vm.UserId && u.Username == vm.Username))
        {
            ModelState.AddModelError(nameof(vm.Username), "Username đã tồn tại.");
            return View("~/Views/Admin/Users/Edit.cshtml", vm);
        }

        if (await _db.Users.AnyAsync(u => u.Userid != vm.UserId && u.Email == vm.Email))
        {
            ModelState.AddModelError(nameof(vm.Email), "Email đã tồn tại.");
            return View("~/Views/Admin/Users/Edit.cshtml", vm);
        }

        if (vm.AvatarFile != null)
        {
            try
            {
                var newUrl = await SaveAvatarAsync(vm.AvatarFile);
                if (!string.IsNullOrWhiteSpace(newUrl))
                    user.Avatarurl = newUrl;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(vm.AvatarFile), ex.Message);
                vm.AvatarUrl = user.Avatarurl;
                return View("~/Views/Admin/Users/Edit.cshtml", vm);
            }
        }

        user.Username = vm.Username;
        user.Fullname = vm.FullName;
        user.Email = vm.Email;
        user.Roleid = vm.RoleId;
        user.Isactive = vm.IsActive;

        if (!string.IsNullOrWhiteSpace(vm.NewPassword))
        {
            user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật user thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Userid == id);
        if (user == null) return NotFound();

        user.Isactive = !user.Isactive;
        await _db.SaveChangesAsync();

        TempData["Success"] = user.Isactive
            ? "Đã bật tài khoản."
            : "Đã tắt tài khoản.";

        return Redirect("/admin/users/index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Userid == id);
        if (user == null) return NotFound();

        var canDelete = await CanHardDeleteUser(id);
        if (!canDelete)
        {
            TempData["Error"] = "Không thể xóa user vì đang liên kết dữ liệu (course/exam/attempt...). Hãy dùng TẮT tài khoản thay vì xóa.";
            return RedirectToAction(nameof(Index));
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã xóa user.";
        return RedirectToAction(nameof(Index));
    }
}