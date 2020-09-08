-- 하위 카테고리 리스트를 출력하는 프로시저
Create Proc dbo.GetSubCategories
    @SuperCategory Int
As
    Select CategoryId, CategoryName From Categories 
    Where SuperCategory = @SuperCategory
Go
--Exec GetSubCategories 1
--Go
