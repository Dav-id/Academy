using Academy.Shared.Data.Models.Accounts;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentSectionQuestionAnswer : BaseModel
    {
        [Required]
        public long                                                 UserProfileId                       { get; set; }
        public UserProfile?                                         UserProfile                         { get; set; }

        [Required]
        public long                                                 AssessmentId                        { get; set; }
        public Assessment?                                          Assessment                          { get; set; }

        [Required]
        public long                                                 QuestionId                          { get; set; }
        public AssessmentSectionQuestion?                           Question                            { get; set; }

        // The answer can be of different types depending on the question type
        public bool?                                                BooleanAnswer                       { get; set; }

        // For text-based answers
        public string?                                              TextAnswer                          { get; set; }
        
        // For numeric answers
        public double?                                              NumericAnswer                       { get; set; }
        
        // For single-choice answers
        public long?                                                SelectedOptionAnswerId              { get; set; }
        public AssessmentSectionQuestionOption?                     SelectedOptionAnswer                { get; set; }
        
        // For multiple-choice answers
        public ICollection<AssessmentSectionQuestionAnswerOption>   SelectedOptionAnswers               { get; set; } = [];
    }
}
