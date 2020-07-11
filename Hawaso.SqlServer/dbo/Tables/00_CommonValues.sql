--[0] Table: CommonValues(Common Code)
CREATE TABLE [dbo].[CommonValues]
(
	[Id] INT NOT NULL PRIMARY KEY Identity(1, 1),		-- Serial Number
	[ParentId] Int Null,								-- ParentId, AppId, SiteId, ...
	[ParentKey] NVarChar(255) Null,						-- ParentKey == 부모의 GUID
	[Name] NVarChar(255) Not Null,						-- 작성자
	[Title] NVarChar(255) Null,							-- 제목
	[Content] NVarChar(Max) Null,						-- 내용
	[IsPinned] Bit Null Default(0),						-- 공지글로 올리기 
	[CreatedBy] NVarChar(255) Null,						-- 등록자(Creator)
	[Created] DateTime Default(GetDate()) Null,  		-- 생성일(PostDate)
	[ModifiedBy] NVarChar(255) Null,					-- 수정자(LastModifiedBy)
	[Modified] DateTime Null,							-- 수정일(LastModified)

	--[2] 자료실 게시판 관련 주요 컬럼
    FileName        NVarChar(255) Null,                         -- 파일명
    FileSize        Int Default 0,                              -- 파일크기
    DownCount       Int Default 0,                              -- 다운수 

	--[!] 답변형 게시판 관련 주요 컬럼
	Ref Int Null,                               -- 참조(부모글)
	Step Int Default 0,                             -- 답변깊이(레벨)
	RefOrder Int Default 0,                         -- 답변순서

	-- 공통 코드 관리 관련 추가 컬럼
	[Category] NVarChar(50) NULL,
	[SubCategory] NVarChar(50) NULL,
	[VariableText] NVarChar(100) NULL,
	[VariableValue] NVarChar(Max) Null, 
)
Go
