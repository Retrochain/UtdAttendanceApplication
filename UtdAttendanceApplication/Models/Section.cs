namespace UtdAttendanceApplication.Models;

public partial class Section
{
    public int SectionId { get; set; }

    public int CourseId { get; set; }

    public int SectionCode { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int Duration { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Password> Passwords { get; set; } = new List<Password>();

    public virtual ICollection<QuizBank> QuizBanks { get; set; } = new List<QuizBank>();

    public virtual ICollection<Quizes> Quizes { get; set; } = new List<Quizes>();
}
