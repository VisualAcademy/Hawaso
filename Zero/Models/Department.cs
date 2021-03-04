using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
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
