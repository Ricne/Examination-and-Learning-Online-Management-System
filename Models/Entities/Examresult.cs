using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("examresults")]
[Index("Attemptid", Name = "UQ__examresu__93079C1F09A332B6", IsUnique = true)]
[Index("Gradedby", Name = "ix_examresults_gradedby")]
public partial class Examresult
{
    [Key]
    [Column("resultid")]
    public int Resultid { get; set; }

    [Column("attemptid")]
    public int Attemptid { get; set; }

    [Column("finalscore", TypeName = "decimal(7, 2)")]
    public decimal Finalscore { get; set; }

    [Column("gradedby")]
    public int Gradedby { get; set; }

    [Column("gradedat")]
    public DateTime Gradedat { get; set; }

    [ForeignKey("Attemptid")]
    [InverseProperty("Examresult")]
    public virtual Examattempt Attempt { get; set; } = null!;

    [ForeignKey("Gradedby")]
    [InverseProperty("Examresults")]
    public virtual User GradedbyNavigation { get; set; } = null!;
}
