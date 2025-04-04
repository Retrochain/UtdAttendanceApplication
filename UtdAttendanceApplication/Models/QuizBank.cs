using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuizBank
{
    public int QuizBankId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public int CourseId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
