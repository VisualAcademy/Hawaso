--[5] 고객(Customers) 테이블: 회원 또는 비회원 중에서 물품을 구매한 사람
-- Customers: Id, FirstName, LastName, StreetAddress, City, StateOrProvinceAbbr, Country, PostalCode, Phone, Email
Create Table dbo.Customers 
(
    CustomerId Int Identity(1, 1) Not Null Primary Key,			--고객 고유 번호
    CustomerName NVarChar(50),			                        --고객명(회원/비회원)
    EmailAddress NVarChar(50),			                        --이메일

    [Address] NVarChar(100) Null,                               --주소
    AddressDetail NVarChar(100) Null,	                        --주소 상세
    Phone1 NVarChar(4),					                        --전화번호1
    Phone2 NVarChar(4),					                        --전화번호2
    Phone3 NVarChar(4),					                        --전화번호3
    Mobile1 NVarChar(4),					                    --휴대폰1
    Mobile2 NVarChar(4),					                    --휴대폰2
    Mobile3 NVarChar(4),					                    --휴대폰3
    Zip NVarChar(255) Null,				                        --우편번호(예전에는 7자리면 충분)
    Ssn1 Char(6) Null,					                        --주민번호 앞자리
    Ssn2 Char(7) Null,					                        --주민번호 뒷자리
    MemberDivision Int,					                        --회원구분(0:비회원,1:회원)

    ----------------------------------------------------------------------

    FirstName NVarChar(255) Null,                               -- 이름
    LastName NVarChar(255) Null,                                -- 성
    Gender NVarChar(255) Null,                                  -- 성별
    City NVarChar(255) Null,                                    -- 도시

	[CreatedBy] NVarChar(255) Null,			                    -- 등록자(Creator)
	[Created] DateTime Default(GetDate()),	                    -- 생성일
	[ModifiedBy] NVarChar(255) Null,		                    -- 수정자(LastModifiedBy)
	[Modified] DateTime Null,				                    -- 수정일(LastModified)
)
Go

-- 예시 데이터 입력
--Insert Into Customers Values (
--    '홍길동', '032', '123', '1234', '010', '123', '1234', '404-230', '인천 서구 가정동'
--    '1234번지', '820205', '1451330', 'ceo@hawaso.com', 1
--)

--Select * From Customers
--Go
