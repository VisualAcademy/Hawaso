CREATE TABLE [dbo].[ContactTypes] (
    [ID] BIGINT IDENTITY (1, 1) NOT NULL,
    [Active] BIT DEFAULT ((1)) NOT NULL,
    [CreatedAt] DATETIMEOFFSET (7) NOT NULL,
    [CreatedBy] NVARCHAR (70) NULL,
    [Label] NVARCHAR (255) NULL,
    [Description] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_ContactTypes] PRIMARY KEY CLUSTERED ([ID] ASC)
);
