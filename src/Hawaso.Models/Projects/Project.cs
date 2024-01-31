using Dul.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hawaso.Models
{
    /// <summary>
    /// 프로젝트(Project) 모델 클래스: Projects 테이블과 일대일로 매핑 
    /// </summary>
    [Table("Projects")]
    public class Project : AuditableBase
    {
        /// <summary>
        /// 일련번호
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 제목
        /// </summary>
        //[Required]
        [MaxLength(255)]
        [Required(ErrorMessage = "Add Title Please")]
        public string Title { get; set; }

        /// <summary>
        /// 내용
        /// </summary>
        [Required(ErrorMessage = "Add Descriptoin Please")]
        public string Content { get; set; }

        /// <summary>
        /// 공지글로 올리기
        /// </summary>
        public bool IsPinned { get; set; } = false;

        public int ManufacturerId { get; set; } // Manufacturers.Id 

        public Manufacturer Manufacturer { get; set; }

        [Required(ErrorMessage = "Select Manufacturer Please")]
        public string ManufacturerName { get; set; } // Manufacturers.Name

        public int? MachineQuantity { get; set; }
        public int? MediaQuantity { get; set; }
        public string Status { get; set; } = "Pending"; // TODO: Other Items?

        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
