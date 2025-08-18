using Academy.Shared.Data.Models.Assessments;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{
    public class LessonPrerequisite : BaseModel
    {
        [Required]
        public long         LessonId                    { get; set; } // The lesson that has a prerequisite
        public Lesson?      Lesson                      { get; set; }

        public int          Order                       { get; set; }

        public long?        PrerequisiteLessonId        { get; set; } // The lesson that must be completed
        public Lesson?      PrerequisiteLesson          { get; set; }
        
        public long?        PrerequisiteAssessmentId    { get; set; } // The quiz that must be completed 
        public Assessment?  PrerequisiteAssessment      { get; set; }
    }
}