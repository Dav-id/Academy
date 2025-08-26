using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{

    public class LessonSectionContent : BaseModel
    {
        [Required]
        public long             LessonSectionId        { get; set; }
        public LessonSection?   LessonSection          { get; set; }

        [Required]
        [MaxLength(100)]
        public string           ContentType             { get; set; } = string.Empty; // e.g., "Video", "Text", "Interactive"

        [Required]
        public string           ContentData             { get; set; } = string.Empty; // Could be a URL, HTML, Markdown, etc.
    }

}