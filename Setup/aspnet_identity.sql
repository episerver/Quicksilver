CREATE TABLE [dbo].[__MigrationHistory] (
    [MigrationId]    NVARCHAR (150)  NOT NULL,
    [ContextKey]     NVARCHAR (300)  NOT NULL,
    [Model]          VARBINARY (MAX) NOT NULL,
    [ProductVersion] NVARCHAR (32)   NOT NULL
);
GO

CREATE TABLE [dbo].[AspNetRoles] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([Name] ASC);
GO

ALTER TABLE [dbo].[AspNetRoles]
    ADD CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC);
GO

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (128) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);
GO

ALTER TABLE [dbo].[AspNetUserClaims]
    ADD CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC);
GO

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        NVARCHAR (128) NOT NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserLogins]([UserId] ASC);
GO

ALTER TABLE [dbo].[AspNetUserLogins]
    ADD CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC);
GO

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserRoles]([UserId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[AspNetUserRoles]([RoleId] ASC);
GO

ALTER TABLE [dbo].[AspNetUserRoles]
    ADD CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC);
GO

CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL,
    [Email]                NVARCHAR (256) NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [PhoneNumber]          NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [AccessFailedCount]    INT            NOT NULL,
    [UserName]             NVARCHAR (256) NOT NULL,
	[NewsLetter]           BIT            NOT NULL
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([UserName] ASC);
GO

ALTER TABLE [dbo].[AspNetUsers]
    ADD CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC);
GO

ALTER TABLE [dbo].[AspNetUserClaims]
    ADD CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserLogins]
    ADD CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserRoles]
    ADD CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[AspNetUserRoles]
    ADD CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

declare @AdminRoleId as uniqueidentifier;
declare @AdminUserId as uniqueidentifier;
declare @WebAdminRoleId as uniqueidentifier;
declare @WebAdminUserId as uniqueidentifier;
declare @EditorRoleId as uniqueidentifier;
declare @EditorUserId as uniqueidentifier;

set @AdminRoleId = NEWID();
set @WebAdminRoleId = NEWID();
set @EditorRoleId = NEWID();
set @AdminUserId = NEWID();
set @EditorUserId = NEWID();
set @WebAdminUserId = NEWID();

insert into AspNetRoles(Id, Name) Values(@AdminRoleId, 'Administrators');
insert into AspNetRoles(Id, Name) Values(@WebAdminRoleId, 'WebAdmins');
insert into AspNetRoles(Id, Name) Values(@EditorRoleId, 'WebEditors');

insert into AspNetUsers (ID, email, EmailConfirmed, PasswordHash, SecurityStamp, UserName, LockoutEnabled, AccessFailedCount, PhoneNumberConfirmed, TwoFactorEnabled, NewsLetter)
values (@AdminUserId, 'admin', 0, 'AAwsxpbbay95Ig5UUtJfqrz5QQZDWbbJShgza2BVP9sZAEaDvoC+UZ6HP1ER3b94FQ==', '989acc4f-30bd-425d-9b20-7c7f85bee15b', 'admin', 0, 0, 0, 0, 0);

insert into AspNetUserRoles (RoleId, UserId) values (@AdminRoleId, @AdminUserId);


insert into AspNetUsers (ID, email, EmailConfirmed, PasswordHash, SecurityStamp, UserName, LockoutEnabled, AccessFailedCount, PhoneNumberConfirmed, TwoFactorEnabled, NewsLetter)
values (@EditorUserId, 'editor@quicksilver.com', 0, 'AAwsxpbbay95Ig5UUtJfqrz5QQZDWbbJShgza2BVP9sZAEaDvoC+UZ6HP1ER3b94FQ==', '989acc4f-30bd-425d-9b20-7c7f85bee15b', 'editor@quicksilver.com', 0, 0, 0, 0, 0);

insert into AspNetUserRoles (RoleId, UserId) values (@EditorRoleId, @EditorUserId);


insert into AspNetUsers (ID, email, EmailConfirmed, PasswordHash, SecurityStamp, UserName, LockoutEnabled, AccessFailedCount, PhoneNumberConfirmed, TwoFactorEnabled, NewsLetter)
values (@WebAdminUserId, 'webaadmin@quicksilver.com', 0, 'AAwsxpbbay95Ig5UUtJfqrz5QQZDWbbJShgza2BVP9sZAEaDvoC+UZ6HP1ER3b94FQ==', '989acc4f-30bd-425d-9b20-7c7f85bee15b', 'webaadmin@quicksilver.com', 0, 0, 0, 0, 0);

insert into AspNetUserRoles (RoleId, UserId) values (@WebAdminRoleId, @WebAdminUserId);

GO
