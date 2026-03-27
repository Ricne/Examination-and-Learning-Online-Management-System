using Microsoft.EntityFrameworkCore;
using OMS.Models.Entities;

namespace OMS.Data;

public partial class OnlineLearningExamSystemContext : DbContext
{
    public OnlineLearningExamSystemContext()
    {
    }

    public OnlineLearningExamSystemContext(DbContextOptions<OnlineLearningExamSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Choice> Choices { get; set; }
    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<Coursestudent> Coursestudents { get; set; }
    public virtual DbSet<Exam> Exams { get; set; }
    public virtual DbSet<Examaccesscode> Examaccesscodes { get; set; }
    public virtual DbSet<Examattempt> Examattempts { get; set; }
    public virtual DbSet<Examresult> Examresults { get; set; }
    public virtual DbSet<Lesson> Lessons { get; set; }
    public virtual DbSet<Lessonprogress> Lessonprogresses { get; set; }
    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<Refreshtoken> Refreshtokens { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Studentanswer> Studentanswers { get; set; }
    public virtual DbSet<Subject> Subjects { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToLower());

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName()?.ToLower());
            }
        }

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.HasKey(e => e.Choiceid);

            entity.HasOne(d => d.Question)
                .WithMany(p => p.Choices)
                .HasConstraintName("fk_choices_question");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Courseid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Isactive)
                .HasDefaultValue(true);

            entity.HasOne(d => d.Subject)
                .WithMany(p => p.Courses)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Teacher)
                .WithMany(p => p.Courses)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Coursestudent>(entity =>
        {
            entity.HasKey(e => new { e.Courseid, e.Studentid });

            entity.Property(e => e.Enrolledat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Examid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Maxattempts)
                .HasDefaultValue(1);
        });

        modelBuilder.Entity<Examaccesscode>(entity =>
        {
            entity.HasKey(e => e.Codeid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Isactive)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<Examattempt>(entity =>
        {
            entity.HasKey(e => e.Attemptid);

            entity.Property(e => e.Starttime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Status)
                .HasDefaultValue("inprogress");
        });

        modelBuilder.Entity<Examresult>(entity =>
        {
            entity.HasKey(e => e.Resultid);

            entity.Property(e => e.Gradedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Lessonid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Questionid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Tokenid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid);
        });

        modelBuilder.Entity<Studentanswer>(entity =>
        {
            entity.HasKey(e => e.Answerid);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Subjectid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Isactive)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid);

            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Isactive)
                .HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}