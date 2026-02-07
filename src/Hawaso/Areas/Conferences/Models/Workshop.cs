namespace Hawaso.Areas.Conferences.Models
{
    public class Workshop
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}
