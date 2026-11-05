using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealTimeSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertGuidKeysToLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE [Users] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [Roles] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [Permissions] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [Servers] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [Reports] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [RefreshTokens] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [MaintenanceWindows] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [Alerts] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;
                ALTER TABLE [MetricSamples] ADD [Id_New] BIGINT IDENTITY(1,1) NOT NULL;

                ALTER TABLE [UserRoles] ADD [UserId_New] BIGINT NULL, [RoleId_New] BIGINT NULL;
                ALTER TABLE [RolePermissions] ADD [RoleId_New] BIGINT NULL, [PermissionId_New] BIGINT NULL;
                ALTER TABLE [Reports] ADD [RequestedByUserId_New] BIGINT NULL;
                ALTER TABLE [RefreshTokens] ADD [UserId_New] BIGINT NULL;
                ALTER TABLE [MetricSamples] ADD [ServerId_New] BIGINT NULL;
                ALTER TABLE [MaintenanceWindows] ADD [ServerId_New] BIGINT NULL;
                ALTER TABLE [Alerts] ADD [ServerId_New] BIGINT NULL, [AcknowledgedByUserId_New] BIGINT NULL;
                """);

            migrationBuilder.Sql(
                """

                UPDATE ur
                SET ur.[UserId_New] = u.[Id_New],
                    ur.[RoleId_New] = r.[Id_New]
                FROM [UserRoles] ur
                INNER JOIN [Users] u ON ur.[UserId] = u.[Id]
                INNER JOIN [Roles] r ON ur.[RoleId] = r.[Id];

                UPDATE rp
                SET rp.[RoleId_New] = r.[Id_New],
                    rp.[PermissionId_New] = p.[Id_New]
                FROM [RolePermissions] rp
                INNER JOIN [Roles] r ON rp.[RoleId] = r.[Id]
                INNER JOIN [Permissions] p ON rp.[PermissionId] = p.[Id];

                UPDATE rt
                SET rt.[UserId_New] = u.[Id_New]
                FROM [RefreshTokens] rt
                INNER JOIN [Users] u ON rt.[UserId] = u.[Id];

                UPDATE rep
                SET rep.[RequestedByUserId_New] = u.[Id_New]
                FROM [Reports] rep
                INNER JOIN [Users] u ON rep.[RequestedByUserId] = u.[Id];

                UPDATE mw
                SET mw.[ServerId_New] = s.[Id_New]
                FROM [MaintenanceWindows] mw
                INNER JOIN [Servers] s ON mw.[ServerId] = s.[Id];

                UPDATE ms
                SET ms.[ServerId_New] = s.[Id_New]
                FROM [MetricSamples] ms
                INNER JOIN [Servers] s ON ms.[ServerId] = s.[Id];

                UPDATE a
                SET a.[ServerId_New] = s.[Id_New]
                FROM [Alerts] a
                INNER JOIN [Servers] s ON a.[ServerId] = s.[Id];

                UPDATE a
                SET a.[AcknowledgedByUserId_New] = u.[Id_New]
                FROM [Alerts] a
                LEFT JOIN [Users] u ON a.[AcknowledgedByUserId] = u.[Id];
                """);

            migrationBuilder.Sql(
                """

                ALTER TABLE [UserRoles] ALTER COLUMN [UserId_New] BIGINT NOT NULL;
                ALTER TABLE [UserRoles] ALTER COLUMN [RoleId_New] BIGINT NOT NULL;
                ALTER TABLE [RolePermissions] ALTER COLUMN [RoleId_New] BIGINT NOT NULL;
                ALTER TABLE [RolePermissions] ALTER COLUMN [PermissionId_New] BIGINT NOT NULL;
                ALTER TABLE [RefreshTokens] ALTER COLUMN [UserId_New] BIGINT NOT NULL;
                ALTER TABLE [Reports] ALTER COLUMN [RequestedByUserId_New] BIGINT NOT NULL;
                ALTER TABLE [MaintenanceWindows] ALTER COLUMN [ServerId_New] BIGINT NOT NULL;
                ALTER TABLE [MetricSamples] ALTER COLUMN [ServerId_New] BIGINT NOT NULL;
                ALTER TABLE [Alerts] ALTER COLUMN [ServerId_New] BIGINT NOT NULL;
                """);

            migrationBuilder.Sql(
                """

                ALTER TABLE [Alerts] DROP CONSTRAINT [FK_Alerts_Servers_ServerId];
                ALTER TABLE [MaintenanceWindows] DROP CONSTRAINT [FK_MaintenanceWindows_Servers_ServerId];
                ALTER TABLE [MetricSamples] DROP CONSTRAINT [FK_MetricSamples_Servers_ServerId];
                ALTER TABLE [RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_Users_UserId];
                ALTER TABLE [Reports] DROP CONSTRAINT [FK_Reports_Users_RequestedByUserId];
                ALTER TABLE [RolePermissions] DROP CONSTRAINT [FK_RolePermissions_Permissions_PermissionId];
                ALTER TABLE [RolePermissions] DROP CONSTRAINT [FK_RolePermissions_Roles_RoleId];
                ALTER TABLE [UserRoles] DROP CONSTRAINT [FK_UserRoles_Roles_RoleId];
                ALTER TABLE [UserRoles] DROP CONSTRAINT [FK_UserRoles_Users_UserId];

                DROP INDEX [IX_Alerts_ServerId_CreatedAtUtc] ON [Alerts];
                DROP INDEX [IX_MaintenanceWindows_ServerId] ON [MaintenanceWindows];
                DROP INDEX [IX_MetricSamples_ServerId_CollectedAtUtc] ON [MetricSamples];
                DROP INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens];
                DROP INDEX [IX_Reports_RequestedByUserId] ON [Reports];
                DROP INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions];
                DROP INDEX [IX_UserRoles_RoleId] ON [UserRoles];

                ALTER TABLE [Alerts] DROP CONSTRAINT [PK_Alerts];
                ALTER TABLE [MaintenanceWindows] DROP CONSTRAINT [PK_MaintenanceWindows];
                ALTER TABLE [MetricSamples] DROP CONSTRAINT [PK_MetricSamples];
                ALTER TABLE [RefreshTokens] DROP CONSTRAINT [PK_RefreshTokens];
                ALTER TABLE [Reports] DROP CONSTRAINT [PK_Reports];
                ALTER TABLE [RolePermissions] DROP CONSTRAINT [PK_RolePermissions];
                ALTER TABLE [UserRoles] DROP CONSTRAINT [PK_UserRoles];
                ALTER TABLE [Permissions] DROP CONSTRAINT [PK_Permissions];
                ALTER TABLE [Roles] DROP CONSTRAINT [PK_Roles];
                ALTER TABLE [Servers] DROP CONSTRAINT [PK_Servers];
                ALTER TABLE [Users] DROP CONSTRAINT [PK_Users];
                """);

            migrationBuilder.Sql(
                """

                ALTER TABLE [Users] DROP COLUMN [Id];
                EXEC sp_rename N'dbo.Users.Id_New', N'Id', 'COLUMN';

                ALTER TABLE [Roles] DROP COLUMN [Id];
                EXEC sp_rename N'dbo.Roles.Id_New', N'Id', 'COLUMN';

                ALTER TABLE [Permissions] DROP COLUMN [Id];
                EXEC sp_rename N'dbo.Permissions.Id_New', N'Id', 'COLUMN';

                ALTER TABLE [Servers] DROP COLUMN [Id];
                EXEC sp_rename N'dbo.Servers.Id_New', N'Id', 'COLUMN';

                ALTER TABLE [Reports] DROP COLUMN [Id];
                ALTER TABLE [Reports] DROP COLUMN [RequestedByUserId];
                EXEC sp_rename N'dbo.Reports.Id_New', N'Id', 'COLUMN';
                EXEC sp_rename N'dbo.Reports.RequestedByUserId_New', N'RequestedByUserId', 'COLUMN';

                ALTER TABLE [RefreshTokens] DROP COLUMN [Id];
                ALTER TABLE [RefreshTokens] DROP COLUMN [UserId];
                EXEC sp_rename N'dbo.RefreshTokens.Id_New', N'Id', 'COLUMN';
                EXEC sp_rename N'dbo.RefreshTokens.UserId_New', N'UserId', 'COLUMN';

                ALTER TABLE [MaintenanceWindows] DROP COLUMN [Id];
                ALTER TABLE [MaintenanceWindows] DROP COLUMN [ServerId];
                EXEC sp_rename N'dbo.MaintenanceWindows.Id_New', N'Id', 'COLUMN';
                EXEC sp_rename N'dbo.MaintenanceWindows.ServerId_New', N'ServerId', 'COLUMN';

                ALTER TABLE [Alerts] DROP COLUMN [Id];
                ALTER TABLE [Alerts] DROP COLUMN [ServerId];
                ALTER TABLE [Alerts] DROP COLUMN [AcknowledgedByUserId];
                EXEC sp_rename N'dbo.Alerts.Id_New', N'Id', 'COLUMN';
                EXEC sp_rename N'dbo.Alerts.ServerId_New', N'ServerId', 'COLUMN';
                EXEC sp_rename N'dbo.Alerts.AcknowledgedByUserId_New', N'AcknowledgedByUserId', 'COLUMN';

                ALTER TABLE [UserRoles] DROP COLUMN [UserId];
                ALTER TABLE [UserRoles] DROP COLUMN [RoleId];
                EXEC sp_rename N'dbo.UserRoles.UserId_New', N'UserId', 'COLUMN';
                EXEC sp_rename N'dbo.UserRoles.RoleId_New', N'RoleId', 'COLUMN';

                ALTER TABLE [RolePermissions] DROP COLUMN [RoleId];
                ALTER TABLE [RolePermissions] DROP COLUMN [PermissionId];
                EXEC sp_rename N'dbo.RolePermissions.RoleId_New', N'RoleId', 'COLUMN';
                EXEC sp_rename N'dbo.RolePermissions.PermissionId_New', N'PermissionId', 'COLUMN';

                ALTER TABLE [MetricSamples] DROP COLUMN [Id];
                ALTER TABLE [MetricSamples] DROP COLUMN [ServerId];
                EXEC sp_rename N'dbo.MetricSamples.Id_New', N'Id', 'COLUMN';
                EXEC sp_rename N'dbo.MetricSamples.ServerId_New', N'ServerId', 'COLUMN';
                """);

            migrationBuilder.Sql(
                """

                ALTER TABLE [Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([Id]);
                ALTER TABLE [Roles] ADD CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]);
                ALTER TABLE [Permissions] ADD CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id]);
                ALTER TABLE [Servers] ADD CONSTRAINT [PK_Servers] PRIMARY KEY ([Id]);
                ALTER TABLE [Reports] ADD CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]);
                ALTER TABLE [RefreshTokens] ADD CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]);
                ALTER TABLE [MaintenanceWindows] ADD CONSTRAINT [PK_MaintenanceWindows] PRIMARY KEY ([Id]);
                ALTER TABLE [Alerts] ADD CONSTRAINT [PK_Alerts] PRIMARY KEY ([Id]);
                ALTER TABLE [MetricSamples] ADD CONSTRAINT [PK_MetricSamples] PRIMARY KEY ([Id]);
                ALTER TABLE [RolePermissions] ADD CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]);
                ALTER TABLE [UserRoles] ADD CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]);

                CREATE INDEX [IX_Alerts_ServerId_CreatedAtUtc] ON [Alerts] ([ServerId], [CreatedAtUtc]);
                CREATE INDEX [IX_MaintenanceWindows_ServerId] ON [MaintenanceWindows] ([ServerId]);
                CREATE INDEX [IX_MetricSamples_ServerId_CollectedAtUtc] ON [MetricSamples] ([ServerId], [CollectedAtUtc]);
                CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
                CREATE INDEX [IX_Reports_RequestedByUserId] ON [Reports] ([RequestedByUserId]);
                CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
                CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

                ALTER TABLE [Alerts] ADD CONSTRAINT [FK_Alerts_Servers_ServerId] FOREIGN KEY ([ServerId]) REFERENCES [Servers] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [MaintenanceWindows] ADD CONSTRAINT [FK_MaintenanceWindows_Servers_ServerId] FOREIGN KEY ([ServerId]) REFERENCES [Servers] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [MetricSamples] ADD CONSTRAINT [FK_MetricSamples_Servers_ServerId] FOREIGN KEY ([ServerId]) REFERENCES [Servers] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [RefreshTokens] ADD CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [Reports] ADD CONSTRAINT [FK_Reports_Users_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
                ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [UserRoles] ADD CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
                ALTER TABLE [UserRoles] ADD CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Down migration is not supported for Guid-to-long key conversion.");
        }
    }
}
