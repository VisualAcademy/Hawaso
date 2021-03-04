using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
{
    [Table("Location")]
    public partial class Location
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Location Name")]
        public string Name { get; set; }

        public bool Active { get; set; }

        public string Property { get; set; }
    }
}
