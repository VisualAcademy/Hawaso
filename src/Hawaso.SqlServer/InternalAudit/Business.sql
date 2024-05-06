CREATE TABLE [dbo].[Business](
	[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,	-- 기본 키, 자동 증가
	[Name] [nvarchar](max) NULL,								-- 사업체 이름
	[Active] [bit] NOT NULL,									-- 활성 상태
	[Address] [nvarchar](max) NULL,								-- 주소
	[City] [nvarchar](max) NULL,								-- 도시
	[State] [nvarchar](max) NULL,								-- 주(州)
	[Zip] [nvarchar](max) NULL,									-- 우편번호
	[RcpTitle] [nvarchar](max) NULL,							-- 수신자 직함
	[RcpName] [nvarchar](max) NULL,								-- 수신자 전체 이름
	[RcpEmail] [nvarchar](max) NULL,							-- 수신자 이메일
	[RcpLastName] [nvarchar](100) NULL,							-- 수신자 성
	[RcpFirstName] [nchar](100) NULL							-- 수신자 이름
)
GO
