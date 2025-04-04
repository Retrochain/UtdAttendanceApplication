using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuizBank
{
    public int QuizBankId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public int CourseId { get; set; }

    public int QuestionId { get; set; }

    public int SectionId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;
}
