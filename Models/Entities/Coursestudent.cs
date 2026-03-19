using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[PrimaryKey("Courseid", "Studentid")]
[Table("coursestudents")]
[Index("Studentid", Name = "ix_cs_studentid")]
public partial class Coursestudent
{
    [Key]
    [Column("courseid")]
    public int Courseid { get; set; }

    [Key]
    [Column("studentid")]
    public int Studentid { get; set; }

    [Column("enrolledat")]
    public DateTime Enrolledat { get; set; }

    [ForeignKey("Courseid")]
    [InverseProperty("Coursestudents")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("Studentid")]
    [InverseProperty("Coursestudents")]
    public virtual User Student { get; set; } = null!;
}
