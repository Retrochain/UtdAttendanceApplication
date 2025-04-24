using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuizQuestion
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int? CorrectOption { get; set; }

    public int QuizBankId { get; set; }

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual QuizBank QuizBank { get; set; } = null!;

    public virtual ICollection<QuizQuestionBankAssignment> QuizQuestionBankAssignments { get; set; } = new List<QuizQuestionBankAssignment>();

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
