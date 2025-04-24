using System;
using System.Collections.Generic;

namespace UtdAttendanceApplication.Models;

public partial class QuizQuestionBankAssignment
{
    public uint AssignmentId { get; set; }

    public int QuizId { get; set; }

    public int QuestionId { get; set; }

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
