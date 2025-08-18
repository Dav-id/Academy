using System.ComponentModel.DataAnnotations;
using Academy.Shared.Data.Models.Accounts;

namespace Academy.Shared.Data.Models.Lessons
{
    public class LessonCompletion : BaseModel
    {
        [Required]
        public long UserProfileId { get; set; }
        public UserProfile? UserProfile { get; set; }

        [Required]
        public long LessonId { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
