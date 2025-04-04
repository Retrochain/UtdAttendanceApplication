using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class Quize
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }

    public int? SectionId { get; set; }

    public int QuestionId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public DateOnly? AvailabeOn { get; set; }

    public DateOnly? AvailableUntil { get; set; }

    public DateTime? CreatedOn { get; set; }

    public TimeOnly? TimeStart { get; set; }

    public TimeOnly? TimeEnd { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Password> Passwords { get; set; } = new List<Password>();

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual Section? Section { get; set; }

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
