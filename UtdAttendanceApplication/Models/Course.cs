using System;
using System.Collections.Generic;

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

    public virtual Professor Prof { get; set; } = null!;

    public virtual ICollection<QuizBank> QuizBanks { get; set; } = new List<QuizBank>();

    public virtual ICollection<Quize> Quizes { get; set; } = new List<Quize>();
}
