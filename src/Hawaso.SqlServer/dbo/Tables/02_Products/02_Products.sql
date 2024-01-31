--[2][1] 상품
-- Products: Id, Name, Price
Create Table dbo.Products 
(
  ProductId Int Identity(1, 1) Not Null Primary Key ,       --상품고유코드
  CategoryId Int Not Null,                                  --카테고리번호
  ModelName NVarChar(50) Null,                              --모델명(상품명)
  -- 
  ModelNumber NVarChar(50) Null,                            --모델번호
  Company NVarChar(50) Null,                                --제조회사
  OriginPrice Int Null,                                     --원가
  SellPrice Int Null,                                       --판매가
  EventName NVarChar(50) Null,         
    --신상품(NEW),히트(HIT),기획(BEST),진열없음(NONE),...
  ProductImage NVarChar(50) Null,                           --이미지명(큰/상세/리스트)
  Explain NVarChar(400) Null,                               --요약설명
  [Description] Text Null,                                  --상세설명 : NVarChar(4000)
  Encoding NVarChar(10) Null,                               --인코딩(Text/HTML/Mixed)
  ProductCount Int Default(0) Null,                         --재고수량, Inventory, Stock
  RegistDate DateTime Default(GetDate()) Null,              --상품등록일
  Mileage Int Null,                                         --마일리지(적립금)
  Absence Int Null                                          --품절여부(1:품절)
)
Go 

--[!] 외래키 따로 지정
Alter Table dbo.Products
Add Foreign Key(CategoryId) References Categories(CategoryId)
Go

----[!] 예시 데이터 입력
--Insert Into Products Values (1, 'COM-01', '좋은컴퓨터', '우리집', 10000, 8000, 
--'NEW', 'COM-01.JPG', '좋은컴퓨터입니다.', '좋은컴퓨터입니다...', 'Text', 100, GetDate(), 0, 0)
--Go
--Insert Into Products Values (2, 'BOOK-01', '좋은책', '우리집', 8000, 5000, 
--'HIT', 'BOOK-01.JPG', '좋은책입니다.', '좋은책입니다...', 'Text', 100, GetDate(), 0, 0)
--Go
--Insert Into Products Values (3, 'SOFTWARE-01', '좋은강의', '우리집', 10000, 8000, 
--'BEST', 'SOFTWARE-01.JPG', '좋은강의입니다.', '좋은강의입니다...', 'Text', 100, GetDate(), 0, 0)
--Go

---- 전체 상품 리스트 출력
--Select * From Products Order By ProductId Desc
--Go


---- 카테고리에 따른 상품리스트 
--Declare @CategoryId Int
--Set @CategoryId = 1
--Select * From Products Where CategoryId = @CategoryId Order By ProductId Desc
--Go

---- Case 표현식으로 카테고리 표현: 조건에 따른 값 출력
--Select ProductId, ModelName, CategoryId,
--	Case CategoryId
--		When 1 Then '컴퓨터'
--		When 2 Then '서적'
--		When 3 Then '강의'
--	End As CategoryName
--From Products;

