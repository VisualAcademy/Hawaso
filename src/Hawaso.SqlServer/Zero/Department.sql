-- 부서 
CREATE TABLE [dbo].[Department] (
    [ID]     INT           IDENTITY (1, 1) NOT NULL,
    [Dept]   NVARCHAR (50) NULL,
    [Active] BIT           NULL,
    CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED ([ID] ASC)
);
