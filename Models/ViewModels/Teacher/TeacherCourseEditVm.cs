using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Teacher;

public class TeacherCourseEditVm
{
    public int Courseid { get; set; }

    [Required]
    [StringLength(200)]
    public string Coursename { get; set; } = "";

    public int Subjectid { get; set; }
    public string Subjectname { get; set; } = "";

    public string? Description { get; set; }

    public bool Isactive { get; set; }
}