------------------------------------------------------------------------------------------------------
-- Table: Alls
-- Filename: Alls.sql
-- Purpose: Unified Board Table
-- Description:
--   Alls table integrates all necessary columns from multiple board systems
--   (Private Board, Acts, Memos) including practical extensions for real-world use:
--     - Soft Delete
--     - Approval Workflow
--     - User Agent & Session Tracking
--     - Display Order & Permissions
--     - Statistics (Likes, Dislikes, Rating)
--     - Multilingual Support
-- Note:
--   - All columns included without omission
--   - All comments and section divisions maintained
--   - Optimized for future scalability
-- Modified: 2025-04-05
------------------------------------------------------------------------------------------------------

CREATE TABLE [dbo].[Alls]
(
    --------------------------------------------------------------------------------------------------
    --[1] 공통 기본 키
    [Id]            BIGINT NOT NULL PRIMARY KEY Identity(1, 1), -- [1][일련번호], Serial Number
    --------------------------------------------------------------------------------------------------

    --[0] Parent 관련
    [ParentId]      BIGINT NULL,                                -- ParentId, AppId, SiteId, ProductId, CategoryId, ...
    [ParentKey]     NVarChar(255) NULL,                         -- ParentKey == 부모의 GUID, UserId, RoleId, ...

    --------------------------------------------------------------------------------------------------
    --[0] Auditable
    [CreatedBy]     NVarChar(255) NULL,                         -- 등록자 (NOT NULL recommended)
    [Created]       DateTimeOffset DEFAULT(SYSDATETIMEOFFSET()) NULL, -- 생성일 (NOT NULL recommended)
    [ModifiedBy]    NVarChar(255) NULL,                         -- 수정자
    [Modified]      DateTimeOffset NULL,                        -- 수정일

    --------------------------------------------------------------------------------------------------
    --[0] 5W1H: 누가, 언제, 어디서, 무엇을, 어떻게, 왜
    [Name]          NVarChar(255) NULL,                         -- [2][이름](작성자) (NOT NULL recommended)
    [PostDate]      DateTime DEFAULT GetDate() NULL,            -- 작성일 (NOT NULL recommended)
    [PostIp]        NVarChar(20) NULL,                          -- 작성IP
    [Title]         NVarChar(512) NULL,                         -- [3][제목] (NOT NULL recommended)
    [Content]       NText NULL,                                 -- [4][내용] (NOT NULL recommended)
    [Category]      NVarChar(255) DEFAULT('Free') NULL,         -- 카테고리 (Default: 'Free')

    --------------------------------------------------------------------------------------------------
    --[1] 기본형 게시판 관련 주요 컬럼
    Email           NVarChar(255) NULL,                         -- 이메일
    [Password]      NVarChar(255) NULL,                         -- 비밀번호
    ReadCount       Int DEFAULT 0 NULL,                         -- 조회수
    Encoding        NVarChar(20) DEFAULT('HTML') NULL,          -- 인코딩 (Default: 'HTML')
    Homepage        NVarChar(100) NULL,                         -- 홈페이지
    ModifyDate      DateTime NULL,                              -- 수정일
    ModifyIp        NVarChar(15) NULL,                          -- 수정IP
    CommentCount    Int DEFAULT 0 NULL,                         -- 댓글수
    IsPinned        Bit DEFAULT 0 NULL,                         -- 공지글 (Default: 0)

    --------------------------------------------------------------------------------------------------
    --[2] 자료실(Upload, BBS) 게시판 관련 주요 컬럼
    FileName        NVarChar(255) NULL,                         -- 파일명
    FileSize        Int DEFAULT 0 NULL,                         -- 파일크기
    DownCount       Int DEFAULT 0 NULL,                         -- 다운수

    --------------------------------------------------------------------------------------------------
    --[3] 답변형(Reply, Qna) 게시판 관련 주요 컬럼
    Ref             Int DEFAULT 0 NULL,                         -- 참조(부모글), Group (Default: 0) (NOT NULL recommended)
    Step            Int DEFAULT 0 NULL,                         -- 답변깊이 (Default: 0)
    RefOrder        Int DEFAULT 0 NULL,                         -- 답변순서 (Default: 0)
    AnswerNum       Int DEFAULT 0 NULL,                         -- 답변수 (Default: 0)
    ParentNum       Int DEFAULT 0 NULL,                         -- 부모글번호 (Default: 0)

    --------------------------------------------------------------------------------------------------
    --[!] 추가 항목들
    [Status]        NVarChar(255) NULL,                         -- 상태 (draft, final, archive)
    TenantId        BigInt DEFAULT 0 NULL,
    TenantName      NVarChar(255) NULL,
    AppId           Int DEFAULT 0 NULL,
    AppName         NVarChar(255) NULL,
    ModuleId        Int DEFAULT 0 NULL,
    ModuleName      NVarChar(255) NULL,

    --------------------------------------------------------------------------------------------------
    --[4] 기타 게시판 기능 확장
    IsLocked        Bit DEFAULT 0 NULL,
    Vote            Int DEFAULT 0 NULL,
    Weather         TinyInt DEFAULT 0 NULL,
    ReplyEmail      Bit DEFAULT 0 NULL,
    Published       Bit DEFAULT 0 NULL,
    BoardType       NVarChar(100) NULL,
    BoardName       NVarChar(255) NULL,
    NickName        NVarChar(255) NULL,
    IconName        NVarChar(100) NULL,
    Price           DECIMAL(18, 2) DEFAULT 0.00 NULL,           -- 가격 (Default: 0.00)
    Community       NVarChar(255) NULL,

    --------------------------------------------------------------------------------------------------
    --[$] 일정 관리
    StartDate       DateTimeOffset(7) NULL,
    EndDate         DateTimeOffset(7) NULL,

    --------------------------------------------------------------------------------------------------
    --[@] 동영상 게시판
    Video           NVarChar(1024) NULL,                        -- 유튜브 동영상 경로

    --------------------------------------------------------------------------------------------------
    --[!] 보안 및 접근 권한
    SecurityLevel   NVarChar(10) NULL,
    AvailableCustomerLevel NVarChar(10) NULL,

    --------------------------------------------------------------------------------------------------
    --[!] 기타 확장 기능
    Num             Int DEFAULT 0 NULL,                         -- 번호 (Default: 0)
    UID             Int DEFAULT 0 NULL,                         -- Users UID
    UserId          NVarChar(255) NULL,                         -- 사용자 테이블 Id
    UserName        NVarChar(255) NULL,                         -- 사용자 아이디
    DivisionId      Int DEFAULT 0 NULL,                         -- 서브 카테고리
    CategoryId      Int DEFAULT 0 NULL,                         -- 카테고리 테이블 Id
    BoardId         Int DEFAULT 0 NULL,                         -- 게시판 테이블 Id
    ApplicationId   Int DEFAULT 0 NULL,                         -- 응용 프로그램 Id

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 소프트 삭제 및 승인 관리
    IsDeleted       Bit DEFAULT 0 NULL,                         -- 소프트 삭제 (Default: 0)
    DeletedBy       NVarChar(255) NULL,                         -- 삭제자
    Deleted         DateTimeOffset NULL,                        -- 삭제 일자

    ApprovalStatus  NVarChar(50) NULL,                          -- 승인 상태 (Pending, Approved, Rejected)
    ApprovalBy      NVarChar(255) NULL,                         -- 승인자
    ApprovalDate    DateTimeOffset NULL,                        -- 승인 일자

    Active          Bit DEFAULT 1 NULL,                         -- 활성 상태 (기본값: true)

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 사용자 추적 정보
    UserAgent       NVarChar(512) NULL,                         -- 브라우저/OS 정보
    Referer         NVarChar(512) NULL,                         -- 유입 경로
    SessionId       NVarChar(255) NULL,                         -- 세션 식별자

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 노출 제어 및 태그
    DisplayOrder    Int DEFAULT 0 NULL,                         -- 노출 순서 (SortOrder)
    ViewRoles       NVarChar(255) NULL,                         -- 열람 권한 (복수 역할)
    Tags            NVarChar(255) NULL,                         -- 태그

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 통계 및 평점
    LikeCount       Int DEFAULT 0 NULL,                         -- 좋아요
    DislikeCount    Int DEFAULT 0 NULL,                         -- 싫어요
    Rating          Decimal(3,2) DEFAULT 0.0 NULL,              -- 평점

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 다국어 지원
    Culture         NVarChar(10) NULL,                           -- 다국어 코드 (ko-KR, en-US 등)

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 고정값/상수 관리용 구분자
    IsSystem        Bit DEFAULT 0 NULL,                         -- 시스템 항목 여부 (예: 삭제 불가, 수정 제한)

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 검색/정렬 최적화용 인덱싱 컬럼
    SearchKeywords  NVarChar(1024) NULL,                       -- 검색 키워드용 통합 텍스트
    SortKey         NVarChar(255) NULL,                         -- 사용자 지정 정렬 키

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 버전 관리/히스토리
    Version         Int DEFAULT 1 NULL,                         -- 버전 번호 (예: 문서 개정)
    HistoryGroupId  UniqueIdentifier NULL,                     -- 동일 문서 그룹 구분자 (버전 관리 시)

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 알림/구독 관련
    IsNotified      Bit DEFAULT 0 NULL,                         -- 알림 여부 (예: 관리자 확인)
    IsSubscribed    Bit DEFAULT 0 NULL,                         -- 사용자 구독 여부

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 외부 연동 또는 확장
    ExternalId      NVarChar(255) NULL,                         -- 외부 시스템 연동용 ID
    ExternalUrl     NVarChar(1024) NULL,                        -- 외부 리소스 링크

    --------------------------------------------------------------------------------------------------
    --[+] ★ 확장 추가: 모바일/앱/API 구분
    SourceType      NVarChar(50) NULL,                          -- 작성 원본 (Web, Mobile, API 등)
    IsMobile        Bit DEFAULT 0 NULL,                         -- 모바일 작성 여부
)
GO
