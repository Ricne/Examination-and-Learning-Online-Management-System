using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("subjects")]
[Index("Isdeleted", Name = "ix_subjects_isdeleted")]
[Index("Subjectname", Name = "uq_subjects_name", IsUnique = true)]
public partial class Subject
{
    [Key]
    [Column("subjectid")]
    public int Subjectid { get; set; }

    [Column("subjectname")]
    [StringLength(100)]
    public string Subjectname { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("isactive")]
    public bool Isactive { get; set; }

    [Column("isdeleted")]
    public bool Isdeleted { get; set; }

    [Column("deletedat")]
    public DateTime? Deletedat { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [InverseProperty("Subject")]
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
