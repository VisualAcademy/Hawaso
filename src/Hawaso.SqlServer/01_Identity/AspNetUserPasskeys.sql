CREATE TABLE [dbo].[AspNetUserPasskeys] (
    [CredentialId] VARBINARY (1024) NOT NULL,
    [UserId]       NVARCHAR (450)   NOT NULL,
    [Data]         NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_AspNetUserPasskeys] PRIMARY KEY CLUSTERED ([CredentialId] ASC),
    CONSTRAINT [FK_AspNetUserPasskeys_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserPasskeys_UserId]
    ON [dbo].[AspNetUserPasskeys]([UserId] ASC);

