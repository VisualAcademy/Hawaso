-- 전체 데이터 조회(로그인 리스트): 관리자는 모든 로그인 리스트 표시
CREATE PROCEDURE [dbo].[LoginsListAdmin]
    @PageNumber Int = 1,
    @PageSize Int = 10
AS
    Select 
		* 
    From Logins
    Order By LoginId Desc
    Offset ((@PageNumber - 1) * @PageSize) Rows Fetch Next @PageSize Rows Only;
Go
