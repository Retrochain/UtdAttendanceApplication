using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int? CourseId { get; set; }

    public int? SectionId { get; set; }

    public int? StudentId { get; set; }

    public int? QuizId { get; set; }

    public string AttendanceStatus { get; set; } = null!;

    public DateTime? Time { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Quize? Quiz { get; set; }

    public virtual Section? Section { get; set; }

    public virtual Student? Student { get; set; }
}
