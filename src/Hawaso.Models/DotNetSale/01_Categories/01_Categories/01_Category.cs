using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetSaleCore.Models
{
    /// <summary>
    /// 카테고리(Category) 모델 클래스: Categories 테이블과 일대일로 매핑
    /// </summary>
    [Table("Categories")]
    public class Category
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

        /// <summary>
        /// 부모 카테고리 번호
        /// </summary>
        public int? SuperCategory { get; set; }

        /// <summary>
        /// 카테고리 보여지는 순서
        /// </summary>
        public int? Align { get; set; }

        /// <summary>
        /// 상품(Products) 참조 Navigation Property
        /// </summary>
        public virtual List<Product> Products { get; set; }
    }
}
