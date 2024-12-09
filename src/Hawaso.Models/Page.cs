using System;

namespace Hawaso.Models
{
    public class Page
    {
        public int Id { get; set; }
        public string TenantName { get; set; } = "Hawaso"; // 기본 테넌트명 Hawaso
        public string PageName { get; set; } = "Contact"; // 기본 페이지명 Contact
        public string Title { get; set; } = string.Empty; // 페이지 제목
        public string Content { get; set; } = string.Empty; // 페이지 본문
        public DateTime LastUpdated { get; set; } // 마지막 수정 날짜
    }
}
