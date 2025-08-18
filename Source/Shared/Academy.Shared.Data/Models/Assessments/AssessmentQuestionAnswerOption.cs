using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentQuestionAnswerOption : BaseModel
    {
        [Required]
        public long                         AssessmentQuestionAnswerId      { get; set; }
        public AssessmentQuestionAnswer?    AssessmentQuestionAnswer        { get; set; }

        [Required]
        public long                         AssessmentQuestionOptionId      { get; set; }
        public AssessmentQuestionOption?    AssessmentQuestionOption        { get; set; }
    }
}
