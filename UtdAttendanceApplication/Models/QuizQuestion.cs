using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuizQuestion
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int? CorrectOption { get; set; }

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual ICollection<QuizBank> QuizBanks { get; set; } = new List<QuizBank>();

    public virtual ICollection<Quize> Quizes { get; set; } = new List<Quize>();

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
