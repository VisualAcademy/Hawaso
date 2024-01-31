CREATE TABLE [dbo].[CaseStatus] (
    [ID]         INT           IDENTITY (1, 1) NOT NULL,
    [CaseStatus] NVARCHAR (50) NULL,
    [Active]     BIT           NULL,
    CONSTRAINT [PK_CaseStatus] PRIMARY KEY CLUSTERED ([ID] ASC)
);
