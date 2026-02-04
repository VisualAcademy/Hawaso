namespace Hawaso.Areas.Conferences.Models
{
    public class Session
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;     // null 방지
        public string Speaker { get; set; } = string.Empty;   // null 방지
        public string Time { get; set; } = string.Empty;      // null 방지
    }
}
