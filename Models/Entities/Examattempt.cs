using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("examattempts")]
[Index("Examid", Name = "ix_attempt_examid")]
[Index("Status", Name = "ix_attempt_status")]
[Index("Studentid", Name = "ix_attempt_studentid")]
public partial class Examattempt
{
    [Key]
    [Column("attemptid")]
    public int Attemptid { get; set; }

    [Column("examid")]
    public int Examid { get; set; }

    [Column("studentid")]
    public int Studentid { get; set; }

    [Column("starttime")]
    public DateTime Starttime { get; set; }

    [Column("submittime")]
    public DateTime? Submittime { get; set; }

    [Column("totalscore", TypeName = "decimal(7, 2)")]
    public decimal? Totalscore { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("usedaccesscode")]
    [StringLength(20)]
    public string? Usedaccesscode { get; set; }

    [ForeignKey("Examid")]
    [InverseProperty("Examattempts")]
    public virtual Exam Exam { get; set; } = null!;

    [InverseProperty("Attempt")]
    public virtual Examresult? Examresult { get; set; }

    [ForeignKey("Studentid")]
    [InverseProperty("Examattempts")]
    public virtual User Student { get; set; } = null!;

    [InverseProperty("Attempt")]
    public virtual ICollection<Studentanswer> Studentanswers { get; set; } = new List<Studentanswer>();
}
