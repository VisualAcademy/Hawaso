namespace Hawaso.Models;

public class Photo
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long? EmployeeId { get; set; }
}
