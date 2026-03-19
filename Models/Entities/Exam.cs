using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("exams")]
[Index("Courseid", Name = "ix_exams_courseid")]
[Index("Isdeleted", Name = "ix_exams_isdeleted")]
[Index("Ispublished", Name = "ix_exams_ispublished")]
[Index("Requireaccesscode", Name = "ix_exams_requireaccesscode")]
public partial class Exam
{
    [Key]
    [Column("examid")]
    public int Examid { get; set; }

    [Column("courseid")]
    public int Courseid { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("durationminutes")]
    public int Durationminutes { get; set; }

    [Column("starttime")]
    public DateTime? Starttime { get; set; }

    [Column("endtime")]
    public DateTime? Endtime { get; set; }

    [Column("totalmarks", TypeName = "decimal(7, 2)")]
    public decimal Totalmarks { get; set; }

    [Column("maxattempts")]
    public int Maxattempts { get; set; }

    [Column("requireaccesscode")]
    public bool Requireaccesscode { get; set; }

    [Column("allowreview")]
    public bool Allowreview { get; set; }

    [Column("reviewavailablefrom")]
    public DateTime? Reviewavailablefrom { get; set; }

    [Column("reviewavailableto")]
    public DateTime? Reviewavailableto { get; set; }

    [Column("ispublished")]
    public bool Ispublished { get; set; }

    [Column("isdeleted")]
    public bool Isdeleted { get; set; }

    [Column("deletedat")]
    public DateTime? Deletedat { get; set; }

    [Column("createdby")]
    public int Createdby { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [ForeignKey("Courseid")]
    [InverseProperty("Exams")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("Createdby")]
    [InverseProperty("Exams")]
    public virtual User CreatedbyNavigation { get; set; } = null!;

    [InverseProperty("Exam")]
    public virtual ICollection<Examaccesscode> Examaccesscodes { get; set; } = new List<Examaccesscode>();

    [InverseProperty("Exam")]
    public virtual ICollection<Examattempt> Examattempts { get; set; } = new List<Examattempt>();

    [ForeignKey("Examid")]
    [InverseProperty("Exams")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
