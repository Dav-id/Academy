using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{

    public class LessonSection : BaseModel
    {
        [Required]
        public long                                 LessonId        { get; set; }
        public Lesson?                              Lesson          { get; set; }

        [Required]
        [MaxLength(200)]
        public string                               Title           { get; set; } = string.Empty;
        public string                               Description     { get; set; } = string.Empty;

        public bool                                 NewScreen       { get; set; } = false;

        public int                                  Order           { get; set; } = 0; // Default order is 0, can be adjusted as needed
        public ICollection<LessonSectionContent>    Contents        { get; set; } = [];

    }

}