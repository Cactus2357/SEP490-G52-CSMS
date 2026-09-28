using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchSupplyRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branch_supply_requests",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    branch_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    request_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    warehouse_note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deliverer_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deliverer_phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    received_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_supply_requests", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_branch_supply_requests_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branch_supply_request_items",
                columns: table => new
                {
                    request_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    quantity_requested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    quantity_released = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_supply_request_items", x => x.request_item_id);
                    table.ForeignKey(
                        name: "FK_branch_supply_request_items_branch_supply_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "branch_supply_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_branch_supply_request_items_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_branch_supply_request_items_material_id",
                table: "branch_supply_request_items",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_branch_supply_request_items_request_id",
                table: "branch_supply_request_items",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_branch_supply_requests_branch_id",
                table: "branch_supply_requests",
                column: "branch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_supply_request_items");

            migrationBuilder.DropTable(
                name: "branch_supply_requests");
        }
    }
}
