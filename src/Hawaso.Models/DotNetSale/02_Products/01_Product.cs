using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetSaleCore.Models
{
    /// <summary>
    /// [2] Model Class: 상품의 필드를 구성하는 상품 상세 클래스
    /// </summary>
    [Table("Products")]
    public class Product
    {
        /// <summary>
        /// 일련번호
        /// </summary>
        [Key]
        public int ProductId { get; set; }

        /// <summary>
        /// 카테고리ID
        /// </summary>
        [Required(ErrorMessage = "카테고리를 선택하세요.")]
        public int CategoryId { get; set; }

        [JsonIgnore]
        public virtual Category Category { get; set; }

        /// <summary>
        /// 상품명
        /// </summary>
        [Required(ErrorMessage = "상품명 입력하세요.")]
        public string ModelName { get; set; }
        
        /// <summary>
        /// 상품코드
        /// </summary>
        [Required(ErrorMessage = "상품코드를 입력하세요.")]
        public string ModelNumber { get; set; }

        /// <summary>
        /// 회사명
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 원가
        /// </summary>
        public int OriginPrice { get; set; } = 0;

        /// <summary>
        /// 판매가
        /// </summary>
        public int SellPrice { get; set; } = 0; 

        /// <summary>
        /// 이벤트 종류
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 이미지 이름
        /// </summary>
        public string ProductImage { get; set; }

        /// <summary>
        /// 짧은 설명
        /// </summary>
        public string Explain { get; set; }

        /// <summary>
        /// 긴 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 본문 내용 표시 형식
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// 재고 수량: Inventory, Stock
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// 상품 등록일
        /// </summary>
        public DateTime? RegistDate { get; set; }

        /// <summary>
        /// 마일리지
        /// </summary>
        public int? Mileage { get; set; }

        /// <summary>
        /// 품절여부
        /// </summary>
        public int? Absence { get; set; }

        //public decimal Price { get; set; }
        //public bool IsAvailable { get; set; }
    }
}
