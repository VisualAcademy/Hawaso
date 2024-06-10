using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hawaso.Models
{
    /// <summary>
    /// [!] 기본 클래스: 공통 속성들을 모두 모아 놓은 만능 모델 클래스
    /// MemoBase, ArticleBase, PostBase, EntryBase, ArchiveBase, ActBase, ...
    /// Scaffold-DbContext: https://docs.microsoft.com/ko-kr/ef/core/cli/powershell#scaffold-dbcontext
    /// </summary>
    public class MemoBase
    {
        #region Key Section
        /// <summary>
        /// [1] 일련 번호(Serial Number)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "번호")]
        public long Id { get; set; } // Id 속성(컬럼)을 int로 사용해야하나? long으로 사용해야하나? 이것이 문제로다. 

        /// <summary>
        /// 숫자 형식의 외래키? - AppId 형태로 ParentId와 ParentKey 속성은 보조로 만들어 놓은 속성
        /// </summary>
        public long? ParentId { get; set; } = default;  // long? 형식으로 변경 가능    

        /// <summary>
        /// 숫자 형식의 외래키? - AppId 형태로 ParentId와 ParentKey 속성은 보조로 만들어 놓은 속성
        /// </summary>
        public string ParentKey { get; set; } = string.Empty; 
        #endregion

        #region Auditable
        /// <summary>
        /// 등록자: CreatedBy, Creator, Email, ...
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// [5] 등록일(생성일): Created
        /// DateTime? 또는 DateTimeOffset? 
        /// </summary>        
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// 수정자: LastModifiedBy, ModifiedBy
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 수정일: LastModified, Modified
        /// </summary>
        public DateTimeOffset? Modified { get; set; }
        #endregion

        #region [0] 5W1H: 누가, 언제, 어디서, 무엇을, 어떻게, 왜
        /// <summary>
        /// [2] 이름(작성자)
        /// </summary>
        [Required(ErrorMessage = "이름을 입력하세요.")]
        [MaxLength(255)]
        [Display(Name = "작성자")]
        [Column(TypeName = "NVarChar(255)")]
        public string Name { get; set; } = string.Empty; 

        /// <summary>
        /// 작성일
        /// </summary>
        [Display(Name = "작성일")]
        public DateTime? PostDate { get; set; }

        /// <summary>
        /// 작성지 IP 주소
        /// </summary>
        [Display(Name = "작성IP")]
        [Column(TypeName = "NVarChar(255)")]
        public string PostIp { get; set; }

        /// <summary>
        /// [3] 제목
        /// </summary>
        [MaxLength(255)]
        [Required(ErrorMessage = "제목을 입력하세요.")]
        [Display(Name = "제목")]
        [Column(TypeName = "NVarChar(255)")]
        public string Title { get; set; } = string.Empty; 

        /// <summary>
        /// [4] 내용
        /// </summary>
        [Display(Name = "내용")]
        public string Content { get; set; }

        /// <summary>
        /// 카테고리: Notice, Free, Data, Photo, ...
        /// </summary>
        [Display(Name = "카테고리")]
        public string Category { get; set; }
        #endregion

        #region [1] 기본형 게시판 관련 주요 컬럼
        /// <summary>
        /// 작성자 이메일
        /// </summary>
        //[EmailAddress(ErrorMessage = "* 이메일을 정확히 입력하세요.")]
        public string Email { get; set; }

        /// <summary>
        /// 비밀번호
        /// </summary>
        [Display(Name = "비밀번호")]
        [Required(ErrorMessage = "* 비밀번호를 작성해 주세요.")]
        public string Password { get; set; }

        /// <summary>
        /// 조회수
        /// </summary>
        [Display(Name = "조회수")]
        public int? ReadCount { get; set; }

        /// <summary>
        /// 인코딩: Text(Plain-Text), HTML(Text/HTML), Mixed(Mixed-Text)
        /// </summary>
        [Display(Name = "인코딩")]
        public string Encoding { get; set; } = "Plain-Text"; // "Text"

        /// <summary>
        /// 홈페이지 
        /// </summary>
        [Display(Name = "홈페이지")]
        public string Homepage { get; set; } // URL

        /// <summary>
        /// 수정일
        /// </summary>
        [Display(Name = "수정일")]
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// 수정 IP 주소
        /// </summary>
        [Display(Name = "수정IP")]
        public string ModifyIp { get; set; }

        /// <summary>
        /// 댓글수 
        /// </summary>
        [Display(Name = "댓글수")]
        public int? CommentCount { get; set; }

        /// <summary>
        /// 상단 고정: 공지글로 올리기, IsActive
        /// </summary>
        public bool IsPinned { get; set; } = false;
        #endregion

        #region [2] 자료실 게시판 관련 주요 컬럼
        /// <summary>
        /// 파일이름
        /// </summary>
        [Display(Name = "파일이름")]
        public string FileName { get; set; }

        /// <summary>
        /// 파일크기
        /// </summary>
        [Display(Name = "파일크기")]
        public int? FileSize { get; set; }

        /// <summary>
        /// 다운수 
        /// </summary>
        [Display(Name = "다운수")]
        public int? DownCount { get; set; }
        #endregion

        #region 답변형 게시판 관련 주요 속성
        /// <summary>
        /// 참조(부모글, 참조 번호)
        /// 그룹ID: 같은 그룹이면 모두 동일한 값 
        /// https://youtu.be/1B4AjvD7s1g
        /// </summary>
        public int Ref { get; set; } = 0;

        /// <summary>
        /// 답변깊이(레벨, 들여쓰기)
        /// </summary>
        public int Step { get; set; } = 0;

        /// <summary>
        /// 답변(참조) 순서
        /// </summary>
        public int RefOrder { get; set; } = 0;

        /// <summary>
        /// 답변수
        /// </summary>
        [Display(Name = "답변수")]
        public int AnswerNum { get; set; } = 0;

        /// <summary>
        /// 부모글 번호
        /// </summary>
        [Display(Name = "부모번호")]
        public int ParentNum { get; set; } = 0; 
        #endregion
    }

    /// <summary>
    /// [1] Model Class: Memo 모델(도메인, 엔터티) 클래스 == Memos 테이블과 일대일로 매핑
    /// Memo, MemoModel, MemoViewModel, MemoDto, MemoEntity, MemoObject, MemoTable 
    /// MemoDomain, ...
    /// </summary>
    [Table("Memos")]
    public class Memo : MemoBase
    {
        // PM> Install-Package System.ComponentModel.Annotations
        // Empty
    }
}
