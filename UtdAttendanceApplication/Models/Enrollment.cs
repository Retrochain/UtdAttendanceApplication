﻿using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int CourseId { get; set; }

    public int StudentId { get; set; }

    public int SectionId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
