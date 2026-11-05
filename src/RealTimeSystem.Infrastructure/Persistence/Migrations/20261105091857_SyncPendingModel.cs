using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealTimeSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Servers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Servers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Servers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Servers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Servers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Reports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "RefreshTokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "RefreshTokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Permissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Permissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "MetricSamples",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "MetricSamples",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MetricSamples",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MetricSamples",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "MetricSamples",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "MetricSamples",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "MaintenanceWindows",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "MaintenanceWindows",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MaintenanceWindows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MaintenanceWindows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "MaintenanceWindows",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "MaintenanceWindows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Alerts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Alerts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Alerts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Alerts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MetricSamples");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "MetricSamples");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MetricSamples");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MetricSamples");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MetricSamples");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "MetricSamples");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MaintenanceWindows");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "MaintenanceWindows");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MaintenanceWindows");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MaintenanceWindows");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MaintenanceWindows");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "MaintenanceWindows");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Alerts");
        }
    }
}
