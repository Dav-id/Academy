using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentQuestion : BaseModel
    {
        [Required]
        public long                                     AssessmentId                        { get; set; }
        public Assessment?                              Assessment                          { get; set; }
        
        public int                                      Order                               { get; set; } = 0; // Default order is 0, can be adjusted as needed

        [Required]
        public string                                   QuestionText                        { get; set; } = string.Empty;

        public QuizQuestionType                         QuestionType                        { get; set; }

        public ICollection<AssessmentQuestionOption>    Options                             { get; set; } = [];
        public int?                                     MinimumOptionChoiceSelections       { get; set; }
        public int?                                     MaximumOptionChoiceSelections       { get; set; }
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