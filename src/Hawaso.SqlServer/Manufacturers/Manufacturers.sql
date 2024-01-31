-- 고객사 리스트 관리 앱
CREATE TABLE [dbo].[Manufacturers]
(
	[Id] INT NOT NULL PRIMARY KEY Identity(1, 1),
	Name NVarChar(255) Not Null,
	ManufacturerCode NVarChar(255) Null,
	Comment NVarChar(Max) Null,
)
