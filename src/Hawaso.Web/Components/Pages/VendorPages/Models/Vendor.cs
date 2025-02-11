using System.ComponentModel.DataAnnotations;

namespace Hawaso.Web.Components.Pages.VendorPages.Models
{
    public class Vendor
    {
        [Key]
        public long ID { get; set; }

        [Required]
        public bool Active { get; set; } = true;

        [Required]
        public string Name { get; set; }
    }
}
