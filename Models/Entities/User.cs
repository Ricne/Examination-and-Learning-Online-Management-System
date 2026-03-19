using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OMS.Models.Entities;

[Table("users")]
[Index("Roleid", Name = "ix_users_roleid")]
[Index("Email", Name = "uq_users_email", IsUnique = true)]
[Index("Username", Name = "uq_users_username", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("userid")]
    public int Userid { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Column("passwordhash")]
    [StringLength(255)]
    public string Passwordhash { get; set; } = null!;

    [Column("fullname")]
    [StringLength(100)]
    public string Fullname { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("roleid")]
    public int Roleid { get; set; }

    [Column("avatarurl")]
    [StringLength(255)]
    public string? Avatarurl { get; set; }

    [Column("isactive")]
    public bool Isactive { get; set; }

    [Column("createdat")]
    public DateTime Createdat { get; set; }

    [InverseProperty("Teacher")]
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    [InverseProperty("Student")]
    public virtual ICollection<Coursestudent> Coursestudents { get; set; } = new List<Coursestudent>();

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Examaccesscode> Examaccesscodes { get; set; } = new List<Examaccesscode>();

    [InverseProperty("Student")]
    public virtual ICollection<Examattempt> Examattempts { get; set; } = new List<Examattempt>();

    [InverseProperty("GradedbyNavigation")]
    public virtual ICollection<Examresult> Examresults { get; set; } = new List<Examresult>();

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Student")]
    public virtual ICollection<Lessonprogress> Lessonprogresses { get; set; } = new List<Lessonprogress>();

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [InverseProperty("User")]
    public virtual ICollection<Refreshtoken> Refreshtokens { get; set; } = new List<Refreshtoken>();

    [ForeignKey("Roleid")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;
}
