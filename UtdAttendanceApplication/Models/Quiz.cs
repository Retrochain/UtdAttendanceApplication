using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }

    public int? SectionId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public int QuizBankId { get; set; }

    public DateTime? AvailableOn { get; set; }

    public DateTime? AvailableUntil { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Password> Passwords { get; set; } = new List<Password>();

    public virtual QuizBank QuizBank { get; set; } = null!;

    public virtual ICollection<QuizQuestionBankAssignment> QuizQuestionBankAssignments { get; set; } = new List<QuizQuestionBankAssignment>();

    public virtual Section? Section { get; set; }

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
