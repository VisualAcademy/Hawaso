-- 전체 데이터 조회(로그인 리스트)
CREATE PROCEDURE [dbo].[LoginsList]
    @PageNumber Int = 1,
    @PageSize Int = 10,
	@UserName NVarChar(100)	-- 사용자 아이디
AS
    Select 
		*
	From Logins
	Where UserName = @UserName 
    Order By LoginId Desc 
    Offset ((@PageNumber - 1) * @PageSize) Rows Fetch Next @PageSize Rows Only;
Go
