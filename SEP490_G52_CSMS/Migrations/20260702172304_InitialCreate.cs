using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_G52_CSMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    branch_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    branch_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    opening_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    closing_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.branch_id);
                });

            migrationBuilder.CreateTable(
                name: "fixed_shifts",
                columns: table => new
                {
                    shift_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shift_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fixed_shifts", x => x.shift_id);
                });

            migrationBuilder.CreateTable(
                name: "product_categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "branch_menus",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    branch_id = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    menu_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_menus", x => x.menu_id);
                    table.ForeignKey(
                        name: "FK_branch_menus_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id");
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    employment_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    citizen_id = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    contract_file_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    face_data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    failed_login_attempts = table.Column<int>(type: "int", nullable: false),
                    lockout_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    branch_id = table.Column<string>(type: "nvarchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.employee_id);
                    table.ForeignKey(
                        name: "FK_employees_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id");
                });

            migrationBuilder.CreateTable(
                name: "master_products",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_products", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_master_products_product_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "product_categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branch_managers",
                columns: table => new
                {
                    branch_id = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    manager_id = table.Column<int>(type: "int", nullable: false),
                    appointed_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_managers", x => new { x.branch_id, x.manager_id });
                    table.ForeignKey(
                        name: "FK_branch_managers_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_branch_managers_employees_manager_id",
                        column: x => x.manager_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cash_handovers",
                columns: table => new
                {
                    handover_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    branch_id = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    handover_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    shift_id = table.Column<int>(type: "int", nullable: false),
                    outgoing_cashier_id = table.Column<int>(type: "int", nullable: false),
                    incoming_cashier_id = table.Column<int>(type: "int", nullable: false),
                    initial_cash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    machine_cash_revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    theoretical_cash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    actual_cash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_password_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    BranchId1 = table.Column<string>(type: "nvarchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_handovers", x => x.handover_id);
                    table.ForeignKey(
                        name: "FK_cash_handovers_branches_BranchId1",
                        column: x => x.BranchId1,
                        principalTable: "branches",
                        principalColumn: "branch_id");
                    table.ForeignKey(
                        name: "FK_cash_handovers_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_handovers_employees_incoming_cashier_id",
                        column: x => x.incoming_cashier_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_handovers_employees_outgoing_cashier_id",
                        column: x => x.outgoing_cashier_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cash_handovers_fixed_shifts_shift_id",
                        column: x => x.shift_id,
                        principalTable: "fixed_shifts",
                        principalColumn: "shift_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    branch_id = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    cashier_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    brewing_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_orders_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id");
                    table.ForeignKey(
                        name: "FK_orders_employees_cashier_id",
                        column: x => x.cashier_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weekly_roster_grids",
                columns: table => new
                {
                    roster_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    branch_id = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    assignment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    shift_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_roster_grids", x => x.roster_id);
                    table.ForeignKey(
                        name: "FK_weekly_roster_grids_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_weekly_roster_grids_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_weekly_roster_grids_fixed_shifts_shift_id",
                        column: x => x.shift_id,
                        principalTable: "fixed_shifts",
                        principalColumn: "shift_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    variant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    size_variant = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    selling_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.variant_id);
                    table.ForeignKey(
                        name: "FK_product_variants_master_products_product_id",
                        column: x => x.product_id,
                        principalTable: "master_products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leave_applications",
                columns: table => new
                {
                    application_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    approved_branch_id = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    approved_manager_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_applications", x => x.application_id);
                    table.ForeignKey(
                        name: "FK_leave_applications_branch_managers_approved_branch_id_approved_manager_id",
                        columns: x => new { x.approved_branch_id, x.approved_manager_id },
                        principalTable: "branch_managers",
                        principalColumns: new[] { "branch_id", "manager_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_leave_applications_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shift_change_requests",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    requesting_employee_id = table.Column<int>(type: "int", nullable: false),
                    aspiration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    approved_branch_id = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    approved_manager_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shift_change_requests", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_shift_change_requests_branch_managers_approved_branch_id_approved_manager_id",
                        columns: x => new { x.approved_branch_id, x.approved_manager_id },
                        principalTable: "branch_managers",
                        principalColumns: new[] { "branch_id", "manager_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shift_change_requests_employees_requesting_employee_id",
                        column: x => x.requesting_employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attendance_logs",
                columns: table => new
                {
                    attendance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roster_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    check_in_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_face_check_in_valid = table.Column<bool>(type: "bit", nullable: false),
                    check_in_confidence = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    check_in_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    check_out_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_face_check_out_valid = table.Column<bool>(type: "bit", nullable: false),
                    check_out_confidence = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    check_out_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    overall_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_logs", x => x.attendance_id);
                    table.ForeignKey(
                        name: "FK_attendance_logs_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attendance_logs_weekly_roster_grids_roster_id",
                        column: x => x.roster_id,
                        principalTable: "weekly_roster_grids",
                        principalColumn: "roster_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_details",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "int", nullable: false),
                    variant_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_details", x => new { x.menu_id, x.variant_id });
                    table.ForeignKey(
                        name: "FK_menu_details_branch_menus_menu_id",
                        column: x => x.menu_id,
                        principalTable: "branch_menus",
                        principalColumn: "menu_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_details_product_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "product_variants",
                        principalColumn: "variant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    variant_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => new { x.order_id, x.variant_id });
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_items_product_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "product_variants",
                        principalColumn: "variant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_logs_employee_id",
                table: "attendance_logs",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_logs_roster_id_employee_id",
                table: "attendance_logs",
                columns: new[] { "roster_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_branch_managers_manager_id",
                table: "branch_managers",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_branch_menus_branch_id",
                table: "branch_menus",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_branches_branch_name",
                table: "branches",
                column: "branch_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cash_handovers_branch_id",
                table: "cash_handovers",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_handovers_BranchId1",
                table: "cash_handovers",
                column: "BranchId1");

            migrationBuilder.CreateIndex(
                name: "IX_cash_handovers_incoming_cashier_id",
                table: "cash_handovers",
                column: "incoming_cashier_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_handovers_outgoing_cashier_id",
                table: "cash_handovers",
                column: "outgoing_cashier_id");

            migrationBuilder.CreateIndex(
                name: "IX_cash_handovers_shift_id",
                table: "cash_handovers",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_branch_id",
                table: "employees",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_citizen_id",
                table: "employees",
                column: "citizen_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_email",
                table: "employees",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_username",
                table: "employees",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leave_applications_approved_branch_id_approved_manager_id",
                table: "leave_applications",
                columns: new[] { "approved_branch_id", "approved_manager_id" });

            migrationBuilder.CreateIndex(
                name: "IX_leave_applications_employee_id",
                table: "leave_applications",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_master_products_category_id",
                table: "master_products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_master_products_product_name",
                table: "master_products",
                column: "product_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_details_variant_id",
                table: "menu_details",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_variant_id",
                table: "order_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_branch_id",
                table: "orders",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_cashier_id",
                table: "orders",
                column: "cashier_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_categories_category_name",
                table: "product_categories",
                column: "category_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_product_id_size_variant",
                table: "product_variants",
                columns: new[] { "product_id", "size_variant" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shift_change_requests_approved_branch_id_approved_manager_id",
                table: "shift_change_requests",
                columns: new[] { "approved_branch_id", "approved_manager_id" });

            migrationBuilder.CreateIndex(
                name: "IX_shift_change_requests_requesting_employee_id",
                table: "shift_change_requests",
                column: "requesting_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_roster_grids_branch_id",
                table: "weekly_roster_grids",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_roster_grids_employee_id",
                table: "weekly_roster_grids",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_roster_grids_shift_id",
                table: "weekly_roster_grids",
                column: "shift_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_logs");

            migrationBuilder.DropTable(
                name: "cash_handovers");

            migrationBuilder.DropTable(
                name: "leave_applications");

            migrationBuilder.DropTable(
                name: "menu_details");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "shift_change_requests");

            migrationBuilder.DropTable(
                name: "weekly_roster_grids");

            migrationBuilder.DropTable(
                name: "branch_menus");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "product_variants");

            migrationBuilder.DropTable(
                name: "branch_managers");

            migrationBuilder.DropTable(
                name: "fixed_shifts");

            migrationBuilder.DropTable(
                name: "master_products");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "product_categories");

            migrationBuilder.DropTable(
                name: "branches");
        }
    }
}
