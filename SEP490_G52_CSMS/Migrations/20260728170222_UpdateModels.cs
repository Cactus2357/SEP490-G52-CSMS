using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "branch_menus",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recipient_name",
                table: "orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "branch_menus");

            migrationBuilder.DropColumn(
                name: "recipient_name",
                table: "orders");
        }
    }
}
