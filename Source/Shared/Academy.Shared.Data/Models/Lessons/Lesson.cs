using System.ComponentModel.DataAnnotations;

using Academy.Shared.Data.Models.Assessments;
using Academy.Shared.Data.Models.Courses;

namespace Academy.Shared.Data.Models.Lessons
{
    public class Lesson : BaseModel
    {
    
        [Required]
        [MaxLength(200)]
        public string                           Title                           { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string                           Summary                         { get; set; } = string.Empty;

        public int                              Order                           { get; set; } = 0; // Default order is 0, can be adjusted as needed

        public DateTime?                        AvailableFrom                   { get; set; } = null;
        public DateTime?                        AvailableTo                     { get; set; } = null;

        [Required]
        public long                             CourseModuleId                  { get; set; }
        public CourseModule?                    CourseModule                    { get; set; }

        public ICollection<LessonContent>       Contents                        { get; set; } = [];
        
        public ICollection<Assessment>          Assessments                     { get; set; } = [];

        public ICollection<LessonPrerequisite>  Prerequisites                   { get; set; } = [];

        public ICollection<LessonPrerequisite>  RequiredBy                      { get; set; } = [];

    }
}