-- 로그인 기록 
CREATE TABLE [dbo].[Logins] (
    [LoginId]   INT                IDENTITY (1, 1) NOT NULL,			-- 일련번호
    [UserId]    INT                NULL,								-- Users.Id
    [UserName]  NVARCHAR (MAX)     NULL,								-- 아이디 또는 이메일
    [LoginIp]   NVARCHAR (255)     NULL,								-- 아이피 주소
    [LoginDate] DATETIMEOFFSET (7) DEFAULT (getdate()) NULL,			-- 등록일 
    PRIMARY KEY CLUSTERED ([LoginId] ASC)
);
