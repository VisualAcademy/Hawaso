using System;

namespace Domain.Entities
{
    public class Division
    {
        /// <summary>
        /// 부서 고유 아이디, 자동 증가
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 활성 상태 표시, 기본값 true (활성)
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// 레코드 생성 시간
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 레코드 생성자 이름
        /// </summary>
        public string CreatedBy { get; set; } = "";

        /// <summary>
        /// 부서명
        /// </summary>
        public string Name { get; set; } = "";
    }
}
