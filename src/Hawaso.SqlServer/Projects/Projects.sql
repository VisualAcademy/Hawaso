-- Projects Tables
CREATE TABLE [dbo].[Projects]
(
	[Id] Int Not Null Primary Key Identity(1, 1),	-- 일련번호
	[Title] NVarChar(255) Not Null,					-- 제목
	[Content] NVarChar(Max) Null,					-- Description

	[ManufacturerId] Int Null,					-- Manufacturers.Id 
	[ManufacturerName] NVarChar(255) Null,		-- Manufacturers.Name: Denormalization

	[MachineQuantity] Int Null Default(0),
	[MediaQuantity] Int Null Default(0),
	[Status] NVarChar(255) Null Default('Pending'),

	[IsPinned] Bit Null Default(0),				-- 공지글로 올리기 

	UserId NVarChar(450) Null,
	UserName NVarChar(450) Null, 

	-- AuditableBase.cs 참조
	[CreatedBy] NVarChar(255) Null,			-- 등록자(Creator)
	[Created] DateTime Default(GetDate()),	-- 생성일
	[ModifiedBy] NVarChar(255) Null,		-- 수정자(LastModifiedBy)
	[Modified] DateTime Null,				-- 수정일(LastModified)
)
Go
