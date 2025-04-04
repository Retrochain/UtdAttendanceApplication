namespace UtdAttendanceApplication.ViewModels
{
    public class QuizViewModel
    {
        public string? CourseName { get; set; }
        public int CourseSection { get; set; }
        public string? ProfName { get; set; }
        public int QuizBankId { get; set; } // Links the quiz to a specific bank
        public List<QuizQuestionViewModel> Questions { get; set; } = new List<QuizQuestionViewModel>();
    }

    public class QuizQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public List<QuestionOptionViewModel> Options { get; set; } = new List<QuestionOptionViewModel>();
        public int? SelectedOptionId { get; set; } // This will store the student's selected option
    }

    public class QuestionOptionViewModel
    {
        public int OptionId { get; set; }
        public string? OptionText { get; set; }
    }
}
