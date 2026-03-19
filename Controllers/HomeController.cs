using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace OMS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

            if (role == "admin") return Redirect("/admin/dashboard/index");
            if (role == "teacher") return Redirect("/teacher/dashboard/index");
            if (role == "student") return Redirect("/student/dashboard/index");
        }

        ViewData["Title"] = "Home";
        return View();
    }
}