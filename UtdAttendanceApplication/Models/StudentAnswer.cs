using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class StudentAnswer
{
    public int AnswerId { get; set; }

    public int QuizId { get; set; }

    public int StudentId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public sbyte IsCorrect { get; set; }

    public DateTime? SubmittedOn { get; set; }

    public string IpAddress { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual QuestionOption? SelectedOption { get; set; }

    public virtual Student Student { get; set; } = null!;
}
