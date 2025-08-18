using Academy.Shared.Data.Models.Assessments;
using Academy.Shared.Data.Models.Lessons;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Courses
{
    public class CourseModule : BaseModel
    {
        [Required]
        [MaxLength(200)]
        public string                   Title               { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string                   Description         { get; set; } = string.Empty;

        public int                      Order               { get; set; } = 0; // Default order is 0, can be adjusted as needed

        [Required]
        public long                     CourseId            { get; set; }
        public Course?                  Course              { get; set; }

        public ICollection<Lesson>      Lessons             { get; set; } = [];
        public ICollection<Assessment>  Assessments         { get; set; } = [];

    }
}
