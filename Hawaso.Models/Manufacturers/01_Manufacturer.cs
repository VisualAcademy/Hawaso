using System.ComponentModel.DataAnnotations;

namespace Hawaso.Models
{
    /// <summary>
    /// 고객사 리스트 관리 앱
    /// </summary>
    public class Manufacturer
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ManufacturerCode { get; set; }
        public string Comment { get; set; }
    }
}
