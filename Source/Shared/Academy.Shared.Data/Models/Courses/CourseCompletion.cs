using System.ComponentModel.DataAnnotations;
using Academy.Shared.Data.Models.Accounts;

namespace Academy.Shared.Data.Models.Courses
{
    public class CourseCompletion : BaseModel
    {
        [Required]
        public long             UserProfileId   { get; set; }
        public UserProfile?     UserProfile     { get; set; }

        [Required]
        public long             CourseId        { get; set; }
        public Course?          Course          { get; set; }

        [Required]
        public DateTime         SubmittedOn     { get; set; } = DateTime.UtcNow;

        public DateTime?        MarkedOn        { get; set; }

        public long?            MarkedById      { get; set; }
        public UserProfile?     MarkedBy        { get; set; }

        public bool             IsPassed        { get; set; } = false;

        public string           Feedback        { get; set; } = string.Empty;

        [Range(0, 100)]
        public double           FinalScore      { get; set; } = 0;
    }
}
