/*
Created: 06.03.2019
Modified: 30.04.2021
Model: Iam
Database: MS SQL Server 2016
*/
-- Create the database schema if it doesn't exists
IF NOT EXISTS (SELECT [schema_id] FROM [sys].[schemas] WHERE [name] = 'Core')
BEGIN
    EXEC (N'CREATE SCHEMA [Core]');
    PRINT 'Created database schema [Core]';
END
ELSE
    PRINT 'Database schema [Core] already exists';    

-- Create the database schema if it doesn't exists
IF NOT EXISTS (SELECT [schema_id] FROM [sys].[schemas] WHERE [name] = 'Identity')
BEGIN
    EXEC (N'CREATE SCHEMA [Identity]');
    PRINT 'Created database schema [Identity]';
END
ELSE
    PRINT 'Database schema [Identity] already exists';         


-- Create fulltext catalogs section -------------------------------------------------

CREATE FULLTEXT CATALOG [IamCatalog]
 WITH ACCENT_SENSITIVITY = OFF
;

-- Create tables section -------------------------------------------------

-- Table Identity.RoleClaims

CREATE TABLE [Identity].[RoleClaims]
(
 [Id] Int IDENTITY(1,1) NOT NULL,
 [RoleId] Int NOT NULL,
 [ClaimType] Nvarchar(max) NULL,
 [ClaimValue] Nvarchar(max) NULL
)
;

-- Create indexes for table Identity.RoleClaims

CREATE INDEX [IX_RoleClaims_RoleId] ON [Identity].[RoleClaims] ([RoleId])
;

-- Add keys for table Identity.RoleClaims

ALTER TABLE [Identity].[RoleClaims] ADD CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id])
;

-- Table Identity.Roles

CREATE TABLE [Identity].[Roles]
(
 [Id] Int NOT NULL,
 [Name] Nvarchar(256) NULL,
 [NormalizedName] Nvarchar(256) NULL,
 [ConcurrencyStamp] Nvarchar(max) NULL
)
;

-- Create indexes for table Identity.Roles

CREATE UNIQUE INDEX [IX_Roles_Name] ON [Identity].[Roles] ([NormalizedName])
 WHERE ([NormalizedName] IS NOT NULL)
;

-- Add keys for table Identity.Roles

ALTER TABLE [Identity].[Roles] ADD CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
;

-- Table Identity.UserClaims

CREATE TABLE [Identity].[UserClaims]
(
 [Id] Int IDENTITY(1,1) NOT NULL,
 [UserId] Int NOT NULL,
 [ClaimType] Nvarchar(max) NULL,
 [ClaimValue] Nvarchar(max) NULL
)
;

-- Create indexes for table Identity.UserClaims

CREATE INDEX [IX_UserClaims_UserId] ON [Identity].[UserClaims] ([UserId])
;

-- Add keys for table Identity.UserClaims

ALTER TABLE [Identity].[UserClaims] ADD CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id])
;

-- Table Identity.UserLogins

CREATE TABLE [Identity].[UserLogins]
(
 [LoginProvider] Nvarchar(128) NOT NULL,
 [ProviderKey] Nvarchar(128) NOT NULL,
 [ProviderDisplayName] Nvarchar(max) NULL,
 [UserId] Int NOT NULL
)
;

-- Create indexes for table Identity.UserLogins

CREATE INDEX [IX_UserLogins_UserId] ON [Identity].[UserLogins] ([UserId])
;

-- Add keys for table Identity.UserLogins

ALTER TABLE [Identity].[UserLogins] ADD CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider],[ProviderKey])
;

-- Table Identity.Users

CREATE TABLE [Identity].[Users]
(
 [Id] Int IDENTITY NOT NULL,
 [UserName] Nvarchar(256) NULL,
 [NormalizedUserName] Nvarchar(256) NULL,
 [Email] Nvarchar(256) NULL,
 [NormalizedEmail] Nvarchar(256) NULL,
 [EmailConfirmed] Bit NOT NULL,
 [PasswordHash] Nvarchar(max) NULL,
 [SecurityStamp] Nvarchar(32) NULL,
 [ConcurrencyStamp] Nvarchar(32) NULL,
 [PhoneNumber] Nvarchar(25) NULL,
 [PhoneNumberConfirmed] Bit NOT NULL,
 [TwoFactorEnabled] Bit NOT NULL,
 [LockoutEnd] Datetimeoffset(7) NULL,
 [LockoutEnabled] Bit NOT NULL,
 [AccessFailedCount] Int NOT NULL
)
;

-- Create indexes for table Identity.Users

CREATE INDEX [IX_Users_Email] ON [Identity].[Users] ([NormalizedEmail])
;

CREATE UNIQUE INDEX [IX_Users_UserName] ON [Identity].[Users] ([NormalizedUserName])
 WHERE ([NormalizedUserName] IS NOT NULL)
;

-- Add keys for table Identity.Users

ALTER TABLE [Identity].[Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
;

-- Create fulltext index for table Identity.Users

CREATE FULLTEXT INDEX ON [Identity].[Users]
  (Email)
  KEY INDEX [PK_Users]
    ON [IamCatalog]
  WITH CHANGE_TRACKING = AUTO,
       STOPLIST = OFF
;

-- Table Identity.UserTokens

CREATE TABLE [Identity].[UserTokens]
(
 [UserId] Int NOT NULL,
 [LoginProvider] Nvarchar(128) NOT NULL,
 [Name] Nvarchar(128) NOT NULL,
 [Value] Nvarchar(max) NULL
)
;

-- Add keys for table Identity.UserTokens

ALTER TABLE [Identity].[UserTokens] ADD CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId],[LoginProvider],[Name])
;

-- Table Core.UserAccount

CREATE TABLE [Core].[UserAccount]
(
 [Id] Int IDENTITY NOT NULL,
 [Forename] Nvarchar(100) NULL,
 [Surname] Nvarchar(100) NULL,
 [Role] Int NOT NULL,
 [IsActive] Bit DEFAULT 1 NOT NULL
)
;

-- Add keys for table Core.UserAccount

ALTER TABLE [Core].[UserAccount] ADD CONSTRAINT [PK_UserAccount] PRIMARY KEY ([Id])
;

-- Create foreign keys (relationships) section ------------------------------------------------- 


ALTER TABLE [Identity].[RoleClaims] ADD CONSTRAINT [FK_Roles_RoleClaims] FOREIGN KEY ([RoleId]) REFERENCES [Identity].[Roles] ([Id]) ON UPDATE NO ACTION ON DELETE CASCADE
;


ALTER TABLE [Identity].[UserClaims] ADD CONSTRAINT [FK_Users_UserClaims] FOREIGN KEY ([UserId]) REFERENCES [Identity].[Users] ([Id]) ON UPDATE NO ACTION ON DELETE CASCADE
;


ALTER TABLE [Identity].[UserLogins] ADD CONSTRAINT [FK_Users_UserLogins] FOREIGN KEY ([UserId]) REFERENCES [Identity].[Users] ([Id]) ON UPDATE NO ACTION ON DELETE CASCADE
;


ALTER TABLE [Identity].[UserTokens] ADD CONSTRAINT [FK_Users_UserTokens] FOREIGN KEY ([UserId]) REFERENCES [Identity].[Users] ([Id]) ON UPDATE NO ACTION ON DELETE NO ACTION
;


ALTER TABLE [Core].[UserAccount] ADD CONSTRAINT [FK_Roles_UserAccount] FOREIGN KEY ([Role]) REFERENCES [Identity].[Roles] ([Id]) ON UPDATE NO ACTION ON DELETE NO ACTION
;




