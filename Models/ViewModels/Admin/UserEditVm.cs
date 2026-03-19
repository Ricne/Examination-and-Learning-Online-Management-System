using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OMS.Models.ViewModels.Admin;

public class UserEditVm
{
    public int UserId { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";

    [StringLength(100, MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [Required]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? AvatarFile { get; set; }

    public string? AvatarUrl { get; set; }
}