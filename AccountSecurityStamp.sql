BEGIN TRANSACTION;
GO

ALTER TABLE [UserAccounts] ADD [SecurityStamp] nvarchar(64) NULL;
GO

                UPDATE [UserAccounts]
                SET [SecurityStamp] =
                    LOWER(
                        REPLACE(
                            CONVERT(NVARCHAR(36), NEWID()),
                            '-', ''
                        )
                    )
                WHERE [SecurityStamp] IS NULL
                   OR [SecurityStamp] = '';
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserAccounts]') AND [c].[name] = N'SecurityStamp');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [UserAccounts] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [UserAccounts] ALTER COLUMN [SecurityStamp] nvarchar(64) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261127093748_AddAccountSecurityStamp', N'8.0.0');
GO

COMMIT;
GO

