using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuizBank
{
    public int QuizBankId { get; set; }

    public int CourseId { get; set; }

    public int SectionId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual Section Section { get; set; } = null!;
}
