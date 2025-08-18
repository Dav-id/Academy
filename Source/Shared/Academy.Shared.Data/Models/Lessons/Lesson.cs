using System.ComponentModel.DataAnnotations;
using Academy.Shared.Data.Models.Courses;

namespace Academy.Shared.Data.Models.Lessons
{
    public class Lesson : BaseModel
    {
    
        [Required]
        [MaxLength(200)]
        public string                       Title                           { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string                       Summary                         { get; set; } = string.Empty;

        [Required]
        public long                         CourseModuleId                  { get; set; }
        public CourseModule?                CourseModule                    { get; set; }

        public ICollection<LessonContent>   Contents                        { get; set; } = [];

        public bool                         QuizzesRequiredForCompletion    { get; set; } = false;
        public ICollection<Quiz>            Quizzes                         { get; set; } = [];
    }
}