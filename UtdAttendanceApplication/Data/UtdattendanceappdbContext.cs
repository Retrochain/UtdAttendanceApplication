using Microsoft.EntityFrameworkCore;
using UtdAttendanceApplication.Models;

namespace UtdAttendanceApplication.Data;

public partial class UtdattendanceappdbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public UtdattendanceappdbContext(DbContextOptions<UtdattendanceappdbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Professor> Professors { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<QuizBank> QuizBanks { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<Quize> Quizes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAnswer> StudentAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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

            entity.HasIndex(e => e.StudentId, "attendanceStudentID_idx");

            entity.Property(e => e.AttendanceId).HasColumnName("attendanceID");
            entity.Property(e => e.AttendanceStatus)
                .HasColumnType("enum('present','absent','excused')")
                .HasColumnName("attendanceStatus");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.QuizId).HasColumnName("quizID");
            entity.Property(e => e.StudentId).HasColumnName("studentID");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("time");

            entity.HasOne(d => d.Course).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("attendanceCourseID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("attendanceQuizID");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasPrincipalKey(p => p.StudentId)
                .HasForeignKey(d => d.StudentId)
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

            entity.HasIndex(e => e.StudentId, "studentId_idx");

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollmentId");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.StudentId).HasColumnName("studentID");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("courseID");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasPrincipalKey(p => p.StudentId)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("studentID");
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
            entity.Property(e => e.OptionText)
                .HasMaxLength(255)
                .HasColumnName("optionText");
            entity.Property(e => e.QuestionId).HasColumnName("questionID");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("questionID");
        });

        modelBuilder.Entity<QuizBank>(entity =>
        {
            entity.HasKey(e => e.QuizBankId).HasName("PRIMARY");

            entity.ToTable("quizBank");

            entity.HasIndex(e => e.CourseId, "quizBankCourseID_idx");

            entity.HasIndex(e => e.QuizBankId, "quizBankID_UNIQUE").IsUnique();

            entity.Property(e => e.QuizBankId).HasColumnName("quizBankID");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.QuizTitle)
                .HasMaxLength(255)
                .HasColumnName("quizTitle");

            entity.HasOne(d => d.Course).WithMany(p => p.QuizBanks)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quizBankCourseID");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PRIMARY");

            entity.ToTable("quizQuestions");

            entity.HasIndex(e => e.QuestionId, "questionID_UNIQUE").IsUnique();

            entity.HasIndex(e => e.QuizBankId, "quizBankID_idx");

            entity.Property(e => e.QuestionId).HasColumnName("questionID");
            entity.Property(e => e.CorrectOption).HasColumnName("correctOption");
            entity.Property(e => e.QuestionAnswer)
                .HasMaxLength(255)
                .HasColumnName("questionAnswer");
            entity.Property(e => e.QuestionText)
                .HasColumnType("text")
                .HasColumnName("questionText");
            entity.Property(e => e.QuizBankId).HasColumnName("quizBankID");

            entity.HasOne(d => d.QuizBank).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizBankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quizBankID");
        });

        modelBuilder.Entity<Quize>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PRIMARY");

            entity.ToTable("quizes");

            entity.HasIndex(e => e.CourseId, "courseID_idx");

            entity.HasIndex(e => e.QuizId, "quizID_UNIQUE").IsUnique();

            entity.Property(e => e.QuizId).HasColumnName("quizID");
            entity.Property(e => e.AvailabeOn)
                .HasColumnType("datetime")
                .HasColumnName("availabeOn");
            entity.Property(e => e.AvailableUntil)
                .HasColumnType("datetime")
                .HasColumnName("availableUntil");
            entity.Property(e => e.CourseId).HasColumnName("courseID");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdOn");
            entity.Property(e => e.QuizPwd)
                .HasMaxLength(255)
                .HasColumnName("quizPwd");
            entity.Property(e => e.QuizTitle)
                .HasMaxLength(45)
                .HasColumnName("quizTitle");

            entity.HasOne(d => d.Course).WithMany(p => p.Quizes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quizCourseID");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("submittedQuestionID");

            entity.HasOne(d => d.Quiz).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("submittedQuizID");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.StudentAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
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
