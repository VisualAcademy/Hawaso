-- 로그인 리스트의 검색 결과의 레코드 수 반환
CREATE PROCEDURE [dbo].[LoginsSearchCount]
    @SearchField NVarChar(25),
    @SearchQuery NVarChar(25), 

	@UserName NVarChar(100)	-- 사용자 아이디
AS
    Set @SearchQuery = '%' + @SearchQuery + '%'

    Select Count(*)
    From Logins 
    Where
		UserName = @UserName
		And
    (
        Case @SearchField 
            When 'UserName' Then [UserName]
            --When 'Title' Then [Title]
            --When 'Content' Then [Content]
            Else @SearchQuery
        End
    ) 
    Like 
    @SearchQuery
Go
