namespace OMS.Models.ViewModels.Student;

public class DoExamVm
{
    public int Attemptid { get; set; }
    public int Examid { get; set; }
    public string ExamTitle { get; set; } = "";
    public int Durationminutes { get; set; }
    public DateTime Starttime { get; set; }

    public DateTime ExpireAt { get; set; }
    public DateTime ServerNow { get; set; }

    public List<DoExamQuestionVm> Questions { get; set; } = new();
}