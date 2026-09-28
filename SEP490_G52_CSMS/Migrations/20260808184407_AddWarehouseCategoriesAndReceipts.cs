using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseCategoriesAndReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "material_categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_receipts",
                columns: table => new
                {
                    receipt_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    import_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_receipts", x => x.receipt_id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_receipt_items",
                columns: table => new
                {
                    receipt_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_receipt_items", x => x.receipt_item_id);
                    table.ForeignKey(
                        name: "FK_warehouse_receipt_items_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_warehouse_receipt_items_warehouse_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "warehouse_receipts",
                        principalColumn: "receipt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_material_categories_category_name",
                table: "material_categories",
                column: "category_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_receipt_items_material_id",
                table: "warehouse_receipt_items",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_receipt_items_receipt_id",
                table: "warehouse_receipt_items",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_receipts_receipt_code",
                table: "warehouse_receipts",
                column: "receipt_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_categories");

            migrationBuilder.DropTable(
                name: "warehouse_receipt_items");

            migrationBuilder.DropTable(
                name: "warehouse_receipts");
        }
    }
}
