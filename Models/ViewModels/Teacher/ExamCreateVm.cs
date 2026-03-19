using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Teacher;

public class ExamCreateVm
{
    [Required]
    public int Courseid { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    [Range(1, 1000)]
    public int Durationminutes { get; set; } = 30;

    public DateTime? Starttime { get; set; }
    public DateTime? Endtime { get; set; }

    [Range(0, 999)]
    public int Maxattempts { get; set; } = 1; // 0 = unlimited

    public bool Ispublished { get; set; } = false;

    public bool Allowreview { get; set; } = false;
}