-- 로그인 리스트에서 데이터 검색 리스트 
CREATE PROCEDURE [dbo].[LoginsSearchList]
    @SearchField NVarChar(25),
    @SearchQuery NVarChar(25),
    @PageNumber Int = 1,
    @PageSize Int = 10,

	@UserName NVarChar(100)	-- 사용자 아이디
AS
    Select 
		* 
    From Logins
    Where 
		UserName = @UserName
		And	
	( 
        Case @SearchField 
            When 'UserName' Then [UserName] 
            Else 
            @SearchQuery 
        End 
    ) Like '%' + @SearchQuery + '%'
    Order By LoginId Desc
    Offset ((@PageNumber - 1) * @PageSize) Rows Fetch Next @PageSize Rows Only;
Go
