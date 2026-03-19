using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models.ViewModels.Auth;

namespace OMS.Controllers;

public class AuthController : Controller
{
    private readonly OnlineLearningExamSystemContext _db;

    public AuthController(OnlineLearningExamSystemContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Login";
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        ViewData["Title"] = "Login";
        if (!ModelState.IsValid) return View(vm);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u =>
                u.Username == vm.UsernameOrEmail || u.Email == vm.UsernameOrEmail);

        if (user is null || user.Isactive != true)
        {
            TempData["Error"] = "Tài khoản không tồn tại hoặc đã bị khóa.";
            return View(vm);
        }

        if (user.Isactive != true)
        {
            TempData["Error"] = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ admin.";
            return View(vm);
        }

        var ok = BCrypt.Net.BCrypt.Verify(vm.Password, user.Passwordhash);
        if (!ok)
        {
            TempData["Error"] = "Sai tài khoản hoặc mật khẩu.";
            return View(vm);
        }

        var roleName = user.Role?.Rolename ?? "";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.Fullname ?? user.Username),
            new Claim(ClaimTypes.Role, roleName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        TempData["Success"] = "Đăng nhập thành công!";

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return roleName switch
        {
            "admin" => Redirect("/admin/dashboard/index"),
            "teacher" => Redirect("/teacher/dashboard/index"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Denied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }
}