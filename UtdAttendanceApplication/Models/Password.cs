namespace UtdAttendanceApplication.Models;

public partial class Password
{
    public int PasswordId { get; set; }

    public int CourseId { get; set; }

    public int SectionId { get; set; }

    public int QuizId { get; set; }

    public string? Pwd { get; set; }

    public DateTime? AvailableOn { get; set; }

    public DateTime? AvailableUntil { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Quizes Quiz { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;
}
