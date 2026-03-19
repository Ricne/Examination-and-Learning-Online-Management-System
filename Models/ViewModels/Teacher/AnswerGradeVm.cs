namespace OMS.Models.ViewModels.Teacher;

public class AnswerGradeVm
{
    public int Answerid { get; set; }
    public int Questionid { get; set; }
    public string Questioncontent { get; set; } = "";
    public string Questiontype { get; set; } = "";
    public decimal Marks { get; set; }

    public string? SelectedChoiceText { get; set; }
    public string? Essayanswer { get; set; }

    public decimal? Score { get; set; }
}