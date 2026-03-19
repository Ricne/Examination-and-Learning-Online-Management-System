using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[PrimaryKey("Lessonid", "Studentid")]
[Table("lessonprogress")]
[Index("Studentid", Name = "ix_lp_studentid")]
public partial class Lessonprogress
{
    [Key]
    [Column("lessonid")]
    public int Lessonid { get; set; }

    [Key]
    [Column("studentid")]
    public int Studentid { get; set; }

    [Column("iscompleted")]
    public bool Iscompleted { get; set; }

    [Column("completedat")]
    public DateTime? Completedat { get; set; }

    [ForeignKey("Lessonid")]
    [InverseProperty("Lessonprogresses")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("Studentid")]
    [InverseProperty("Lessonprogresses")]
    public virtual User Student { get; set; } = null!;
}
