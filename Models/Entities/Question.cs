using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("questions")]
[Index("Courseid", Name = "ix_questions_courseid")]
[Index("Isdeleted", Name = "ix_questions_isdeleted")]
public partial class Question
{
    [Key]
    [Column("questionid")]
    public int Questionid { get; set; }

    [Column("courseid")]
    public int Courseid { get; set; }

    [Column("questioncontent")]
    public string Questioncontent { get; set; } = null!;

    [Column("questiontype")]
    [StringLength(20)]
    public string Questiontype { get; set; } = null!;

    [Column("marks", TypeName = "decimal(7, 2)")]
    public decimal Marks { get; set; }

    [Column("createdby")]
    public int Createdby { get; set; }

    [Column("isdeleted")]
    public bool Isdeleted { get; set; }

    [Column("deletedat")]
    public DateTime? Deletedat { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();

    [ForeignKey("Courseid")]
    [InverseProperty("Questions")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("Createdby")]
    [InverseProperty("Questions")]
    public virtual User CreatedbyNavigation { get; set; } = null!;

    [InverseProperty("Question")]
    public virtual ICollection<Studentanswer> Studentanswers { get; set; } = new List<Studentanswer>();

    [ForeignKey("Questionid")]
    [InverseProperty("Questions")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
