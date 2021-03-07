using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
{
    [Table("Sublocation")]
    public partial class Sublocation
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Sublocation Name")]
        [Column("Sublocation")]
        public string SublocationName { get; set; }

        public bool Active { get; set; }

        public string Location { get; set; }

        public string Property { get; set; }
    }
}
