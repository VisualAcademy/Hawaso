--[0][0] 부서
CREATE TABLE [dbo].[Departments] 
(
    [Id]        BIGINT             IDENTITY (1, 1) NOT NULL Primary Key,
    [Active]    BIT                DEFAULT ((1)) NOT NULL,
    [CreatedAt] DATETIMEOFFSET (7) NOT NULL,
    [CreatedBy] NVARCHAR (255)     NULL,
    [Name]      NVARCHAR (MAX)     NULL,                                    -- 'IT', 'HR', 'Sales', 'Marketing', 'Finance', 'Engineering', 'Accounting'    
);
