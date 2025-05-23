﻿using Microsoft.EntityFrameworkCore;
using UtdAttendanceApplication.Models;

namespace UtdAttendanceApplication.Data;

public partial class AppDBContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AppDBContext(DbContextOptions<AppDBContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<Professor> Professors { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizBank> QuizBanks { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizQuestionBankAssignment> QuizQuestionBankAssignments { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAnswer> StudentAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connstr = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseMySql(connstr, ServerVersion.Parse("8.0.40-mysql"));
            throw new InvalidOperationException("Database connection string is not configured");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PRIMARY");

            entity.ToTable("attendance");

            entity.HasIndex(e => e.CourseId, "attendanceCourseID_idx");

            entity.HasIndex(e => e.AttendanceId, "attendanceID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.QuizId, "attendanceQuizID_idx");

            entity.HasIndex(e => e.SectionId, "attendanceSectionID_idx");

            entity.HasIndex(e => e.StudentId, "attendanceStudentID_idx");

            entity.Property(e => e.AttendanceId).HasColumnName("attendanceID");
            entity.Property(e => e.AttendanceStatus)
                .HasColumnType("enum('present','absent','excused','late')")
                .HasColumnName("attendanceStatus");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.QuizId).HasColumnName("quizID");
            entity.Property(e => e.SectionId).HasColumnName("sectionID");
            entity.Property(e => e.StudentId).HasColumnName("studentID");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("time");

            entity.HasOne(d => d.Course).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("attendanceCourseID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("attendanceQuizID");

            entity.HasOne(d => d.Section).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("attendanceSectionID");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasPrincipalKey(p => p.StudentId)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("attendanceStudentID");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PRIMARY");

            entity.ToTable("courses");

            entity.HasIndex(e => e.CourseId, "courseID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.ProfId, "profID_idx");

            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.CourseCode).HasColumnName("courseCode");
            entity.Property(e => e.CourseDept)
                .HasMaxLength(45)
                .HasColumnName("courseDept");
            entity.Property(e => e.CourseName)
                .HasMaxLength(45)
                .HasColumnName("courseName");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdOn");
            entity.Property(e => e.ProfId).HasColumnName("profID");
            entity.Property(e => e.ProfName)
                .HasMaxLength(45)
                .HasColumnName("profName");

            entity.HasOne(d => d.Prof).WithMany(p => p.Courses)
                .HasPrincipalKey(p => p.ProfessorId)
                .HasForeignKey(d => d.ProfId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("profID");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PRIMARY");

            entity.ToTable("enrollment");

            entity.HasIndex(e => e.CourseId, "courseId_idx");

            entity.HasIndex(e => e.EnrollmentId, "enrollmentId_UNIQUE").IsUnique();

            entity.HasIndex(e => e.SectionId, "sectionID_idx");

            entity.HasIndex(e => e.StudentId, "studentID_idx");

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollmentId");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.SectionId).HasColumnName("sectionID");
            entity.Property(e => e.StudentId).HasColumnName("studentID");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("courseID");

            entity.HasOne(d => d.Section).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("sectionID");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasPrincipalKey(p => p.StudentId)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("studentID");
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.HasKey(e => e.PasswordId).HasName("PRIMARY");

            entity.ToTable("passwords");

            entity.HasIndex(e => e.CourseId, "passCourseID_idx");

            entity.HasIndex(e => e.QuizId, "passQuizID_idx");

            entity.HasIndex(e => e.SectionId, "passSectionID_idx");

            entity.Property(e => e.PasswordId).HasColumnName("passwordID");
            entity.Property(e => e.AvailableOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("availableOn");
            entity.Property(e => e.AvailableUntil)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("availableUntil");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.Pwd)
                .HasMaxLength(45)
                .HasColumnName("pwd");
            entity.Property(e => e.QuizId).HasColumnName("quizID");
            entity.Property(e => e.SectionId).HasColumnName("sectionID");

            entity.HasOne(d => d.Course).WithMany(p => p.Passwords)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("passCourseID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Passwords)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("passQuizID");

            entity.HasOne(d => d.Section).WithMany(p => p.Passwords)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("passSectionID");
        });

        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(e => new { e.ProfessorId, e.UtdId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("professors");

            entity.HasIndex(e => e.ProfessorId, "professorID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UtdId, "utdID_UNIQUE").IsUnique();

            entity.Property(e => e.ProfessorId)
                .ValueGeneratedOnAdd()
                .HasColumnName("professorID");
            entity.Property(e => e.UtdId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("utdID");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdOn");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(45)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(45)
                .HasColumnName("lastName");
            entity.Property(e => e.MiddleInit)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("middleInit");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.UserName)
                .HasMaxLength(45)
                .HasColumnName("userName");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PRIMARY");

            entity.ToTable("questionOptions");

            entity.HasIndex(e => e.OptionId, "optionID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.QuestionId, "questionID_idx");

            entity.Property(e => e.OptionId).HasColumnName("optionID");
            entity.Property(e => e.OptionLabel).HasColumnName("optionLabel");
            entity.Property(e => e.OptionText)
                .HasMaxLength(255)
                .HasColumnName("optionText");
            entity.Property(e => e.QuestionId).HasColumnName("questionID");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("questionID");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PRIMARY");

            entity.ToTable("quizzes");

            entity.HasIndex(e => e.CourseId, "courseID_idx");

            entity.HasIndex(e => e.QuizId, "quizID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.SectionId, "quizSectionID_idx");

            entity.HasIndex(e => e.QuizBankId, "quizbankid_idx");

            entity.Property(e => e.QuizId).HasColumnName("quizID");
            entity.Property(e => e.AvailableOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("availableOn");
            entity.Property(e => e.AvailableUntil)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("availableUntil");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdOn");
            entity.Property(e => e.QuizBankId).HasColumnName("quizBankID");
            entity.Property(e => e.QuizTitle)
                .HasMaxLength(45)
                .HasColumnName("quizTitle");
            entity.Property(e => e.SectionId).HasColumnName("sectionID");

            entity.HasOne(d => d.Course).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quizCourseID");

            entity.HasOne(d => d.QuizBank).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.QuizBankId)
                .HasConstraintName("quizBankIDLink");

            entity.HasOne(d => d.Section).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("quizSectionID");
        });

        modelBuilder.Entity<QuizBank>(entity =>
        {
            entity.HasKey(e => e.QuizBankId).HasName("PRIMARY");

            entity.ToTable("quizBank");

            entity.HasIndex(e => e.CourseId, "quizBankCourseID_idx");

            entity.HasIndex(e => e.QuizBankId, "quizBankID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.SectionId, "quizBankSectionID_idx");

            entity.Property(e => e.QuizBankId).HasColumnName("quizBankID");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.QuizBankTitle)
                .HasMaxLength(255)
                .HasColumnName("quizBankTitle");
            entity.Property(e => e.SectionId).HasColumnName("sectionID");

            entity.HasOne(d => d.Course).WithMany(p => p.QuizBanks)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quizBankCourseID");

            entity.HasOne(d => d.Section).WithMany(p => p.QuizBanks)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quizBankSectionID");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PRIMARY");

            entity.ToTable("quizQuestions");

            entity.HasIndex(e => e.QuestionId, "questionID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.QuizBankId, "quizquestionbankID_idx");

            entity.Property(e => e.QuestionId).HasColumnName("questionID");
            entity.Property(e => e.CorrectOption).HasColumnName("correctOption");
            entity.Property(e => e.QuestionText)
                .HasColumnType("text")
                .HasColumnName("questionText");
            entity.Property(e => e.QuizBankId).HasColumnName("quizBankID");

            entity.HasOne(d => d.QuizBank).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizBankId)
                .HasConstraintName("quizQuestioBankID");
        });

        modelBuilder.Entity<QuizQuestionBankAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PRIMARY");

            entity.ToTable("quizQuestionBankAssignment");

            entity.HasIndex(e => e.AssignmentId, "assignmentID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.QuestionId, "quizquestionassignmentquestionID_idx");

            entity.HasIndex(e => e.QuizId, "quizquestionassignmentquizID_idx");

            entity.Property(e => e.AssignmentId).HasColumnName("assignmentID");
            entity.Property(e => e.QuestionId).HasColumnName("questionID");
            entity.Property(e => e.QuizId).HasColumnName("quizID");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizQuestionBankAssignments)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("quizquestionassignmentquestionID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestionBankAssignments)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("quizquestionassignmentquizID");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PRIMARY");

            entity.ToTable("sections");

            entity.HasIndex(e => e.CourseId, "courseSectionID_idx");

            entity.Property(e => e.SectionId).HasColumnName("sectionID");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.EndTime)
                .HasColumnType("time")
                .HasColumnName("endTime");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.MeetingRoom)
                .HasMaxLength(45)
                .HasColumnName("meetingRoom");
            entity.Property(e => e.SectionCode)
                .HasMaxLength(3)
                .HasColumnName("sectionCode");
            entity.Property(e => e.StartDate).HasColumnName("startDate");
            entity.Property(e => e.StartTime)
                .HasColumnType("time")
                .HasColumnName("startTime");

            entity.HasOne(d => d.Course).WithMany(p => p.Sections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("courseSectionID");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.UtdId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("students");

            entity.HasIndex(e => e.StudentId, "studentID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.UtdId, "utdID_UNIQUE").IsUnique();

            entity.Property(e => e.StudentId)
                .ValueGeneratedOnAdd()
                .HasColumnName("studentID");
            entity.Property(e => e.UtdId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("utdID");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdOn");
            entity.Property(e => e.FirstName)
                .HasMaxLength(45)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(45)
                .HasColumnName("lastName");
            entity.Property(e => e.UserName)
                .HasMaxLength(45)
                .HasColumnName("userName");
        });

        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PRIMARY");

            entity.ToTable("studentAnswers");

            entity.HasIndex(e => e.AnswerId, "answerID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.QuestionId, "submittedQuestionID_idx");

            entity.HasIndex(e => e.QuizId, "submittedQuizID_idx");

            entity.HasIndex(e => e.SelectedOptionId, "submittedSelectedOptionID_idx");

            entity.HasIndex(e => e.StudentId, "submittedStudentID_idx");

            entity.Property(e => e.AnswerId).HasColumnName("answerID");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ipAddress");
            entity.Property(e => e.IsCorrect).HasColumnName("isCorrect");
            entity.Property(e => e.QuestionId).HasColumnName("questionID");
            entity.Property(e => e.QuizId).HasColumnName("quizID");
            entity.Property(e => e.SelectedOptionId).HasColumnName("selectedOptionID");
            entity.Property(e => e.StudentId).HasColumnName("studentID");
            entity.Property(e => e.SubmittedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("submittedOn");

            entity.HasOne(d => d.Question).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("submittedQuestionID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("submittedQuizID");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("submittedSelectedOptionID");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAnswers)
                .HasPrincipalKey(p => p.StudentId)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("submittedStudentID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
