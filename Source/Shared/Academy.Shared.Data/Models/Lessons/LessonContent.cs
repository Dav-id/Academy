using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Lessons
{

    public class LessonContent : BaseModel
    {
        [Required]
        public long LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } // e.g., "Video", "Text", "Interactive"

        [Required]
        public string ContentData { get; set; } // Could be a URL, HTML, Markdown, etc.
    }

}