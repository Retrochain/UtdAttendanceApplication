using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string UtdId { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? MiddleInit { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? Password { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
