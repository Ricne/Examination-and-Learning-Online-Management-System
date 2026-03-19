using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Auth;

public class LoginVm
{
    [Required(ErrorMessage = "Vui lòng nhập Username hoặc Email")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}