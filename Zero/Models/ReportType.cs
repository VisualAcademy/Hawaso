using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
{
    [Table("ReportType")]
    public partial class ReportType
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Report Type")]
        public string TypeName { get; set; }

        public bool Active { get; set; }

        [Display(Name = "Designation")]
        public string TypeDesignation { get; set; }
    }
}
