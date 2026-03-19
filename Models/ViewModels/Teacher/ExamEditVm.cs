using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Teacher;

public class ExamEditVm
{
    public int Examid { get; set; }

    [Required]
    public int Courseid { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    [Range(1, 1000)]
    public int Durationminutes { get; set; }

    public DateTime? Starttime { get; set; }
    public DateTime? Endtime { get; set; }

    [Range(0, 999)]
    public int Maxattempts { get; set; }

    public bool Ispublished { get; set; }
    public bool Allowreview { get; set; }
}