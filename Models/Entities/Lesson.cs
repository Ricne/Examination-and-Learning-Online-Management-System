using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("lessons")]
[Index("Courseid", Name = "ix_lessons_courseid")]
[Index("Isdeleted", Name = "ix_lessons_isdeleted")]
[Index("Courseid", "Lessonorder", Name = "uq_lesson_order", IsUnique = true)]
public partial class Lesson
{
    [Key]
    [Column("lessonid")]
    public int Lessonid { get; set; }

    [Column("courseid")]
    public int Courseid { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("content")]
    public string? Content { get; set; }

    [Column("videourl")]
    [StringLength(500)]
    public string? Videourl { get; set; }

    [Column("attachmenturl")]
    [StringLength(500)]
    public string? Attachmenturl { get; set; }

    [Column("lessonorder")]
    public int Lessonorder { get; set; }

    [Column("ispublished")]
    public bool Ispublished { get; set; }

    [Column("isdeleted")]
    public bool Isdeleted { get; set; }

    [Column("deletedat")]
    public DateTime? Deletedat { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [ForeignKey("Courseid")]
    [InverseProperty("Lessons")]
    public virtual Course Course { get; set; } = null!;

    [InverseProperty("Lesson")]
    public virtual ICollection<Lessonprogress> Lessonprogresses { get; set; } = new List<Lessonprogress>();
}
