using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCashHandoverStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HandoverRecords");

            migrationBuilder.DropTable(
                name: "ShiftSessions");

            migrationBuilder.AddColumn<DateTime>(
                name: "closed_at",
                table: "cash_handovers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "cash_handovers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "opened_at",
                table: "cash_handovers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "cash_handovers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "closed_at",
                table: "cash_handovers");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "cash_handovers");

            migrationBuilder.DropColumn(
                name: "opened_at",
                table: "cash_handovers");

            migrationBuilder.DropColumn(
                name: "status",
                table: "cash_handovers");

            migrationBuilder.CreateTable(
                name: "ShiftSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashierId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClosingFloat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShiftCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartingFloat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HandoverRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromCashierId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftSessionId = table.Column<int>(type: "int", nullable: false),
                    ToCashierId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoverRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandoverRecords_ShiftSessions_ShiftSessionId",
                        column: x => x.ShiftSessionId,
                        principalTable: "ShiftSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HandoverRecords_ShiftSessionId",
                table: "HandoverRecords",
                column: "ShiftSessionId");
        }
    }
}
