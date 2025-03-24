CREATE TABLE [dbo].[CertificateReports] (
    [ID]                       BIGINT             IDENTITY (1, 1) NOT NULL,
    [Active]                   BIT                DEFAULT ((1)) NOT NULL,
    [ApprovedBy]               NVARCHAR (MAX)     NULL,
    [Conclusion]               NVARCHAR (MAX)     NULL,
    [CreatedAt]                DATETIMEOFFSET (7) NOT NULL,
    [CreatedBy]                NVARCHAR (70)      NULL,
    [DateApproved]             DATETIMEOFFSET (7) NULL,
    [EligibilityDetermination] NVARCHAR (MAX)     NULL,
    [EmployeeID]               BIGINT             NULL,
    [File]                     NVARCHAR (MAX)     NULL,
    [InvestigationID]          BIGINT             NULL,
    [Reason]                   NVARCHAR (MAX)     NULL,
    [ReviewNotes]              NVARCHAR (MAX)     NULL,
    [Status]                   NVARCHAR (MAX)     NULL,
    [Type]                     INT                DEFAULT ((0)) NOT NULL,
    [VendorID]                 BIGINT             NULL,
    CONSTRAINT [PK_CertificateReports] PRIMARY KEY CLUSTERED ([ID] ASC)
);
