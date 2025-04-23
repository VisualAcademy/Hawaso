using System;
using System.ComponentModel.DataAnnotations;

namespace All.Entities
{
    public class AllowedIpRange
    {
        public int Id { get; set; }

        [Display(Name = "Start IP Range")]
        public string StartIpRange { get; set; }

        [Display(Name = "End IP Range")]
        public string EndIpRange { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Create Date")]
        public DateTime CreateDate { get; set; } = DateTime.Now; // 기본값 설정

        [Display(Name = "Tenant ID")]
        public long TenantId { get; set; }
    }
}
