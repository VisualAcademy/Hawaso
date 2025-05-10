-- [0][0] 포스트: Posts 
CREATE TABLE [dbo].[Posts]
(
    [Id]           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,               -- 고유 ID
    [Active]       BIT NOT NULL DEFAULT(1),                                 -- 활성 상태
    [IsDeleted]    BIT NOT NULL DEFAULT(0),                                 -- 소프트 삭제
    [Created]      DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),  -- 생성 일시
    [CreatedBy]    NVARCHAR(255) NULL,                                      -- 생성자
    [Name]         NVARCHAR(255) NULL,                                      -- 포스트 이름
    [DisplayOrder] INT NOT NULL DEFAULT(0),                                 -- 정렬 순서
    [FileName]     NVARCHAR(255) NULL,                                      -- 저장된 파일명
    [FileSize]     INT NULL,                                                -- 파일 크기 (bytes)
    [DownCount]    INT NULL,                                                -- 다운로드 횟수
    [ParentId]     BIGINT NULL,                                             -- 연관 부모 ID
    [ParentKey]    NVARCHAR(255) NULL,                                      -- 연관 부모 키

    -- 게시판 스레드 관련 필드
    [Ref]          INT NULL DEFAULT 0,                                      -- 그룹 번호 (원글 기준)
    [Step]         INT NULL DEFAULT 0,                                      -- 들여쓰기 단계
    [RefOrder]     INT NULL DEFAULT 0,                                      -- 그룹 내 정렬 순서
    [AnswerNum]    INT NULL DEFAULT 0,                                      -- 답변 개수
    [ParentNum]    INT NULL DEFAULT 0                                       -- 부모 글 번호 (원글과 연결)
);