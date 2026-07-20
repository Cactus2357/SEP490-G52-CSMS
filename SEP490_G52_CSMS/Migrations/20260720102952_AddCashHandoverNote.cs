using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCashHandoverNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "cash_handovers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notes",
                table: "cash_handovers");
        }
    }
}
