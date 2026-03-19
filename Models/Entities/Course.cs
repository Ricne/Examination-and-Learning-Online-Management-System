using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("courses")]
[Index("Isdeleted", Name = "ix_courses_isdeleted")]
[Index("Subjectid", Name = "ix_courses_subjectid")]
[Index("Teacherid", Name = "ix_courses_teacherid")]
public partial class Course
{
    [Key]
    [Column("courseid")]
    public int Courseid { get; set; }

    [Column("coursename")]
    [StringLength(200)]
    public string Coursename { get; set; } = null!;

    [Column("subjectid")]
    public int Subjectid { get; set; }

    [Column("teacherid")]
    public int Teacherid { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("isactive")]
    public bool Isactive { get; set; }

    [Column("isdeleted")]
    public bool Isdeleted { get; set; }

    [Column("deletedat")]
    public DateTime? Deletedat { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [InverseProperty("Course")]
    public virtual ICollection<Coursestudent> Coursestudents { get; set; } = new List<Coursestudent>();

    [InverseProperty("Course")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Course")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    [InverseProperty("Course")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [ForeignKey("Subjectid")]
    [InverseProperty("Courses")]
    public virtual Subject Subject { get; set; } = null!;

    [ForeignKey("Teacherid")]
    [InverseProperty("Courses")]
    public virtual User Teacher { get; set; } = null!;
}
