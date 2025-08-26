using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentSectionQuestionAnswerOption : BaseModel
    {
        [Required]
        public long                                 AnswerId       { get; set; }
        public AssessmentSectionQuestionAnswer?     Answer         { get; set; }

        [Required]
        public long                                 OptionId       { get; set; }
        public AssessmentSectionQuestionOption?     Option         { get; set; }
    }
}
