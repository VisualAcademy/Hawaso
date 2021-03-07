CREATE TABLE [dbo].[ReportSpecific] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [Specific]     NVARCHAR (255) NULL,
    [Active]       BIT            NULL,
    [ReportTypeID] INT            NULL,
    CONSTRAINT [PK_ReportSpecific] PRIMARY KEY CLUSTERED ([ID] ASC)
);

