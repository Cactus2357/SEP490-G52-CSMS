using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialsAndRecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cash_handovers_branches_BranchId1",
                table: "cash_handovers");

            migrationBuilder.DropIndex(
                name: "IX_cash_handovers_BranchId1",
                table: "cash_handovers");

            migrationBuilder.DropColumn(
                name: "BranchId1",
                table: "cash_handovers");

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    material_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    material_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    material_kind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    storage_unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.material_id);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    recipe_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    variant_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    branch_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.recipe_id);
                    table.ForeignKey(
                        name: "FK_recipes_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipes_product_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "product_variants",
                        principalColumn: "variant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_materials_material_name_supplier",
                table: "materials",
                columns: new[] { "material_name", "supplier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipes_material_id",
                table: "recipes",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_variant_id",
                table: "recipes",
                column: "variant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipes");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.AddColumn<string>(
                name: "BranchId1",
                table: "cash_handovers",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cash_handovers_BranchId1",
                table: "cash_handovers",
                column: "BranchId1");

            migrationBuilder.AddForeignKey(
                name: "FK_cash_handovers_branches_BranchId1",
                table: "cash_handovers",
                column: "BranchId1",
                principalTable: "branches",
                principalColumn: "branch_id");
        }
    }
}
