IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [user] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_user] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928160453_InitDB', N'9.0.9');

ALTER TABLE [user] ADD [Dni] bigint NOT NULL DEFAULT CAST(0 AS bigint);

ALTER TABLE [user] ADD [Foto] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [user] ADD [LastName] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [user] ADD [Mail] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [user] ADD [Password] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [user] ADD [Telefono] bigint NOT NULL DEFAULT CAST(0 AS bigint);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928162430_AgregandoCamposAUsuarios', N'9.0.9');

ALTER TABLE [user] ADD [Rol_id] int NOT NULL DEFAULT 0;

ALTER TABLE [user] ADD [rolId] int NOT NULL DEFAULT 0;

CREATE TABLE [rol] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_rol] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_user_rolId] ON [user] ([rolId]);

ALTER TABLE [user] ADD CONSTRAINT [FK_user_rol_rolId] FOREIGN KEY ([rolId]) REFERENCES [rol] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928165110_AgregandoRoles', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928165614_AnotacionRol', N'9.0.9');

ALTER TABLE [user] DROP CONSTRAINT [FK_user_rol_rolId];

DROP INDEX [IX_user_rolId] ON [user];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user]') AND [c].[name] = N'rolId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [user] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [user] DROP COLUMN [rolId];

CREATE INDEX [IX_user_Rol_id] ON [user] ([Rol_id]);

ALTER TABLE [user] ADD CONSTRAINT [FK_user_rol_Rol_id] FOREIGN KEY ([Rol_id]) REFERENCES [rol] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250928165853_RolFluentApi', N'9.0.9');

ALTER TABLE [user] DROP CONSTRAINT [FK_user_rol_Rol_id];

DROP TABLE [rol];

EXEC sp_rename N'[user].[Telefono]', N'PhoneNumber', 'COLUMN';

EXEC sp_rename N'[user].[Rol_id]', N'Role_id', 'COLUMN';

EXEC sp_rename N'[user].[Foto]', N'Photo', 'COLUMN';

EXEC sp_rename N'[user].[IX_user_Rol_id]', N'IX_user_Role_id', 'INDEX';

CREATE TABLE [consortium] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_consortium] PRIMARY KEY ([Id])
);

CREATE TABLE [responsibleSector] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_responsibleSector] PRIMARY KEY ([Id])
);

CREATE TABLE [role] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_role] PRIMARY KEY ([Id])
);

CREATE TABLE [claimResponse] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [ResponseDate] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [ResponsibleSector_id] int NOT NULL,
    CONSTRAINT [PK_claimResponse] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_claimResponse_responsibleSector_ResponsibleSector_id] FOREIGN KEY ([ResponsibleSector_id]) REFERENCES [responsibleSector] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_claimResponse_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [claim] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [Priority] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastUpdatedAt] datetime2 NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Archive] nvarchar(max) NOT NULL,
    [User_id] int NOT NULL,
    [ClaimResponse_id] int NOT NULL,
    CONSTRAINT [PK_claim] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_claim_claimResponse_ClaimResponse_id] FOREIGN KEY ([ClaimResponse_id]) REFERENCES [claimResponse] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_claim_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_claim_ClaimResponse_id] ON [claim] ([ClaimResponse_id]);

CREATE INDEX [IX_claim_User_id] ON [claim] ([User_id]);

CREATE INDEX [IX_claimResponse_ResponsibleSector_id] ON [claimResponse] ([ResponsibleSector_id]);

CREATE INDEX [IX_claimResponse_UserId] ON [claimResponse] ([UserId]);

ALTER TABLE [user] ADD CONSTRAINT [FK_user_role_Role_id] FOREIGN KEY ([Role_id]) REFERENCES [role] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250929001756_Reclamos', N'9.0.9');

CREATE TABLE [event] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [Location] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [NumberOfPersons] int NOT NULL,
    [Mandatory] bit NOT NULL,
    CONSTRAINT [PK_event] PRIMARY KEY ([Id])
);

CREATE TABLE [place] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_place] PRIMARY KEY ([Id])
);

CREATE TABLE [residence] (
    [Id] int NOT NULL IDENTITY,
    [Number] int NOT NULL,
    [Floor] int NOT NULL,
    [Tower] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_residence] PRIMARY KEY ([Id])
);

CREATE TABLE [UserEvent] (
    [EventId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_UserEvent] PRIMARY KEY ([EventId], [UserId]),
    CONSTRAINT [FK_UserEvent_event_EventId] FOREIGN KEY ([EventId]) REFERENCES [event] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserEvent_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [reserves] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DeletedAt] datetime2 NOT NULL,
    [Place_id] int NOT NULL,
    [Residence_id] int NOT NULL,
    [User_id] int NOT NULL,
    CONSTRAINT [PK_reserves] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_reserves_place_Place_id] FOREIGN KEY ([Place_id]) REFERENCES [place] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_reserves_residence_Residence_id] FOREIGN KEY ([Residence_id]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_reserves_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserResidence] (
    [ResidenceId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_UserResidence] PRIMARY KEY ([ResidenceId], [UserId]),
    CONSTRAINT [FK_UserResidence_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserResidence_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_reserves_Place_id] ON [reserves] ([Place_id]);

CREATE INDEX [IX_reserves_Residence_id] ON [reserves] ([Residence_id]);

CREATE INDEX [IX_reserves_User_id] ON [reserves] ([User_id]);

CREATE INDEX [IX_UserEvent_UserId] ON [UserEvent] ([UserId]);

CREATE INDEX [IX_UserResidence_UserId] ON [UserResidence] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250929015251_Reservas', N'9.0.9');

CREATE TABLE [forum] (
    [Id] int NOT NULL IDENTITY,
    [Category] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_forum] PRIMARY KEY ([Id])
);

CREATE TABLE [thread] (
    [Id] int NOT NULL IDENTITY,
    [Theme] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [Forum_id] int NOT NULL,
    [User_id] int NOT NULL,
    CONSTRAINT [PK_thread] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_thread_forum_Forum_id] FOREIGN KEY ([Forum_id]) REFERENCES [forum] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_thread_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [message] (
    [Id] int NOT NULL IDENTITY,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [optionalFile] nvarchar(max) NOT NULL,
    [Thread_id] int NOT NULL,
    [User_id] int NOT NULL,
    CONSTRAINT [PK_message] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_message_thread_Thread_id] FOREIGN KEY ([Thread_id]) REFERENCES [thread] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_message_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_message_Thread_id] ON [message] ([Thread_id]);

CREATE INDEX [IX_message_User_id] ON [message] ([User_id]);

CREATE INDEX [IX_thread_Forum_id] ON [thread] ([Forum_id]);

CREATE INDEX [IX_thread_User_id] ON [thread] ([User_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250929201952_HiloMensajeForo', N'9.0.9');

CREATE TABLE [categoryPoll] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_categoryPoll] PRIMARY KEY ([Id])
);

CREATE TABLE [resultPoll] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [Percentage] float NOT NULL,
    CONSTRAINT [PK_resultPoll] PRIMARY KEY ([Id])
);

CREATE TABLE [poll] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DeletedAt] datetime2 NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [User_id] int NOT NULL,
    [CategoryPoll_id] int NOT NULL,
    [ResultPoll_id] int NOT NULL,
    CONSTRAINT [PK_poll] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_poll_categoryPoll_CategoryPoll_id] FOREIGN KEY ([CategoryPoll_id]) REFERENCES [categoryPoll] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_poll_resultPoll_ResultPoll_id] FOREIGN KEY ([ResultPoll_id]) REFERENCES [resultPoll] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_poll_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [pollOption] (
    [Id] int NOT NULL IDENTITY,
    [Poll_id] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_pollOption] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_pollOption_poll_Poll_id] FOREIGN KEY ([Poll_id]) REFERENCES [poll] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [vote] (
    [Id] int NOT NULL IDENTITY,
    [Poll_id] int NOT NULL,
    [User_id] int NOT NULL,
    [PollOption_id] int NOT NULL,
    [VotedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_vote] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_vote_pollOption_PollOption_id] FOREIGN KEY ([PollOption_id]) REFERENCES [pollOption] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_vote_poll_Poll_id] FOREIGN KEY ([Poll_id]) REFERENCES [poll] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_vote_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_poll_CategoryPoll_id] ON [poll] ([CategoryPoll_id]);

CREATE UNIQUE INDEX [IX_poll_ResultPoll_id] ON [poll] ([ResultPoll_id]);

CREATE INDEX [IX_poll_User_id] ON [poll] ([User_id]);

CREATE INDEX [IX_pollOption_Poll_id] ON [pollOption] ([Poll_id]);

CREATE INDEX [IX_vote_Poll_id] ON [vote] ([Poll_id]);

CREATE INDEX [IX_vote_PollOption_id] ON [vote] ([PollOption_id]);

CREATE INDEX [IX_vote_User_id] ON [vote] ([User_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250929210555_Votación', N'9.0.9');

CREATE TABLE [userDocument] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [User_id] int NOT NULL,
    [Residence_id] int NOT NULL,
    CONSTRAINT [PK_userDocument] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_userDocument_residence_Residence_id] FOREIGN KEY ([Residence_id]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_userDocument_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_userDocument_Residence_id] ON [userDocument] ([Residence_id]);

CREATE INDEX [IX_userDocument_User_id] ON [userDocument] ([User_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250929212246_Documentos', N'9.0.9');

ALTER TABLE [userDocument] DROP CONSTRAINT [FK_userDocument_residence_Residence_id];

EXEC sp_rename N'[userDocument].[Residence_id]', N'Consortium_id', 'COLUMN';

EXEC sp_rename N'[userDocument].[IX_userDocument_Residence_id]', N'IX_userDocument_Consortium_id', 'INDEX';

ALTER TABLE [userDocument] ADD CONSTRAINT [FK_userDocument_consortium_Consortium_id] FOREIGN KEY ([Consortium_id]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250929213312_ModificoError', N'9.0.9');

DROP INDEX [IX_claim_ClaimResponse_id] ON [claim];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[claim]') AND [c].[name] = N'User_id');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [claim] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [claim] ALTER COLUMN [User_id] int NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[claim]') AND [c].[name] = N'LastUpdatedAt');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [claim] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [claim] ALTER COLUMN [LastUpdatedAt] datetime2 NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[claim]') AND [c].[name] = N'ClaimResponse_id');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [claim] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [claim] ALTER COLUMN [ClaimResponse_id] int NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[claim]') AND [c].[name] = N'Archive');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [claim] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [claim] ALTER COLUMN [Archive] nvarchar(max) NULL;

CREATE UNIQUE INDEX [IX_claim_ClaimResponse_id] ON [claim] ([ClaimResponse_id]) WHERE [ClaimResponse_id] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250930013520_CamposNoNulosClaim', N'9.0.9');

DROP INDEX [IX_poll_ResultPoll_id] ON [poll];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[poll]') AND [c].[name] = N'ResultPoll_id');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [poll] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [poll] ALTER COLUMN [ResultPoll_id] int NULL;

CREATE UNIQUE INDEX [IX_poll_ResultPoll_id] ON [poll] ([ResultPoll_id]) WHERE [ResultPoll_id] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250930014942_nullableResultPoll', N'9.0.9');

ALTER TABLE [message] ADD [State] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250930031421_message', N'9.0.9');

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[forum]') AND [c].[name] = N'Category');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [forum] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [forum] ALTER COLUMN [Category] int NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250930190454_ForumCategory', N'9.0.9');

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user]') AND [c].[name] = N'Photo');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [user] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [user] ALTER COLUMN [Photo] nvarchar(max) NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user]') AND [c].[name] = N'Dni');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [user] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [user] ALTER COLUMN [Dni] bigint NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250930215311_nuleableDNIyPhotoEnUser', N'9.0.9');

ALTER TABLE [user] ADD [RequiresPasswordChange] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251002174918_AddRequiresPasswordChangeToUser', N'9.0.9');

CREATE TABLE [refreshToken] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [RevokedAt] datetime2 NULL,
    [RevokedByIp] nvarchar(max) NULL,
    [ReplacedByToken] nvarchar(max) NULL,
    [CreatedByIp] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_refreshToken] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_refreshToken_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_refreshToken_UserId] ON [refreshToken] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251002220454_AddRefreshTokenTable', N'9.0.9');

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[refreshToken]') AND [c].[name] = N'Token');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [refreshToken] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [refreshToken] ALTER COLUMN [Token] nvarchar(450) NOT NULL;

CREATE UNIQUE INDEX [IX_refreshToken_Token] ON [refreshToken] ([Token]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251002221126_changeInRefreshToken', N'9.0.9');

CREATE TABLE [reaction] (
    [Id] int NOT NULL IDENTITY,
    [User_id] int NOT NULL,
    [Message_id] int NULL,
    [Thread_id] int NULL,
    [ReactionType] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_reaction] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_reaction_message_Message_id] FOREIGN KEY ([Message_id]) REFERENCES [message] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_reaction_thread_Thread_id] FOREIGN KEY ([Thread_id]) REFERENCES [thread] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_reaction_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_reaction_Message_id] ON [reaction] ([Message_id]);

CREATE INDEX [IX_reaction_Thread_id] ON [reaction] ([Thread_id]);

CREATE UNIQUE INDEX [IX_reaction_User_id_Message_id_Thread_id] ON [reaction] ([User_id], [Message_id], [Thread_id]) WHERE [Message_id] IS NOT NULL AND [Thread_id] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251002234743_Reacciones', N'9.0.9');

CREATE TABLE [expense] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [TotalAmount] float(18) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpirationDate] datetime2 NOT NULL,
    [Id_Consortium] int NOT NULL,
    [Id_Residence] int NOT NULL,
    CONSTRAINT [PK_expense] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_expense_consortium_Id_Consortium] FOREIGN KEY ([Id_Consortium]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_expense_residence_Id_Residence] FOREIGN KEY ([Id_Residence]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [expenseDetail] (
    [Id] int NOT NULL IDENTITY,
    [Amount] float(18) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Id_Expense] int NOT NULL,
    CONSTRAINT [PK_expenseDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_expenseDetail_expense_Id_Expense] FOREIGN KEY ([Id_Expense]) REFERENCES [expense] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_expense_Id_Consortium] ON [expense] ([Id_Consortium]);

CREATE INDEX [IX_expense_Id_Residence] ON [expense] ([Id_Residence]);

CREATE INDEX [IX_expenseDetail_Id_Expense] ON [expenseDetail] ([Id_Expense]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251004154800_expensas', N'9.0.9');

CREATE TABLE [paymentMethod] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_paymentMethod] PRIMARY KEY ([Id])
);

CREATE TABLE [payment] (
    [Id] int NOT NULL IDENTITY,
    [Voucher] nvarchar(max) NOT NULL,
    [Date] datetime2 NOT NULL,
    [Id_Expense] int NOT NULL,
    [Id_Residence] int NOT NULL,
    [Id_PaymentMethod] int NOT NULL,
    CONSTRAINT [PK_payment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_payment_expense_Id_Expense] FOREIGN KEY ([Id_Expense]) REFERENCES [expense] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_payment_paymentMethod_Id_PaymentMethod] FOREIGN KEY ([Id_PaymentMethod]) REFERENCES [paymentMethod] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_payment_residence_Id_Residence] FOREIGN KEY ([Id_Residence]) REFERENCES [residence] ([Id])
);

CREATE INDEX [IX_payment_Id_Expense] ON [payment] ([Id_Expense]);

CREATE INDEX [IX_payment_Id_PaymentMethod] ON [payment] ([Id_PaymentMethod]);

CREATE INDEX [IX_payment_Id_Residence] ON [payment] ([Id_Residence]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251004160423_pagos', N'9.0.9');

EXEC sp_rename N'[userDocument].[Type]', N'Category', 'COLUMN';

ALTER TABLE [userDocument] ADD [Description] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251009004012_agregoEntidadDocumento', N'9.0.9');

CREATE TABLE [supplierCategory] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_supplierCategory] PRIMARY KEY ([Id])
);

CREATE TABLE [supplier] (
    [Id] int NOT NULL IDENTITY,
    [CommercialName] nvarchar(200) NOT NULL,
    [BusinessName] nvarchar(200) NULL,
    [Cuit] nvarchar(max) NOT NULL,
    [SupplierCategoryId] int NOT NULL,
    [Email] nvarchar(max) NULL,
    [Phone] nvarchar(50) NULL,
    [Address] nvarchar(500) NULL,
    [ContactPerson] nvarchar(200) NULL,
    [Observations] nvarchar(1000) NULL,
    [Rating] decimal(3,2) NULL,
    [Active] bit NOT NULL,
    [RegistrationDate] datetime2 NOT NULL,
    [LastInteraction] datetime2 NULL,
    [ConsortiumId] int NOT NULL,
    CONSTRAINT [PK_supplier] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_supplier_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_supplier_supplierCategory_SupplierCategoryId] FOREIGN KEY ([SupplierCategoryId]) REFERENCES [supplierCategory] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [supplierContract] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [ContractType] nvarchar(100) NULL,
    [Description] nvarchar(1000) NULL,
    [Price] decimal(18,2) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Active] bit NOT NULL,
    [FilePath] nvarchar(max) NULL,
    [SupplierId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_supplierContract] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_supplierContract_supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [supplier] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_supplier_ConsortiumId] ON [supplier] ([ConsortiumId]);

CREATE INDEX [IX_supplier_SupplierCategoryId] ON [supplier] ([SupplierCategoryId]);

CREATE INDEX [IX_supplierContract_SupplierId] ON [supplierContract] ([SupplierId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251009022026_suppliers', N'9.0.9');

ALTER TABLE [claimResponse] DROP CONSTRAINT [FK_claimResponse_user_UserId];

ALTER TABLE [expense] DROP CONSTRAINT [FK_expense_consortium_Id_Consortium];

ALTER TABLE [expense] DROP CONSTRAINT [FK_expense_residence_Id_Residence];

ALTER TABLE [expenseDetail] DROP CONSTRAINT [FK_expenseDetail_expense_Id_Expense];

ALTER TABLE [payment] DROP CONSTRAINT [FK_payment_expense_Id_Expense];

ALTER TABLE [payment] DROP CONSTRAINT [FK_payment_paymentMethod_Id_PaymentMethod];

ALTER TABLE [payment] DROP CONSTRAINT [FK_payment_residence_Id_Residence];

ALTER TABLE [reaction] DROP CONSTRAINT [FK_reaction_message_Message_id];

ALTER TABLE [reaction] DROP CONSTRAINT [FK_reaction_thread_Thread_id];

ALTER TABLE [reaction] DROP CONSTRAINT [FK_reaction_user_User_id];

ALTER TABLE [UserEvent] DROP CONSTRAINT [FK_UserEvent_event_EventId];

ALTER TABLE [UserEvent] DROP CONSTRAINT [FK_UserEvent_user_UserId];

ALTER TABLE [UserResidence] DROP CONSTRAINT [FK_UserResidence_residence_ResidenceId];

ALTER TABLE [UserResidence] DROP CONSTRAINT [FK_UserResidence_user_UserId];

ALTER TABLE [paymentMethod] DROP CONSTRAINT [PK_paymentMethod];

ALTER TABLE [payment] DROP CONSTRAINT [PK_payment];

ALTER TABLE [expense] DROP CONSTRAINT [PK_expense];

EXEC sp_rename N'[paymentMethod]', N'PaymentMethod', 'OBJECT';

EXEC sp_rename N'[payment]', N'Payment', 'OBJECT';

EXEC sp_rename N'[expense]', N'Expense', 'OBJECT';

EXEC sp_rename N'[Payment].[Id_Residence]', N'ResidenceId', 'COLUMN';

EXEC sp_rename N'[Payment].[Id_PaymentMethod]', N'PaymentMethodId', 'COLUMN';

EXEC sp_rename N'[Payment].[Id_Expense]', N'ExpenseId', 'COLUMN';

EXEC sp_rename N'[Payment].[IX_payment_Id_Residence]', N'IX_Payment_ResidenceId', 'INDEX';

EXEC sp_rename N'[Payment].[IX_payment_Id_PaymentMethod]', N'IX_Payment_PaymentMethodId', 'INDEX';

EXEC sp_rename N'[Payment].[IX_payment_Id_Expense]', N'IX_Payment_ExpenseId', 'INDEX';

EXEC sp_rename N'[expenseDetail].[Id_Expense]', N'ExpenseId', 'COLUMN';

EXEC sp_rename N'[expenseDetail].[IX_expenseDetail_Id_Expense]', N'IX_expenseDetail_ExpenseId', 'INDEX';

EXEC sp_rename N'[Expense].[Id_Residence]', N'ResidenceId', 'COLUMN';

EXEC sp_rename N'[Expense].[Id_Consortium]', N'ConsortiumId', 'COLUMN';

EXEC sp_rename N'[Expense].[IX_expense_Id_Residence]', N'IX_Expense_ResidenceId', 'INDEX';

EXEC sp_rename N'[Expense].[IX_expense_Id_Consortium]', N'IX_Expense_ConsortiumId', 'INDEX';

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[expenseDetail]') AND [c].[name] = N'Amount');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [expenseDetail] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [expenseDetail] ALTER COLUMN [Amount] float NOT NULL;

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expense]') AND [c].[name] = N'TotalAmount');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Expense] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Expense] ALTER COLUMN [TotalAmount] float NOT NULL;

ALTER TABLE [PaymentMethod] ADD CONSTRAINT [PK_PaymentMethod] PRIMARY KEY ([Id]);

ALTER TABLE [Payment] ADD CONSTRAINT [PK_Payment] PRIMARY KEY ([Id]);

ALTER TABLE [Expense] ADD CONSTRAINT [PK_Expense] PRIMARY KEY ([Id]);

CREATE TABLE [blockchainProof] (
    [Id] int NOT NULL IDENTITY,
    [PollId] int NULL,
    [DocumentId] uniqueidentifier NULL,
    [HashHex] nvarchar(max) NOT NULL,
    [Uri] nvarchar(max) NOT NULL,
    [TxHash] nvarchar(max) NOT NULL,
    [Contract] nvarchar(max) NOT NULL,
    [Network] nvarchar(max) NOT NULL,
    [ChainId] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_blockchainProof] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_blockchainProof_poll_PollId] FOREIGN KEY ([PollId]) REFERENCES [poll] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_blockchainProof_PollId] ON [blockchainProof] ([PollId]) WHERE [PollId] IS NOT NULL;

ALTER TABLE [claimResponse] ADD CONSTRAINT [FK_claimResponse_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Expense] ADD CONSTRAINT [FK_Expense_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Expense] ADD CONSTRAINT [FK_Expense_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]);

ALTER TABLE [expenseDetail] ADD CONSTRAINT [FK_expenseDetail_Expense_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expense] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Payment] ADD CONSTRAINT [FK_Payment_Expense_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expense] ([Id]);

ALTER TABLE [Payment] ADD CONSTRAINT [FK_Payment_PaymentMethod_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethod] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Payment] ADD CONSTRAINT [FK_Payment_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]);

ALTER TABLE [reaction] ADD CONSTRAINT [FK_reaction_message_Message_id] FOREIGN KEY ([Message_id]) REFERENCES [message] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [reaction] ADD CONSTRAINT [FK_reaction_thread_Thread_id] FOREIGN KEY ([Thread_id]) REFERENCES [thread] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [reaction] ADD CONSTRAINT [FK_reaction_user_User_id] FOREIGN KEY ([User_id]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [UserEvent] ADD CONSTRAINT [FK_UserEvent_event_EventId] FOREIGN KEY ([EventId]) REFERENCES [event] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [UserEvent] ADD CONSTRAINT [FK_UserEvent_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [UserResidence] ADD CONSTRAINT [FK_UserResidence_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [UserResidence] ADD CONSTRAINT [FK_UserResidence_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251009230512_Blockchain', N'9.0.9');

EXEC sp_rename N'[supplierContract].[Price]', N'MonthlyAmount', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251010105101_changeInSupplier', N'9.0.9');

ALTER TABLE [Expense] DROP CONSTRAINT [FK_Expense_consortium_ConsortiumId];

ALTER TABLE [Expense] DROP CONSTRAINT [FK_Expense_residence_ResidenceId];

ALTER TABLE [expenseDetail] DROP CONSTRAINT [FK_expenseDetail_Expense_ExpenseId];

ALTER TABLE [Payment] DROP CONSTRAINT [FK_Payment_Expense_ExpenseId];

ALTER TABLE [Payment] DROP CONSTRAINT [FK_Payment_PaymentMethod_PaymentMethodId];

ALTER TABLE [Payment] DROP CONSTRAINT [FK_Payment_residence_ResidenceId];

ALTER TABLE [refreshToken] DROP CONSTRAINT [FK_refreshToken_user_UserId];

ALTER TABLE [supplier] DROP CONSTRAINT [FK_supplier_consortium_ConsortiumId];

ALTER TABLE [supplier] DROP CONSTRAINT [FK_supplier_supplierCategory_SupplierCategoryId];

ALTER TABLE [supplierContract] DROP CONSTRAINT [FK_supplierContract_supplier_SupplierId];

ALTER TABLE [PaymentMethod] DROP CONSTRAINT [PK_PaymentMethod];

ALTER TABLE [Payment] DROP CONSTRAINT [PK_Payment];

ALTER TABLE [Expense] DROP CONSTRAINT [PK_Expense];

EXEC sp_rename N'[PaymentMethod]', N'paymentMethod', 'OBJECT';

EXEC sp_rename N'[Payment]', N'payment', 'OBJECT';

EXEC sp_rename N'[Expense]', N'Expenses', 'OBJECT';

EXEC sp_rename N'[payment].[IX_Payment_ResidenceId]', N'IX_payment_ResidenceId', 'INDEX';

EXEC sp_rename N'[payment].[IX_Payment_PaymentMethodId]', N'IX_payment_PaymentMethodId', 'INDEX';

EXEC sp_rename N'[payment].[IX_Payment_ExpenseId]', N'IX_payment_ExpenseId', 'INDEX';

EXEC sp_rename N'[Expenses].[IX_Expense_ResidenceId]', N'IX_Expenses_ResidenceId', 'INDEX';

EXEC sp_rename N'[Expenses].[IX_Expense_ConsortiumId]', N'IX_Expenses_ConsortiumId', 'INDEX';

ALTER TABLE [paymentMethod] ADD CONSTRAINT [PK_paymentMethod] PRIMARY KEY ([Id]);

ALTER TABLE [payment] ADD CONSTRAINT [PK_payment] PRIMARY KEY ([Id]);

ALTER TABLE [Expenses] ADD CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]);

ALTER TABLE [expenseDetail] ADD CONSTRAINT [FK_expenseDetail_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]);

ALTER TABLE [payment] ADD CONSTRAINT [FK_payment_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]);

ALTER TABLE [payment] ADD CONSTRAINT [FK_payment_paymentMethod_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [paymentMethod] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [payment] ADD CONSTRAINT [FK_payment_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]);

ALTER TABLE [refreshToken] ADD CONSTRAINT [FK_refreshToken_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [supplier] ADD CONSTRAINT [FK_supplier_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [supplier] ADD CONSTRAINT [FK_supplier_supplierCategory_SupplierCategoryId] FOREIGN KEY ([SupplierCategoryId]) REFERENCES [supplierCategory] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [supplierContract] ADD CONSTRAINT [FK_supplierContract_supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [supplier] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011112659_necesaryMigration', N'9.0.9');

ALTER TABLE [supplier] DROP CONSTRAINT [FK_supplier_consortium_ConsortiumId];

ALTER TABLE [supplier] DROP CONSTRAINT [FK_supplier_supplierCategory_SupplierCategoryId];

DROP TABLE [supplierCategory];

DROP INDEX [IX_supplier_SupplierCategoryId] ON [supplier];

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'SupplierCategoryId');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [supplier] DROP COLUMN [SupplierCategoryId];

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'Phone');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [supplier] ALTER COLUMN [Phone] nvarchar(max) NULL;

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'Observations');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [supplier] ALTER COLUMN [Observations] nvarchar(max) NULL;

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'ContactPerson');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [supplier] ALTER COLUMN [ContactPerson] nvarchar(max) NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'ConsortiumId');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [supplier] ALTER COLUMN [ConsortiumId] int NULL;

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'CommercialName');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [supplier] ALTER COLUMN [CommercialName] nvarchar(max) NOT NULL;

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'BusinessName');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [supplier] ALTER COLUMN [BusinessName] nvarchar(max) NULL;

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'Address');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [supplier] ALTER COLUMN [Address] nvarchar(max) NULL;

ALTER TABLE [supplier] ADD [SupplierCategory] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [supplier] ADD CONSTRAINT [FK_supplier_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011142037_SupllierModification', N'9.0.9');

ALTER TABLE [supplier] DROP CONSTRAINT [FK_supplier_consortium_ConsortiumId];

DROP INDEX [IX_supplier_ConsortiumId] ON [supplier];

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[supplier]') AND [c].[name] = N'ConsortiumId');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [supplier] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [supplier] DROP COLUMN [ConsortiumId];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011162815_NuevaBD', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251012000934_Record', N'9.0.9');

ALTER TABLE [residence] ADD [ConsortiumId] int NOT NULL DEFAULT 0;

ALTER TABLE [poll] ADD [EndDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [poll] ADD [StartDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

CREATE INDEX [IX_residence_ConsortiumId] ON [residence] ([ConsortiumId]);

ALTER TABLE [residence] ADD CONSTRAINT [FK_residence_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251012140940_Expenses', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251012153514_updates', N'9.0.9');

ALTER TABLE [reserves] ADD [Date] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [claim] ADD [ResidenceId] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_claim_ResidenceId] ON [claim] ([ResidenceId]);

ALTER TABLE [claim] ADD CONSTRAINT [FK_claim_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251012160052_ReservasDate', N'9.0.9');

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[reserves]') AND [c].[name] = N'DeletedAt');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [reserves] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [reserves] ALTER COLUMN [DeletedAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251012215408_reserva', N'9.0.9');

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[reserves]') AND [c].[name] = N'DeletedAt');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [reserves] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [reserves] ALTER COLUMN [DeletedAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251014000709_reservaCreate', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251015003336_updatesMartu', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251017232402_update', N'9.0.9');

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[message]') AND [c].[name] = N'optionalFile');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [message] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [message] ALTER COLUMN [optionalFile] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251019034014_MessageOptionalFile', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020130131_updates4', N'9.0.9');

ALTER TABLE [thread] DROP CONSTRAINT [FK_thread_forum_Forum_id];

ALTER TABLE [thread] DROP CONSTRAINT [FK_thread_user_User_id];

EXEC sp_rename N'[thread].[User_id]', N'UserId', 'COLUMN';

EXEC sp_rename N'[thread].[Forum_id]', N'ForumId', 'COLUMN';

EXEC sp_rename N'[thread].[IX_thread_User_id]', N'IX_thread_UserId', 'INDEX';

EXEC sp_rename N'[thread].[IX_thread_Forum_id]', N'IX_thread_ForumId', 'INDEX';

ALTER TABLE [forum] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [thread] ADD CONSTRAINT [FK_thread_forum_ForumId] FOREIGN KEY ([ForumId]) REFERENCES [forum] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [thread] ADD CONSTRAINT [FK_thread_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020203854_ForumState', N'9.0.9');

ALTER TABLE [payment] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [payment] ADD [MercadoPagoPaymentId] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [payment] ADD [PreferenceId] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [payment] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [payment] ADD [StatusDetail] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020212806_mp', N'9.0.9');

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment]') AND [c].[name] = N'Voucher');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [payment] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [payment] ALTER COLUMN [Voucher] nvarchar(max) NULL;

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment]') AND [c].[name] = N'StatusDetail');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [payment] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [payment] ALTER COLUMN [StatusDetail] nvarchar(max) NULL;

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment]') AND [c].[name] = N'PaymentMethodId');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [payment] DROP CONSTRAINT [' + @var26 + '];');
ALTER TABLE [payment] ALTER COLUMN [PaymentMethodId] int NULL;

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment]') AND [c].[name] = N'MercadoPagoPaymentId');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [payment] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [payment] ALTER COLUMN [MercadoPagoPaymentId] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251021221515_nulosEnPayment', N'9.0.9');

CREATE TABLE [invoice] (
    [Id] int NOT NULL IDENTITY,
    [Concept] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [InvoiceNumber] bigint NOT NULL,
    [SupplierName] nvarchar(max) NOT NULL,
    [DateOfIssue] datetime2 NOT NULL,
    [ExpirationDate] datetime2 NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Cuit] int NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [TotalTaxes] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_invoice] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251025163259_addInvoice', N'9.0.9');

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'SupplierName');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [invoice] ALTER COLUMN [SupplierName] nvarchar(200) NOT NULL;

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'InvoiceNumber');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [invoice] ALTER COLUMN [InvoiceNumber] nvarchar(50) NOT NULL;

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'FilePath');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var30 + '];');
ALTER TABLE [invoice] ALTER COLUMN [FilePath] nvarchar(500) NOT NULL;

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'Description');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [invoice] ALTER COLUMN [Description] nvarchar(1000) NULL;

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'Cuit');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var32 + '];');
ALTER TABLE [invoice] ALTER COLUMN [Cuit] nvarchar(11) NOT NULL;

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'Concept');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [invoice] ALTER COLUMN [Concept] nvarchar(200) NOT NULL;

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'Category');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var34 + '];');
ALTER TABLE [invoice] ALTER COLUMN [Category] nvarchar(100) NOT NULL;

ALTER TABLE [invoice] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251026132507_chagesInInvoice', N'9.0.9');

ALTER TABLE [invoice] ADD [ConfidenceScore] real NULL;

ALTER TABLE [invoice] ADD [ProcessedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [invoice] ADD [PurchaseOrder] nvarchar(100) NULL;

ALTER TABLE [invoice] ADD [SupplierAddress] nvarchar(500) NULL;

CREATE TABLE [invoiceItem] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Amount] decimal(18,2) NULL,
    [Quantity] int NULL,
    [UnitPrice] decimal(18,2) NULL,
    CONSTRAINT [PK_invoiceItem] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_invoiceItem_invoice_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [invoice] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_invoiceItem_InvoiceId] ON [invoiceItem] ([InvoiceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251026143428_chagesInInvoice2', N'9.0.9');

ALTER TABLE [payment] ADD [InstallmentAmount] decimal(18,2) NULL;

ALTER TABLE [payment] ADD [Installments] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251026202422_cuotasMercadoPago', N'9.0.9');

ALTER TABLE [supplier] ADD [ConsortiumId] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_supplier_ConsortiumId] ON [supplier] ([ConsortiumId]);

ALTER TABLE [supplier] ADD CONSTRAINT [FK_supplier_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251026213253_AddConsortiumToSupplier', N'9.0.9');

ALTER TABLE [thread] ADD [UpdatedAt] datetime2 NULL;

ALTER TABLE [message] ADD [DeletedAt] datetime2 NULL;

ALTER TABLE [message] ADD [DeletedByUserId] int NULL;

ALTER TABLE [message] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [message] ADD [UpdatedAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251028134853_MessageAndThreadDates', N'9.0.9');

ALTER TABLE [user] ADD [HasPermission] bit NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251030132952_updateDatabaseR', N'9.0.9');

ALTER TABLE [Expenses] DROP CONSTRAINT [FK_Expenses_residence_ResidenceId];

ALTER TABLE [payment] DROP CONSTRAINT [FK_payment_Expenses_ExpenseId];

DROP TABLE [expenseDetail];

DROP INDEX [IX_Expenses_ResidenceId] ON [Expenses];

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expenses]') AND [c].[name] = N'Category');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Expenses] DROP CONSTRAINT [' + @var35 + '];');
ALTER TABLE [Expenses] DROP COLUMN [Category];

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expenses]') AND [c].[name] = N'ResidenceId');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Expenses] DROP CONSTRAINT [' + @var36 + '];');
ALTER TABLE [Expenses] DROP COLUMN [ResidenceId];

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expenses]') AND [c].[name] = N'State');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Expenses] DROP CONSTRAINT [' + @var37 + '];');
ALTER TABLE [Expenses] DROP COLUMN [State];

EXEC sp_rename N'[payment].[ExpenseId]', N'ExpenseDetailByResidenceId', 'COLUMN';

EXEC sp_rename N'[payment].[IX_payment_ExpenseId]', N'IX_payment_ExpenseDetailByResidenceId', 'INDEX';

ALTER TABLE [residence] ADD [Coeficient] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [invoice] ADD [ExpenseId] int NOT NULL DEFAULT 0;

CREATE TABLE [ExpenseDetailByResidences] (
    [Id] int NOT NULL IDENTITY,
    [TotalAmount] float NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [ExpenseId] int NOT NULL,
    [ResidenceId] int NOT NULL,
    CONSTRAINT [PK_ExpenseDetailByResidences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExpenseDetailByResidences_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]),
    CONSTRAINT [FK_ExpenseDetailByResidences_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id])
);

CREATE INDEX [IX_invoice_ExpenseId] ON [invoice] ([ExpenseId]);

CREATE INDEX [IX_ExpenseDetailByResidences_ExpenseId] ON [ExpenseDetailByResidences] ([ExpenseId]);

CREATE INDEX [IX_ExpenseDetailByResidences_ResidenceId] ON [ExpenseDetailByResidences] ([ResidenceId]);

ALTER TABLE [invoice] ADD CONSTRAINT [FK_invoice_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [payment] ADD CONSTRAINT [FK_payment_ExpenseDetailByResidences_ExpenseDetailByResidenceId] FOREIGN KEY ([ExpenseDetailByResidenceId]) REFERENCES [ExpenseDetailByResidences] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251030214711_ExpenseDetailByResidenceYModificaciones', N'9.0.9');

ALTER TABLE [userDocument] ADD [IsValid] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [poll] ADD [ApprovedAt] datetime2 NULL;

ALTER TABLE [poll] ADD [ApprovedByUserId] int NULL;

CREATE INDEX [IX_poll_ApprovedByUserId] ON [poll] ([ApprovedByUserId]);

ALTER TABLE [poll] ADD CONSTRAINT [FK_poll_user_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [user] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031134810_UserDocumentAndPoll', N'9.0.9');

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'ExpenseId');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var38 + '];');
ALTER TABLE [invoice] ALTER COLUMN [ExpenseId] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031225631_modifyInvoice', N'9.0.9');

ALTER TABLE [invoice] ADD [ConsortiumId] int NULL;

CREATE INDEX [IX_invoice_ConsortiumId] ON [invoice] ([ConsortiumId]);

ALTER TABLE [invoice] ADD CONSTRAINT [FK_invoice_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031232221_addConsortiumInInvoice', N'9.0.9');

CREATE TABLE [passwordResetToken] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [UsedAt] datetime2 NULL,
    [CreatedByIp] nvarchar(max) NOT NULL,
    [UsedByIp] nvarchar(max) NULL,
    CONSTRAINT [PK_passwordResetToken] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_passwordResetToken_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_passwordResetToken_UserId] ON [passwordResetToken] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251101223116_addTokenReset', N'9.0.9');

ALTER TABLE [blockchainProof] ADD [CallTranscriptId] int NULL;

CREATE TABLE [Calls] (
    [Id] int NOT NULL IDENTITY,
    [CreatedByUserId] int NOT NULL,
    [StartedAt] datetime2 NOT NULL,
    [EndedAt] datetime2 NULL,
    [Status] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_Calls] PRIMARY KEY ([Id])
);

CREATE TABLE [CallParticipants] (
    [Id] int NOT NULL IDENTITY,
    [CallId] int NOT NULL,
    [UserId] int NOT NULL,
    [JoinedAt] datetime2 NOT NULL,
    [LeftAt] datetime2 NULL,
    CONSTRAINT [PK_CallParticipants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CallParticipants_Calls_CallId] FOREIGN KEY ([CallId]) REFERENCES [Calls] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CallTranscripts] (
    [Id] int NOT NULL IDENTITY,
    [CallId] int NOT NULL,
    [TranscriptPath] nvarchar(300) NOT NULL,
    [AudioPath] nvarchar(max) NULL,
    [TranscriptHash] nvarchar(200) NOT NULL,
    [AudioHash] nvarchar(200) NULL,
    [BlockchainTxHash] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CallTranscripts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CallTranscripts_Calls_CallId] FOREIGN KEY ([CallId]) REFERENCES [Calls] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_blockchainProof_CallTranscriptId] ON [blockchainProof] ([CallTranscriptId]);

CREATE INDEX [IX_CallParticipants_CallId] ON [CallParticipants] ([CallId]);

CREATE UNIQUE INDEX [IX_CallTranscripts_CallId] ON [CallTranscripts] ([CallId]);

ALTER TABLE [blockchainProof] ADD CONSTRAINT [FK_blockchainProof_CallTranscripts_CallTranscriptId] FOREIGN KEY ([CallTranscriptId]) REFERENCES [CallTranscripts] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112233429_CallAndCallTranscript', N'9.0.9');

ALTER TABLE [reserves] ADD [ConsortiumId] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_reserves_ConsortiumId] ON [reserves] ([ConsortiumId]);

ALTER TABLE [reserves] ADD CONSTRAINT [FK_reserves_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113150316_consortiumInReserve', N'9.0.9');

ALTER TABLE [CallParticipants] ADD [IsCameraOn] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CallParticipants] ADD [IsConnected] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [CallParticipants] ADD [IsMuted] bit NOT NULL DEFAULT CAST(0 AS bit);

CREATE TABLE [CallMessages] (
    [Id] int NOT NULL IDENTITY,
    [CallId] int NOT NULL,
    [UserId] int NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [SentAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CallMessages] PRIMARY KEY ([Id])
);

CREATE TABLE [CallRecordings] (
    [Id] int NOT NULL IDENTITY,
    [CallId] int NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [ContentType] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CallRecordings] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251114171314_CallMessagesAndCallRecording', N'9.0.9');

CREATE TABLE [notification] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Body] nvarchar(1000) NOT NULL,
    [Channel] nvarchar(20) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [SentAt] datetime2 NULL,
    [ReadAt] datetime2 NULL,
    [RelatedEntityId] int NULL,
    [RelatedEntityType] nvarchar(50) NULL,
    [MetadataJson] nvarchar(max) NULL,
    [ErrorMessage] nvarchar(500) NULL,
    CONSTRAINT [PK_notification] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_notification_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [notificationPreference] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [PushEnabled] bit NOT NULL,
    [EmailEnabled] bit NOT NULL,
    [SmsEnabled] bit NOT NULL,
    [ExpenseNotificationsEnabled] bit NOT NULL,
    [MeetingNotificationsEnabled] bit NOT NULL,
    [VotingNotificationsEnabled] bit NOT NULL,
    [ForumNotificationsEnabled] bit NOT NULL,
    [MaintenanceNotificationsEnabled] bit NOT NULL,
    [ClaimNotificationsEnabled] bit NOT NULL,
    [FcmToken] nvarchar(500) NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsConfigured] bit NOT NULL,
    CONSTRAINT [PK_notificationPreference] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_notificationPreference_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_notification_CreatedAt] ON [notification] ([CreatedAt]);

CREATE INDEX [IX_notification_Status] ON [notification] ([Status]);

CREATE INDEX [IX_notification_UserId] ON [notification] ([UserId]);

CREATE INDEX [IX_notification_UserId_Status] ON [notification] ([UserId], [Status]);

CREATE UNIQUE INDEX [IX_notificationPreference_UserId] ON [notificationPreference] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251115145048_Notifications', N'9.0.9');

ALTER TABLE [claim] ADD [ConsortiumId] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_claim_ConsortiumId] ON [claim] ([ConsortiumId]);

ALTER TABLE [claim] ADD CONSTRAINT [FK_claim_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251115171832_claimYConsortium', N'9.0.9');

CREATE TABLE [ExpenseResidence] (
    [ExpenseId] int NOT NULL,
    [ResidenceId] int NOT NULL,
    CONSTRAINT [PK_ExpenseResidence] PRIMARY KEY ([ExpenseId], [ResidenceId]),
    CONSTRAINT [FK_ExpenseResidence_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExpenseResidence_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ExpenseResidence_ResidenceId] ON [ExpenseResidence] ([ResidenceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251115201447_expenseAndResidence', N'9.0.9');

ALTER TABLE [consortium] ADD [AdministratorId] int NULL;

CREATE INDEX [IX_consortium_AdministratorId] ON [consortium] ([AdministratorId]);

ALTER TABLE [consortium] ADD CONSTRAINT [FK_consortium_user_AdministratorId] FOREIGN KEY ([AdministratorId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116202250_changesUser', N'9.0.9');

ALTER TABLE [invoice] ADD [ResidenceId] int NULL;

CREATE INDEX [IX_invoice_ResidenceId] ON [invoice] ([ResidenceId]);

ALTER TABLE [invoice] ADD CONSTRAINT [FK_invoice_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116203047_invoiceInResidence', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116203125_updatesNecessries', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116203727_changesInUser2', N'9.0.9');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251116211119_changesInUser3', N'9.0.9');

ALTER TABLE [ExpenseDetailByResidences] DROP CONSTRAINT [FK_ExpenseDetailByResidences_Expenses_ExpenseId];

ALTER TABLE [invoice] DROP CONSTRAINT [FK_invoice_Expenses_ExpenseId];

DROP INDEX [IX_invoice_ExpenseId] ON [invoice];

DROP INDEX [IX_ExpenseDetailByResidences_ExpenseId] ON [ExpenseDetailByResidences];

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoice]') AND [c].[name] = N'ExpenseId');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [invoice] DROP CONSTRAINT [' + @var39 + '];');
ALTER TABLE [invoice] DROP COLUMN [ExpenseId];

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ExpenseDetailByResidences]') AND [c].[name] = N'ExpenseId');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [ExpenseDetailByResidences] DROP CONSTRAINT [' + @var40 + '];');
ALTER TABLE [ExpenseDetailByResidences] DROP COLUMN [ExpenseId];

CREATE TABLE [ExpenseAndExpenseDetail] (
    [ExpenseDetailByResidenceId] int NOT NULL,
    [ExpenseId] int NOT NULL,
    CONSTRAINT [PK_ExpenseAndExpenseDetail] PRIMARY KEY ([ExpenseDetailByResidenceId], [ExpenseId]),
    CONSTRAINT [FK_ExpenseAndExpenseDetail_ExpenseDetailByResidences_ExpenseDetailByResidenceId] FOREIGN KEY ([ExpenseDetailByResidenceId]) REFERENCES [ExpenseDetailByResidences] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExpenseAndExpenseDetail_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExpenseInvoice] (
    [ExpenseId] int NOT NULL,
    [InvoicesId] int NOT NULL,
    CONSTRAINT [PK_ExpenseInvoice] PRIMARY KEY ([ExpenseId], [InvoicesId]),
    CONSTRAINT [FK_ExpenseInvoice_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExpenseInvoice_invoice_InvoicesId] FOREIGN KEY ([InvoicesId]) REFERENCES [invoice] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ExpenseAndExpenseDetail_ExpenseId] ON [ExpenseAndExpenseDetail] ([ExpenseId]);

CREATE INDEX [IX_ExpenseInvoice_InvoicesId] ON [ExpenseInvoice] ([InvoicesId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251117221755_relacionInvoiceExpenseExpenseDetail', N'9.0.9');

ALTER TABLE [poll] ADD [ConsortiumId] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_poll_ConsortiumId] ON [poll] ([ConsortiumId]);

ALTER TABLE [poll] ADD CONSTRAINT [FK_poll_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251118173021_addRelationPollConsortium', N'9.0.9');

ALTER TABLE [thread] ADD [ConsortiumId] int NULL;

ALTER TABLE [Calls] ADD [ConsortiumId] int NULL;

CREATE INDEX [IX_thread_ConsortiumId] ON [thread] ([ConsortiumId]);

CREATE INDEX [IX_Calls_ConsortiumId] ON [Calls] ([ConsortiumId]);

ALTER TABLE [Calls] ADD CONSTRAINT [FK_Calls_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [thread] ADD CONSTRAINT [FK_thread_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251118204046_addConsortiumIdInPollAndForum', N'9.0.9');

ALTER TABLE [forum] ADD [ConsortiumId] int NULL;

CREATE INDEX [IX_forum_ConsortiumId] ON [forum] ([ConsortiumId]);

ALTER TABLE [forum] ADD CONSTRAINT [FK_forum_consortium_ConsortiumId] FOREIGN KEY ([ConsortiumId]) REFERENCES [consortium] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251118212226_addConsortiumIdInForum', N'9.0.9');

ALTER TABLE [Calls] ADD [Description] nvarchar(max) NULL;

ALTER TABLE [Calls] ADD [MeetingType] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Calls] ADD [Title] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251120161527_CallUpdate', N'9.0.9');

ALTER TABLE [reserves] DROP CONSTRAINT [FK_reserves_residence_Residence_id];

DROP INDEX [IX_reserves_Residence_id] ON [reserves];

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[reserves]') AND [c].[name] = N'Residence_id');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [reserves] DROP CONSTRAINT [' + @var41 + '];');
ALTER TABLE [reserves] DROP COLUMN [Residence_id];

ALTER TABLE [reserves] ADD [ResidenceId] int NULL;

CREATE INDEX [IX_reserves_ResidenceId] ON [reserves] ([ResidenceId]);

ALTER TABLE [reserves] ADD CONSTRAINT [FK_reserves_residence_ResidenceId] FOREIGN KEY ([ResidenceId]) REFERENCES [residence] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251122194250_DeleteResidenceInReserve', N'9.0.9');

COMMIT;
GO

