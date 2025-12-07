-- ===================================================================
-- Categories 테이블 생성
-- ===================================================================

--[1] 카테고리: 상품분류
Create Table dbo.Categories
(
    CategoryId      Int Identity(1, 1) Not Null Primary Key,    -- 카테고리번호(Id, CategoryId, ...), 일련번호, GUID 
    CategoryName    NVarChar(50),                               -- 카테고리명
    --
    SuperCategory Int Null	                                    -- 부모카테고리번호(확장용): ParentId, ParentCategoryId로 이름 변경해도 무관
        References Categories(CategoryId),
    Align Int Default(0)                                        -- 카테고리보여지는순서(확장용)
)
Go

--[!] 예시 데이터 입력
-- 아래 카테고리를 표현하고자 한다면???
--	-컴퓨터
--		-데스크톱
--		-노트북
--			-삼성
--			-LG
--	-서적
--	-강의

-- 대분류만 사용한다면... SuperCategory가 Null이면, 최상위 분류, 그렇지 않으면 하위 분류
-- -- 또 다른 방법은 SuperCategory에 -1값을 넣고 구분(프로그램 작성시 훨씬 편함)
--Insert Into Categories Values('컴퓨터', Null, 0)
--Go
--Insert Into Categories Values('서적', Null, 1)
--Go
--Insert Into Categories Values('강의', Null, 2)
--Go

-- 대중소 분류로 확장한다면...
--Insert Into Categories Values('데스크톱', 1, 0)
--Go
--Insert Into Categories Values('노트북', 1, 1)
--Go
--Insert Into Categories Values('삼성', 5, 0)
--Go
--Insert Into Categories Values('LG', 5, 1)
--Go

---- 전체 출력 예시
--Select * From Categories Order By CategoryId Desc
--Go

---- 대분류만 출력 예시
--Select CategoryId, CategoryName From Categories Where SuperCategory Is Null 
--Order By Align Asc
--Go

---- 현재 카테고리의 하위 카테고리 목록을 출력하는 구문
--Declare @SuperCategory Int
--Set @SuperCategory = 5
--Select CategoryId, CategoryName From Categories 
--Where SuperCategory = @SuperCategory
--Go

---- 하위 카테고리 리스트를 출력하는 프로시저
--Create Proc dbo.GetSubCategories
--	@SuperCategory Int
--As
--	Select CategoryID, CategoryName From Categories 
--	Where SuperCategory = @SuperCategory
--Go
--Exec GetSubCategories 1
--Go
