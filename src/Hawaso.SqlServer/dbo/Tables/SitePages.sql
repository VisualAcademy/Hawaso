CREATE TABLE [dbo].[SitePages]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SitePages] PRIMARY KEY,
    [RoutePattern] NVARCHAR(300) NOT NULL,
    [HttpMethod] NVARCHAR(50) NULL,
    [DisplayName] NVARCHAR(300) NULL,
    [PageTitle] NVARCHAR(200) NULL,
    [PageNumber] INT NULL,
    [SortOrder] INT NOT NULL CONSTRAINT [DF_SitePages_SortOrder] DEFAULT 0,
    [IsPublic] BIT NOT NULL CONSTRAINT [DF_SitePages_IsPublic] DEFAULT 1,
    [IsVisibleInDashboard] BIT NOT NULL CONSTRAINT [DF_SitePages_IsVisibleInDashboard] DEFAULT 1,
    [RequiredRoles] NVARCHAR(500) NULL,
    [RequiredPolicy] NVARCHAR(200) NULL,
    [AllowAnonymous] BIT NOT NULL CONSTRAINT [DF_SitePages_AllowAnonymous] DEFAULT 0,
    [IsEndpointActive] BIT NOT NULL CONSTRAINT [DF_SitePages_IsEndpointActive] DEFAULT 1,
    [LastSyncedAtUtc] DATETIME2 NULL,
    [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_SitePages_CreatedAtUtc] DEFAULT SYSUTCDATETIME(),
    [UpdatedAtUtc] DATETIME2 NULL
);

--CREATE UNIQUE INDEX [UX_SitePages_RoutePattern_HttpMethod]
--ON [dbo].[SitePages]([RoutePattern], [HttpMethod]);