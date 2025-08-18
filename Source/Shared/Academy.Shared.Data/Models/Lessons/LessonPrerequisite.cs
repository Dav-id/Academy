using System.ComponentModel.DataAnnotations;
using Academy.Shared.Data.Models.Courses;

namespace Academy.Shared.Data.Models.Lessons
{

public class LessonPrerequisite : BaseModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid LessonId { get; set; } // The lesson that has a prerequisite
    public Lesson Lesson { get; set; }

    [Required]
    public Guid PrerequisiteLessonId { get; set; } // The lesson that must be completed first
    public Lesson PrerequisiteLesson { get; set; }
}

}