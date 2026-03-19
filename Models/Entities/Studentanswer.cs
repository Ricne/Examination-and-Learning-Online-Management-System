using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("studentanswers")]
[Index("Attemptid", Name = "ix_studentanswers_attemptid")]
[Index("Questionid", Name = "ix_studentanswers_questionid")]
[Index("Attemptid", "Questionid", Name = "ux_studentanswers_attempt_question", IsUnique = true)]
public partial class Studentanswer
{
    [Key]
    [Column("answerid")]
    public int Answerid { get; set; }

    [Column("attemptid")]
    public int Attemptid { get; set; }

    [Column("questionid")]
    public int Questionid { get; set; }

    [Column("selectedchoiceid")]
    public int? Selectedchoiceid { get; set; }

    [Column("essayanswer")]
    public string? Essayanswer { get; set; }

    [Column("score", TypeName = "decimal(7, 2)")]
    public decimal? Score { get; set; }

    [ForeignKey("Attemptid")]
    [InverseProperty("Studentanswers")]
    public virtual Examattempt Attempt { get; set; } = null!;

    [ForeignKey("Questionid")]
    [InverseProperty("Studentanswers")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("Selectedchoiceid")]
    [InverseProperty("Studentanswers")]
    public virtual Choice? Selectedchoice { get; set; }
}
