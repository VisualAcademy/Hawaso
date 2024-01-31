-- 로그인 리스트에서 데이터 검색 리스트 
CREATE PROCEDURE [dbo].[LoginsSearchListAdmin]
    @SearchField NVarChar(25),
    @SearchQuery NVarChar(25),
	@StartDate DateTime,
	@EndDate DateTime,
    @PageNumber Int = 1,
    @PageSize Int = 10
AS
    Select 
		* 
    From Logins
    Where 
	( 
        Case @SearchField 
            When 'UserName' Then [UserName] 
            When 'LoginIp' Then LoginIp 
            Else 
            @SearchQuery 
        End 
    ) Like '%' + @SearchQuery + '%'
	And 
	LoginDate Between @StartDate And @EndDate 
    Order By LoginId Desc
    Offset ((@PageNumber - 1) * @PageSize) Rows Fetch Next @PageSize Rows Only;
Go
