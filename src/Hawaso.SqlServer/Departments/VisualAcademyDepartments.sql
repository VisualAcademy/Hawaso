--[0][0] 부서: Departments 
CREATE TABLE [dbo].[VisualAcademyDepartments]
(
    [Id]        BIGINT             IDENTITY (1, 1) NOT NULL Primary Key,    -- 부서 고유 아이디, 자동 증가
    [Active]    BIT                DEFAULT ((1)) NOT NULL,                  -- 활성 상태 표시, 기본값 1 (활성)
    [CreatedAt] DATETIMEOFFSET (7) NOT NULL,                                -- 레코드 생성 시간
    [CreatedBy] NVARCHAR (70)      NULL,                                    -- 레코드 생성자 이름
    [Name]      NVARCHAR (MAX)     NULL                                     -- 부서명
);
