using System;
using System.Collections.Generic;
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
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Choice>(entity =>
        {
            entity.HasKey(e => e.Choiceid).HasName("PK__choices__16C8B75F6987F2BB");

            entity.HasOne(d => d.Question).WithMany(p => p.Choices).HasConstraintName("fk_choices_question");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Courseid).HasName("PK__courses__2AAB4BC9471B9734");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.Subject).WithMany(p => p.Courses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_courses_subject");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Courses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_courses_teacher");
        });

        modelBuilder.Entity<Coursestudent>(entity =>
        {
            entity.HasKey(e => new { e.Courseid, e.Studentid }).HasName("PK__coursest__7E7A26EFA1FDA1A7");

            entity.Property(e => e.Enrolledat).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Course).WithMany(p => p.Coursestudents).HasConstraintName("fk_cs_course");

            entity.HasOne(d => d.Student).WithMany(p => p.Coursestudents).HasConstraintName("fk_cs_student");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Examid).HasName("PK__exams__A56C2E67A94BE1C6");

            entity.ToTable("exams", tb => tb.HasTrigger("trg_exams_publish_validate"));

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Maxattempts).HasDefaultValue(1);

            entity.HasOne(d => d.Course).WithMany(p => p.Exams).HasConstraintName("fk_exams_course");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_exams_createdby");

            entity.HasMany(d => d.Questions).WithMany(p => p.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "Examquestion",
                    r => r.HasOne<Question>().WithMany()
                        .HasForeignKey("Questionid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_eq_question"),
                    l => l.HasOne<Exam>().WithMany()
                        .HasForeignKey("Examid")
                        .HasConstraintName("fk_eq_exam"),
                    j =>
                    {
                        j.HasKey("Examid", "Questionid").HasName("PK__examques__13400C718E31CCD8");
                        j.ToTable("examquestions", tb => tb.HasTrigger("trg_examquestions_update_totalmarks"));
                        j.HasIndex(new[] { "Questionid" }, "ix_examquestions_questionid");
                        j.IndexerProperty<int>("Examid").HasColumnName("examid");
                        j.IndexerProperty<int>("Questionid").HasColumnName("questionid");
                    });
        });

        modelBuilder.Entity<Examaccesscode>(entity =>
        {
            entity.HasKey(e => e.Codeid).HasName("PK__examacce__47F9C38CABBE8D99");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Examaccesscodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_eac_createdby");

            entity.HasOne(d => d.Exam).WithMany(p => p.Examaccesscodes).HasConstraintName("fk_eac_exam");
        });

        modelBuilder.Entity<Examattempt>(entity =>
        {
            entity.HasKey(e => e.Attemptid).HasName("PK__examatte__93079C1E5CEF0DC2");

            entity.ToTable("examattempts", tb => tb.HasTrigger("trg_examattempts_enforce_maxattempts"));

            entity.Property(e => e.Starttime).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("inprogress");

            entity.HasOne(d => d.Exam).WithMany(p => p.Examattempts).HasConstraintName("fk_attempt_exam");

            entity.HasOne(d => d.Student).WithMany(p => p.Examattempts).HasConstraintName("fk_attempt_student");
        });

        modelBuilder.Entity<Examresult>(entity =>
        {
            entity.HasKey(e => e.Resultid).HasName("PK__examresu__C6EBD0433EC3794D");

            entity.Property(e => e.Gradedat).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Attempt).WithOne(p => p.Examresult).HasConstraintName("fk_result_attempt");

            entity.HasOne(d => d.GradedbyNavigation).WithMany(p => p.Examresults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_result_gradedby");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Lessonid).HasName("PK__lessons__F88B935051A31C27");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons).HasConstraintName("fk_lessons_course");
        });

        modelBuilder.Entity<Lessonprogress>(entity =>
        {
            entity.HasKey(e => new { e.Lessonid, e.Studentid }).HasName("PK__lessonpr__AC5AFE76518F04CF");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Lessonprogresses).HasConstraintName("fk_lp_lesson");

            entity.HasOne(d => d.Student).WithMany(p => p.Lessonprogresses).HasConstraintName("fk_lp_student");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Questionid).HasName("PK__question__62C2216AEFD90A2D");

            entity.ToTable("questions", tb => tb.HasTrigger("trg_questions_update_totalmarks"));

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Course).WithMany(p => p.Questions).HasConstraintName("fk_questions_course");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_questions_createdby");
        });

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Tokenid).HasName("PK__refresht__AC17DF2F2CE6675E");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.Refreshtokens).HasConstraintName("fk_refreshtokens_user");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("PK__roles__CD994BF2D0E014A6");
        });

        modelBuilder.Entity<Studentanswer>(entity =>
        {
            entity.HasKey(e => e.Answerid).HasName("PK__studenta__6837BD9C4E00477D");

            entity.ToTable("studentanswers", tb => tb.HasTrigger("trg_studentanswers_validate"));

            entity.HasOne(d => d.Attempt).WithMany(p => p.Studentanswers).HasConstraintName("fk_sa_attempt");

            entity.HasOne(d => d.Question).WithMany(p => p.Studentanswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sa_question");

            entity.HasOne(d => d.Selectedchoice).WithMany(p => p.Studentanswers).HasConstraintName("fk_sa_choice");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Subjectid).HasName("PK__subjects__ACE1437884B53055");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Isactive).HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK__users__CBA1B25799860EF1");

            entity.Property(e => e.Createdat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Isactive).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
