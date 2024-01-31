using System;

namespace Hawaso.Models
{
    /// <summary>
    /// Login 모델: Logins 테이블과 일대일로 매핑 
    /// </summary>
    public class Login
    {
        /// <summary>
        /// 일련번호 
        /// </summary>
        public int LoginId { get; set; }

        /// <summary>
        /// 로그인 사용자의 Users.Id / Users.UserId
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 로그인 사용자의 아이디/이름
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 로그인 IP 주소 
        /// </summary>
        public string LoginIp { get; set; }

        /// <summary>
        /// 로그인 일시 
        /// </summary>
        public DateTimeOffset LoginDate { get; set; }
    }
}
