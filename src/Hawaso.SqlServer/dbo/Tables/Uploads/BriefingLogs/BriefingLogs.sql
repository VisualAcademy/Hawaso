--[1] Table: BriefingLogs(자료실) 테이블
CREATE TABLE [dbo].[BriefingLogs]
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

	--[!] 자료실 소스를 바탕으로 추가 필드 받기 
	[DateTimeStarted] DateTime Default(GetDate()) Null, 
	[Priority] NVarChar(Max) Null, 
)
Go
