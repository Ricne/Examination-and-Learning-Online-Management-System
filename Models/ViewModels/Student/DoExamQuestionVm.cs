namespace OMS.Models.ViewModels.Student;

public class DoExamQuestionVm
{
    public int Questionid { get; set; }
    public string Questioncontent { get; set; } = "";
    public string Questiontype { get; set; } = "";
    public decimal Marks { get; set; }

    public List<DoExamChoiceVm> Choices { get; set; } = new();

    public int? Selectedchoiceid { get; set; }
    public string? Essayanswer { get; set; }
}