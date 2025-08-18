using Academy.Shared.Data.Models.Accounts;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Courses
{
    public class CourseEnrollment : BaseModel
    {

        [Required]
        public long         UserProfileId       { get; set; }
        public UserProfile? UserProfile         { get; set; }

        [Required]
        public long         CourseId            { get; set; }
        public Course?      Course              { get; set; }

        [Required]
        public DateTime     EnrolledOn          { get; set; }

        public bool         IsCompleted         { get; set; }
    }
}
