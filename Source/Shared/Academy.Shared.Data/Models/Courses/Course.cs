using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Courses
{
    public class Course : BaseModel
    {

        [Required]
        [MaxLength(200)]
        public string                           Title { get; set; }

        [MaxLength(1000)]
        public string                           Description { get; set; }

        public ICollection<CourseModule>        CourseModules { get; set; } = [];
        //public ICollection<CourseEnrollment>    Enrollments { get; set; } = [];
    }

}
