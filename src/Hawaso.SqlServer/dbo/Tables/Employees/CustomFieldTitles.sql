CREATE TABLE [dbo].[CustomFieldTitles] (
    [ID] bigint IDENTITY(1,1) PRIMARY KEY,
    [Type] nvarchar(50) NOT NULL,
    [Field] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NULL,
    [Visible] bit NOT NULL DEFAULT 0,
    [Searchable] bit NOT NULL DEFAULT 0
);
