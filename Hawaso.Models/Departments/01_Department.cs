using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hawaso.Models
{
    /// <summary>
    /// [1][1] 테이블과 일대일로 매핑되는 모델 클래스
    /// </summary>
    [Table("Departments")]
    public class DepartmentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public bool? Active { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public string Name { get; set; }
    }
}
