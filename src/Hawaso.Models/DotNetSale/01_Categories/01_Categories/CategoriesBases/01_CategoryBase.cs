using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetSaleCore.Models
{
    /// <summary>
    /// 카테고리(Category) 모델 클래스: Categories 테이블과 일대일로 매핑
    /// </summary>
    [Table("CategoriesBases")]
    public class CategoryBase
    {
        /// <summary>
        /// 카테고리 고유 일련번호
        /// </summary>
        [Key]
        public int CategoryId { get; set; }

        /// <summary>
        /// 카테고리 이름
        /// </summary>
        [Required(ErrorMessage = "카테고리 이름을 입력하세요.")]
        public string CategoryName { get; set; }
    }
}
