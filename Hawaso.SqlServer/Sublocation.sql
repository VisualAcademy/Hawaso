CREATE TABLE [dbo].[Sublocation] (
    [ID]          INT           IDENTITY (1, 1) NOT NULL,
    [Sublocation] NVARCHAR (50) NULL,
    [Active]      BIT           NULL,
    [Location]    NVARCHAR (50) NULL,
    [Property]    NVARCHAR (50) NULL,
    CONSTRAINT [PK_Sublocation] PRIMARY KEY CLUSTERED ([ID] ASC)
);
