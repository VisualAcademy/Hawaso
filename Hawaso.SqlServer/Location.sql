-- 위치 
CREATE TABLE [dbo].[Location] (
    [ID]       INT           IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (50) NULL,
    [Active]   BIT           NULL,
    [Property] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ([ID] ASC)
);
