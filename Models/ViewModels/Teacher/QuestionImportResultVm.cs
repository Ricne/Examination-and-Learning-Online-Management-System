namespace OMS.Models.ViewModels.Teacher;

public class QuestionImportResultVm
{
    public int TotalRows { get; set; }
    public int Inserted { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}