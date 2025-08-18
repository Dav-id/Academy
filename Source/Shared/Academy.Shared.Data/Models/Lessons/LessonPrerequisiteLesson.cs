using Academy.Shared.Data.Models.Assessments;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{
    public class LessonPrerequisiteLesson : BaseModelNoId
    {
        [Required]
        public long         LessonId                    { get; set; } // The lesson that has a prerequisite
        public Lesson?      Lesson                      { get; set; }

        public int          Order                       { get; set; }

        public long?        PrerequisiteLessonId        { get; set; } // The lesson that must be completed
        public Lesson?      PrerequisiteLesson          { get; set; }
    }
}