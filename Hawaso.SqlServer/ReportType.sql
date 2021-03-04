CREATE TABLE [dbo].[ReportType] (
    [ID]              INT           IDENTITY (1, 1) NOT NULL,
    [TypeName]        NVARCHAR (50) NULL,
    [TypeDesignation] NVARCHAR (50) NULL,
    [Active]          BIT           NULL,
    CONSTRAINT [PK_ReportType] PRIMARY KEY CLUSTERED ([ID] ASC)
);
