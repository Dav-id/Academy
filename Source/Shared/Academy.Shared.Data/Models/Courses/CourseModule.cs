using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Courses
{
    public class CourseModule : BaseModel
    {

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public long CourseId { get; set; }
        public Course? Course { get; set; }

        //public ICollection<Lesson> Lessons { get; set; } = [];
        //public ICollection<Assessment> Assessments { get; set; } = [];

    }
}
