-- 로그인 리스트의 검색 결과의 레코드 수 반환
CREATE PROCEDURE [dbo].[LoginsSearchCountAdmin]
    @SearchField NVarChar(25),
    @SearchQuery NVarChar(25),
	@StartDate DateTime,
	@EndDate DateTime
AS
    Set @SearchQuery = '%' + @SearchQuery + '%'

    Select Count(*)
    From Logins
    Where
    (
        Case @SearchField 
            When 'UserName' Then [UserName]
            When 'LoginIp' Then LoginIp 
            Else @SearchQuery
        End
    ) 
    Like 
    @SearchQuery
	And 
	LoginDate Between @StartDate And @EndDate 
Go
