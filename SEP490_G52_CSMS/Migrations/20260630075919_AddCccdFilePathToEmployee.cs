using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCccdFilePathToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cccd_file_path",
                table: "employees",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cccd_file_path",
                table: "employees");
        }
    }
}
