using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("examaccesscodes")]
[Index("Examid", Name = "ix_eac_examid")]
[Index("Expiresat", Name = "ix_eac_expiresat")]
[Index("Isactive", Name = "ix_eac_isactive")]
[Index("Accesscode", Name = "uq_eac_code", IsUnique = true)]
public partial class Examaccesscode
{
    [Key]
    [Column("codeid")]
    public int Codeid { get; set; }

    [Column("examid")]
    public int Examid { get; set; }

    [Column("accesscode")]
    [StringLength(20)]
    public string Accesscode { get; set; } = null!;

    [Column("createdby")]
    public int Createdby { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [Column("expiresat")]
    public DateTime? Expiresat { get; set; }

    [Column("isactive")]
    public bool Isactive { get; set; }

    [Column("maxuses")]
    public int? Maxuses { get; set; }

    [Column("usedcount")]
    public int Usedcount { get; set; }

    [ForeignKey("Createdby")]
    [InverseProperty("Examaccesscodes")]
    public virtual User CreatedbyNavigation { get; set; } = null!;

    [ForeignKey("Examid")]
    [InverseProperty("Examaccesscodes")]
    public virtual Exam Exam { get; set; } = null!;
}
