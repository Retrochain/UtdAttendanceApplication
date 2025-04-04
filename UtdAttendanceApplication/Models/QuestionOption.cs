using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuestionOption
{
    public int OptionId { get; set; }

    public int? QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public virtual QuizQuestion? Question { get; set; }

    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
