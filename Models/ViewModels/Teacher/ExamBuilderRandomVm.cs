using System.ComponentModel.DataAnnotations;

namespace OMS.Models.ViewModels.Teacher;

public class ExamBuilderRandomVm
{
    public int Examid { get; set; }

    [Range(1, 200)]
    public int NumberOfQuestions { get; set; } = 5;
}