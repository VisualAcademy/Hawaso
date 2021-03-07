using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zero.Models
{
    [Table("ReportSpecific")]
    public partial class ReportSpecific
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Report Specific")]
        public string Specific { get; set; }

        public bool Active { get; set; }

        public ReportType ReportType { get; set; }
        [Display(Name = "ReportType ???")]
        public int? ReportTypeId { get; set; }
    }
}
