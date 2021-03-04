using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hawaso.Models
{
    [Table("Department")]
    public partial class Department
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Department")]
        public string Dept { get; set; }

        public bool Active { get; set; }
    }
}
