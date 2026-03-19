using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Teacher;

public class QuestionEditVm
{
    public int Questionid { get; set; }

    [Required]
    public int Courseid { get; set; }

    [Required]
    [MaxLength(20)]
    public string Questiontype { get; set; } = "mcq"; 

    [Required]
    public string Questioncontent { get; set; } = "";

    [Range(0.01, 999999)]
    public decimal Marks { get; set; }

    public string? ChoiceA { get; set; }
    public string? ChoiceB { get; set; }
    public string? ChoiceC { get; set; }
    public string? ChoiceD { get; set; }
    public string? Correct { get; set; }
}