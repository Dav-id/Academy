using Academy.Shared.Data.Models.Assessments.Enums;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentSectionQuestion : BaseModel
    {
        [Required]
        public long                                             SectionId                           { get; set; }
        public AssessmentSection?                               Section                             { get; set; }
        
        public int                                              Order                               { get; set; } = 0; // Default order is 0, can be adjusted as needed

        [Required]
        public string                                           QuestionText                        { get; set; } = string.Empty;

        public AssessmentQuestionType                           QuestionType                        { get; set; } = AssessmentQuestionType.LongAnswer;

        public ICollection<AssessmentSectionQuestionOption>     Options                             { get; set; } = [];
        public int?                                             MinimumOptionChoiceSelections       { get; set; }
        public int?                                             MaximumOptionChoiceSelections       { get; set; }
    }
}