-- 건물 
CREATE TABLE [dbo].[Property] (
    [ID]     INT           IDENTITY (1, 1) NOT NULL,
    [Name]   NVARCHAR (50) NULL,
    [Active] BIT           NULL,
    CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED ([ID] ASC)
);
