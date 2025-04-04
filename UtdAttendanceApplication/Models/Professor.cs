using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class Professor
{
    public int ProfessorId { get; set; }

    public string UtdId { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? MiddleInit { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? Password { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
