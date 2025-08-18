using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{
    public class QuizQuestion : BaseModel
    {
        [Required]
        public long QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        [Required]
        public string QuestionText { get; set; }

        public QuizQuestionType QuestionType { get; set; }

        public ICollection<QuizAnswerOption> Options { get; set; } = [];
    }

    public enum QuizQuestionType
    {
        MultipleChoice,
        SingleChoice,
        TrueFalse,
        ShortAnswer,
        Essay
    }
}