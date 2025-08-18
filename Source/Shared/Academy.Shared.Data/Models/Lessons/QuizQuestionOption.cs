using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{
    public class QuizAnswerOption : BaseModel
    {
        [Required]
        public long QuizQuestionId { get; set; }
        public QuizQuestion? QuizQuestion { get; set; }

        [Required]
        public string OptionText { get; set; } = string.Empty;

        public double Score { get; set; } = 0.0; // Default score is 0.0, can be adjusted as needed
        public bool IsCorrect { get; set; }
        public int Order { get; set; } = 0; // Default order is 0, can be adjusted as needed
    }
}