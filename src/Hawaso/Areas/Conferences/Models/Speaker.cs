namespace Hawaso.Areas.Conferences.Models
{
    public class Speaker
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
    }
}
