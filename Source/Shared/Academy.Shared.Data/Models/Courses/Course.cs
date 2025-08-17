using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Courses
{
    public class Course : BaseModel
    {

        [Required]
        [MaxLength(200)]
        public string                           Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string                           Description { get; set; } = string.Empty;

        public ICollection<CourseModule>        CourseModules { get; set; } = [];
        //public ICollection<CourseEnrollment>    Enrollments { get; set; } = [];
    }

}
