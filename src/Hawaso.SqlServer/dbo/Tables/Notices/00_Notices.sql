--[1] Table: Notices(공지사항) 테이블
CREATE TABLE [dbo].[Notices]
(
	[Id] INT NOT NULL PRIMARY KEY Identity(1, 1),		-- Serial Number
	[ParentId] Int Null,								-- ParentId, AppId, SiteId, ...
	[Name] NVarChar(255) Not Null,						-- 작성자
	[Title] NVarChar(255) Null,							-- 제목
	[Content] NVarChar(Max) Null,						-- 내용
	[IsPinned] Bit Null Default(0),						-- 공지글로 올리기 
	[CreatedBy] NVarChar(255) Null,						-- 등록자(Creator)
	[Created] DateTime Default(GetDate()) Null,  		-- 생성일(PostDate)
	[ModifiedBy] NVarChar(255) Null,					-- 수정자(LastModifiedBy)
	[Modified] DateTime Null,							-- 수정일(LastModified)
)
Go
