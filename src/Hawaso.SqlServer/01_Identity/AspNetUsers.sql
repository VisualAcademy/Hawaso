-- ASP.NET Core Identity의 사용자 테이블인 AspNetUsers의 스키마입니다.
-- 다음은 테이블에 추가된 필드인 Address를 확인할 수 있습니다.

-- ASP.NET Core Identity의 사용자 인증과 권한을 관리하는 테이블입니다.
CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (450)     NOT NULL,                      -- 사용자의 고유 식별자
    [UserName]             NVARCHAR (256)     NULL,                          -- 사용자 이름
    [NormalizedUserName]   NVARCHAR (256)     NULL,                          -- 정규화된 사용자 이름
    [Email]                NVARCHAR (256)     NULL,                          -- 이메일 주소
    [NormalizedEmail]      NVARCHAR (256)     NULL,                          -- 정규화된 이메일 주소
    [EmailConfirmed]       BIT                NOT NULL,                      -- 이메일 확인 여부
    [PasswordHash]         NVARCHAR (MAX)     NULL,                          -- 비밀번호 해시값
    [SecurityStamp]        NVARCHAR (MAX)     NULL,                          -- 보안 스탬프
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,                          -- 동시성 스탬프
    [PhoneNumber]          NVARCHAR (MAX)     NULL,                          -- 전화번호
    [PhoneNumberConfirmed] BIT                NOT NULL,                      -- 전화번호 확인 여부
    [TwoFactorEnabled]     BIT                NOT NULL,                      -- 이중 인증 활성화 여부
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,                          -- 계정 잠금 해제 일시
    [LockoutEnabled]       BIT                NOT NULL,                      -- 계정 잠금 활성화 여부
    [AccessFailedCount]    INT                NOT NULL,                      -- 계정 접근 실패 횟수
    [Address]              NVARCHAR (MAX)     NULL,                          -- 주소 (추가된 필드)
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
	[Timezone] [nvarchar](max) NULL,

    [TenantName]               NVARCHAR (MAX)     Default('Kodee'),

    RegistrationDate DATETIMEOFFSET  NULL DEFAULT (SYSDATETIMEOFFSET()),

    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)             -- 기본키 설정
);


GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);
