namespace UtdAttendanceApplication.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public int CourseCode { get; set; }

    public int ProfId { get; set; }

    public string ProfName { get; set; } = null!;

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Password> Passwords { get; set; } = new List<Password>();

    public virtual Professor Prof { get; set; } = null!;

    public virtual ICollection<QuizBank> QuizBanks { get; set; } = new List<QuizBank>();

    public virtual ICollection<Quizes> Quizes { get; set; } = new List<Quizes>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
