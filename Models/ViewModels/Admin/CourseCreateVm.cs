using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Admin;

public class CourseCreateVm
{
    [Required, StringLength(200)]
    public string CourseName { get; set; } = "";

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}