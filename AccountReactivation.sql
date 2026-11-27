BEGIN TRANSACTION;
GO

ALTER TABLE [UserAccounts] DROP CONSTRAINT [CK_UserAccounts_Role_AccountType];
GO

ALTER TABLE [VerificationCodes] ADD [Purpose] nvarchar(30) NOT NULL DEFAULT N'Registration';
GO

ALTER TABLE [UserAccounts] ADD [DeactivatedAt] datetime2 NULL;
GO

ALTER TABLE [UserAccounts] ADD [DeactivationReason] nvarchar(30) NULL;
GO

CREATE INDEX [IX_VerificationCodes_UserId_Purpose_CreatedAt] ON [VerificationCodes] ([UserId], [Purpose], [CreatedAt]);
GO

ALTER TABLE [VerificationCodes] ADD CONSTRAINT [CK_VerificationCodes_Purpose] CHECK ([Purpose] IN ('Registration', 'Reactivation'));
GO

ALTER TABLE [UserAccounts] ADD CONSTRAINT [CK_UserAccounts_DeactivationStatus] CHECK ((
            [IsActive] = 1
            AND [DeactivationReason] IS NULL
            AND [DeactivatedAt] IS NULL
        )
        OR
        (
            [IsActive] = 0
            AND
            (
                (
                    [DeactivationReason] IS NULL
                    AND [DeactivatedAt] IS NULL
                )
                OR
                (
                    [DeactivationReason] IS NOT NULL
                    AND [DeactivationReason] IN
                        ('SelfDeactivated', 'AdminSuspended')
                    AND [DeactivatedAt] IS NOT NULL
                )
            )
        ));
GO

ALTER TABLE [UserAccounts] ADD CONSTRAINT [CK_UserAccounts_Role_AccountType] CHECK ((
            [Role] = 'Customer'
            AND [AccountType] IS NOT NULL
            AND [AccountType] IN
                ('LocalCustomer', 'ForeignTourist')
        )
        OR
        (
            [Role] IN ('Admin', 'KitchenStaff', 'Shipper')
            AND [AccountType] IS NULL
            AND [Username] IS NOT NULL
        ));
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261127064247_AddAccountReactivation', N'8.0.0');
GO

COMMIT;
GO

