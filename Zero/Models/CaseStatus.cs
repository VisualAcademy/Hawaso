using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
{
    [Table("CaseStatus")]
    public partial class CaseStatus
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Status Name")]
        [Column("CaseStatus")]
        public string CaseStatusName { get; set; }

        public bool Active { get; set; }
    }
}
