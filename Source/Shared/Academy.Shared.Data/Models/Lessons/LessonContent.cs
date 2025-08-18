using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{

    public class LessonContent : BaseModel
    {
        [Required]
        public long     LessonId        { get; set; }
        public Lesson?  Lesson          { get; set; }

        [Required]
        [MaxLength(100)]
        public string   ContentType     { get; set; } = string.Empty; // e.g., "Video", "Text", "Interactive"

        [Required]
        public string   ContentData     { get; set; } = string.Empty; // Could be a URL, HTML, Markdown, etc.
    }

}