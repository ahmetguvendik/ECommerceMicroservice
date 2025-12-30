using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RekeyProductOutboxWithIdempotentToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "ProductOutboxes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductOutboxes");

            migrationBuilder.DropColumn(
                name: "ModifiedTime",
                table: "ProductOutboxes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ProductOutboxes",
                newName: "IdempotentToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdempotentToken",
                table: "ProductOutboxes",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "ProductOutboxes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductOutboxes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedTime",
                table: "ProductOutboxes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
