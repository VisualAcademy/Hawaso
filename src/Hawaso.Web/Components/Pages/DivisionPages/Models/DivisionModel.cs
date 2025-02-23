using Domain.Entities;

namespace Hawaso.Web.Components.Pages.DivisionPages.Models
{
    /// <summary>
    /// 테이블과 일대일로 매핑되는 모델 클래스: Division, DivisionModel, ...
    /// </summary>
    //[Table("Divisions")]
    public class DivisionModel : Division
    {
        #region +
        ///// <summary>
        ///// 부서 고유 아이디, 자동 증가
        ///// </summary>
        ////[Key]
        ////[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public long Id { get; set; }

        ///// <summary>
        ///// 활성 상태 표시, 기본값 true (활성)
        ///// </summary>
        //public bool? Active { get; set; }

        ///// <summary>
        ///// 레코드 생성 시간
        ///// </summary>
        //public DateTimeOffset CreatedAt { get; set; }

        ///// <summary>
        ///// 레코드 생성자 이름
        ///// </summary>
        //public string? CreatedBy { get; set; }

        ///// <summary>
        ///// 부서명
        ///// </summary>
        //public string? Name { get; set; } 
        #endregion
        public new bool? Active { get; set; }
    }
}
