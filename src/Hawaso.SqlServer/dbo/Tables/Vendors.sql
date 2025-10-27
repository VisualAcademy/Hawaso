CREATE TABLE [dbo].[Vendors] (
    [ID]                         BIGINT             IDENTITY (1, 1) NOT NULL PRIMARY KEY,
    [Active]                     BIT                NOT NULL DEFAULT 1,
    [Name]                       NVARCHAR (MAX)     NOT NULL,
    [Alias]                      NVARCHAR (MAX)     NULL,
    [LicenseNumber]              NVARCHAR (35)      NULL,
    [LicenseDate]                DATETIME2 (7)      NULL,
    [LicenseRenewalDate]         DATETIME2 (7)      NULL,
    [LicenseExpirationDate]      DATETIME2 (7)      NULL,

    [MailingAddress_City]        NVARCHAR (MAX)     NULL,
    [MailingAddress_Country]     NVARCHAR (MAX)     NULL,
);
