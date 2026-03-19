using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("refreshtokens")]
[Index("Expiresat", Name = "ix_refreshtokens_expiresat")]
[Index("Userid", Name = "ix_refreshtokens_userid")]
[Index("Token", Name = "ux_refreshtokens_token", IsUnique = true)]
public partial class Refreshtoken
{
    [Key]
    [Column("tokenid")]
    public long Tokenid { get; set; }

    [Column("userid")]
    public int Userid { get; set; }

    [Column("token")]
    [StringLength(400)]
    public string Token { get; set; } = null!;

    [Column("jwtid")]
    [StringLength(100)]
    public string? Jwtid { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [Column("expiresat")]
    public DateTime Expiresat { get; set; }

    [Column("revokedat")]
    public DateTime? Revokedat { get; set; }

    [Column("replacedbytoken")]
    [StringLength(400)]
    public string? Replacedbytoken { get; set; }

    [Column("createdbyip")]
    [StringLength(45)]
    public string? Createdbyip { get; set; }

    [Column("revokedbyip")]
    [StringLength(45)]
    public string? Revokedbyip { get; set; }

    [Column("useragent")]
    [StringLength(300)]
    public string? Useragent { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Refreshtokens")]
    public virtual User User { get; set; } = null!;
}
