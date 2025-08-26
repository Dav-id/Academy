using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{
    public class AssessmentSection : BaseModel
    {
        [Required]
        public long                                     AssessmentId                        { get; set; }
        public Assessment?                              Assessment                          { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string                                   Title                               { get; set; } = string.Empty;

        public string                                   Description                         { get; set; } = string.Empty;

        public bool                                     NewScreen                           { get; set; } = false;
        public int                                      Order                               { get; set; } = 0;

        public ICollection<AssessmentSectionQuestion>   Questions                           { get; set; } = [];

    }
}