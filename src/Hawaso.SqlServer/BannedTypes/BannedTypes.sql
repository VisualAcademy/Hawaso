CREATE TABLE [dbo].[BannedTypes]
(
    [Id]        BIGINT             IDENTITY (1, 1) NOT NULL Primary Key,    -- BannedType 고유 아이디, 자동 증가
    [Active]    BIT                DEFAULT ((1)) NOT NULL,                  -- 활성 상태 표시, 기본값 1 (활성)
    [CreatedAt] DATETIMEOFFSET (7) NOT NULL,                                -- 레코드 생성 시간
    [CreatedBy] NVARCHAR (255)     NULL,                                    -- 레코드 생성자 이름
    [Name]      NVARCHAR (MAX)     NULL                                     -- BannedType명
);