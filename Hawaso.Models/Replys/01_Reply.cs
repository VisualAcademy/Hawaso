using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReplyApp.Models
{
    /// <summary>
    /// [!] 기본 클래스: 공통 속성들을 모두 모아 놓은 모델 클래스
    /// MemoBase, ArticleBase, PostBase, EntryBase, ...
    /// </summary>
    public class ReplyBase
    {
        /// <summary>
        /// 일련 번호(Serial Number)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 외래키
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// 외래키
        /// </summary>
        public string ParentKey { get; set; }

        /// <summary>
        /// 이름
        /// </summary>
        [Required(ErrorMessage = "이름을 입력하세요.")]
        [MaxLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// [3] 제목
        /// </summary>
        [MaxLength(255)]
        [Required(ErrorMessage = "제목을 입력하세요.")]
        [Display(Name = "제목")]
        [Column(TypeName = "NVarChar(255)")]
        public string Title { get; set; }

        /// <summary>
        /// [4] 내용
        /// </summary>
        [Display(Name = "내용")]
        public string Content { get; set; }

        /// <summary>
        /// 상단 고정: 공지글로 올리기
        /// </summary>
        public bool? IsPinned { get; set; } = false; 

        /// <summary>
        /// 등록자: CreatedBy, Creator
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// [5] 등록일(생성일): Created
        /// DateTime? 또는 DateTimeOffset? 
        /// </summary>        
        public DateTime? Created { get; set; }

        /// <summary>
        /// 수정자: LastModifiedBy, ModifiedBy
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// 수정일: LastModified, Modified
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// 조회수 
        /// </summary>
        [Display(Name = "조회수")]
        public int ReadCount { get; set; }

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
        public int FileSize { get; set; }

        /// <summary>
        /// 다운수 
        /// </summary>
        [Display(Name = "다운수")]
        public int DownCount { get; set; }
        #endregion

        #region 답변형 게시판 관련 주요 속성
        /// <summary>
        /// 참조(부모글)
        /// </summary>
        public int? Ref { get; set; }

        /// <summary>
        /// 답변깊이(레벨)
        /// </summary>
        public int? Step { get; set; }

        /// <summary>
        /// 답변순서
        /// </summary>
        public int? RefOrder { get; set; } 
        #endregion
    }

    /// <summary>
    /// [1] Model Class: Reply 모델 클래스 == Replys 테이블과 일대일로 매핑
    /// Reply, ReplyModel, ReplyViewModel, ReplyBase, ReplyDto, ReplyEntity, ReplyObject, ...
    /// </summary>
    [Table("Replys")]
    public class Reply : ReplyBase
    {
        // PM> Install-Package System.ComponentModel.Annotations
        // Empty
    }
}
