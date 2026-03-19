namespace OMS.Models.ViewModels.Teacher;

public class AttemptListItemVm
{
    public int Attemptid { get; set; }
    public int Examid { get; set; }
    public string ExamTitle { get; set; } = "";
    public string StudentName { get; set; } = "";
    public DateTime Starttime { get; set; }
    public DateTime? Submittime { get; set; }
    public decimal? Totalscore { get; set; }
    public string Status { get; set; } = "";
}