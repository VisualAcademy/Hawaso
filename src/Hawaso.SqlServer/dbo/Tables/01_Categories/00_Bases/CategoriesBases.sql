-- 카테고리 테이블: Categories, CategoryBases
CREATE TABLE [dbo].[CategoriesBases]
(
	CategoryId Int Identity(1, 1) Primary Key Not Null, -- 카테고리 번호
	CategoryName NVarChar(50) Not Null                  -- 카테고리 이름
)
Go
