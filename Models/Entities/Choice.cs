using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("choices")]
[Index("Questionid", Name = "ix_choices_questionid")]
public partial class Choice
{
    [Key]
    [Column("choiceid")]
    public int Choiceid { get; set; }

    [Column("questionid")]
    public int Questionid { get; set; }

    [Column("choicetext")]
    [StringLength(500)]
    public string Choicetext { get; set; } = null!;

    [Column("iscorrect")]
    public bool Iscorrect { get; set; }

    [ForeignKey("Questionid")]
    [InverseProperty("Choices")]
    public virtual Question Question { get; set; } = null!;

    [InverseProperty("Selectedchoice")]
    public virtual ICollection<Studentanswer> Studentanswers { get; set; } = new List<Studentanswer>();
}
