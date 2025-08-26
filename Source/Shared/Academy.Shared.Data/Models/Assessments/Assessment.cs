using Academy.Shared.Data.Models.Assessments.Enums;
using Academy.Shared.Data.Models.Courses;
using Academy.Shared.Data.Models.Lessons;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Assessments
{

    public class Assessment : BaseModel
    {
        [Required]
        public AssessmentType                               AssessmentType      { get; set; }

        public long?                                        CourseModuleId      { get; set; }
        public CourseModule?                                CourseModule        { get; set; }

        public long?                                        LessonId            { get; set; }
        public Lesson?                                      Lesson              { get; set; }

        [Required]
        [MaxLength(200)]
        public string                                       Title               { get; set; } = string.Empty;
        public string                                       Description         { get; set; } = string.Empty;

        public double?                                      TargetScore         { get; set; } = null;
        public double?                                      PassingScore        { get; set; } = 0.6d; // Default to 60% passing score

        public ICollection<AssessmentSectionQuestion>       Questions           { get; set; } = [];

        public ICollection<LessonPrerequisiteAssessment>    RequiredBy          { get; set; } = [];
    }
}

