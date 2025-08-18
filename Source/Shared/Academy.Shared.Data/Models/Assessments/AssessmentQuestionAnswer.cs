using Academy.Shared.Data.Models.Accounts;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentQuestionAnswer
    {
        [Required]
        public long                                         UserProfileId               { get; set; }
        public UserProfile?                                 UserProfile                 { get; set; }

        [Required]
        public long                                         AssessmentId                { get; set; }
        public Assessment?                                  Assessment                  { get; set; }

        [Required]
        public long                                         AssessmentQuestionId        { get; set; }

        public ICollection<AssessmentQuestionAnswerOption>  SelectedOptions             { get; set; } = [];
    }
}
