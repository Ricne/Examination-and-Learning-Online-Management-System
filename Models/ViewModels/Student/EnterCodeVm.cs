using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Student;

public class EnterCodeVm
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = "";
}