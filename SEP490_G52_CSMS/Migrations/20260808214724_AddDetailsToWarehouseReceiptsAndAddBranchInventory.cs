using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailsToWarehouseReceiptsAndAddBranchInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "warehouse_receipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "deliverer_name",
                table: "warehouse_receipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "deliverer_phone",
                table: "warehouse_receipts",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receiver_name",
                table: "warehouse_receipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "branch_inventories",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    branch_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    stock_quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    low_stock_threshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_inventories", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK_branch_inventories_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_branch_inventories_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_branch_inventories_branch_id_material_id",
                table: "branch_inventories",
                columns: new[] { "branch_id", "material_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_branch_inventories_material_id",
                table: "branch_inventories",
                column: "material_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_inventories");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "warehouse_receipts");

            migrationBuilder.DropColumn(
                name: "deliverer_name",
                table: "warehouse_receipts");

            migrationBuilder.DropColumn(
                name: "deliverer_phone",
                table: "warehouse_receipts");

            migrationBuilder.DropColumn(
                name: "receiver_name",
                table: "warehouse_receipts");
        }
    }
}
