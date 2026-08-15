using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Models.Sales;
namespace SEP490_G52_CSMS.Models
{
    public class CSMSAppDbContext : DbContext
    {
        public CSMSAppDbContext(DbContextOptions<CSMSAppDbContext> options) : base(options) { }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<BranchManager> BranchManagers { get; set; }
        public DbSet<FixedShift> FixedShifts { get; set; }
        public DbSet<WeeklyRosterGrid> WeeklyRosterGrids { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<CashHandover> CashHandovers { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<ShiftChangeRequest> ShiftChangeRequests { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<MasterProduct> MasterProducts { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<BranchMenu> BranchMenus { get; set; }
        public DbSet<MenuDetail> MenuDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<MaterialCategory> MaterialCategories { get; set; }
        public DbSet<WarehouseReceipt> WarehouseReceipts { get; set; }
        public DbSet<WarehouseReceiptItem> WarehouseReceiptItems { get; set; }
        public DbSet<BranchInventory> BranchInventories { get; set; }
        public DbSet<BranchSupplyRequest> BranchSupplyRequests { get; set; }
        public DbSet<BranchSupplyRequestItem> BranchSupplyRequestItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BranchSetting> BranchSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // CẤU HÌNH CHO BẢNG NHẬT KÝ CHẤM CÔNG (Sửa lỗi Cascade ở AttendanceLog)
            modelBuilder.Entity<Attendance.AttendanceLog>(entity =>
            {
                // Khóa ngoại trỏ tới bảng Nhân viên
                entity.HasOne(al => al.Employee)
                      .WithMany(e => e.AttendanceLogs)
                      .HasForeignKey(al => al.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict); // Đổi thành Restrict

                // Khóa ngoại trỏ tới bảng Lịch trực tuần
                entity.HasOne(al => al.WeeklyRosterGrid)
                      .WithMany(w => w.AttendanceLogs) // Hoặc .WithMany() nếu bảng WeeklyRosterGrid không có list này
                      .HasForeignKey(al => al.RosterId)
                      .OnDelete(DeleteBehavior.Restrict); // Đổi thành Restrict
            });
            // CẤU HÌNH CHO BẢNG BÀN GIAO KÉT (SỬA LỖI MULTIPLE CASCADE PATHS)
            modelBuilder.Entity<CashHandover>(entity =>
            {
                // Khóa ngoại 1: Thu ngân bàn giao ca
                entity.HasOne(ch => ch.OutgoingCashier)
                      .WithMany() // Hoặc .WithMany(e => e.OutgoingHandovers) nếu có thiết lập bộ sưu tập ở Employee
                      .HasForeignKey(ch => ch.OutgoingCashierId)
                      .OnDelete(DeleteBehavior.Restrict); // Đổi CASCADE thành RESTRICT

                // Khóa ngoại 2: Thu ngân nhận ca
                entity.HasOne(ch => ch.IncomingCashier)
                      .WithMany() // Hoặc .WithMany(e => e.IncomingHandovers)
                      .HasForeignKey(ch => ch.IncomingCashierId)
                      .OnDelete(DeleteBehavior.Restrict); // Đổi CASCADE thành RESTRICT

                // Khóa ngoại 3: Chi nhánh (Nên đổi luôn nếu gặp lỗi tương tự với bảng Branch)
                entity.HasOne(ch => ch.Branch)
                      .WithMany(b => b.CashHandovers)
                      .HasForeignKey(ch => ch.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // 2. Cấu hình khóa ngoại phức hợp cho bảng LeaveApplication
            modelBuilder.Entity<Attendance.LeaveApplication>()
                .HasOne(la => la.Approver)
                .WithMany()
                .HasForeignKey(la => new { la.ApprovedBranchId, la.ApprovedManagerId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance.LeaveApplication>()
                .HasOne(la => la.Employee)
                .WithMany(e => e.LeaveApplications)
                .HasForeignKey(la => la.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Cấu hình khóa ngoại phức hợp cho bảng ShiftChangeRequest
            modelBuilder.Entity<Attendance.ShiftChangeRequest>()
                .HasOne(scr => scr.Approver)
                .WithMany()
                .HasForeignKey(scr => new { scr.ApprovedBranchId, scr.ApprovedManagerId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance.ShiftChangeRequest>()
                .HasOne(scr => scr.RequestingEmployee)
                .WithMany()
                .HasForeignKey(scr => scr.RequestingEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            // Cấu hình các Composite Key (Khóa phức hợp nhiều trường)
            modelBuilder.Entity<BranchManager>().HasKey(bm => new { bm.BranchId, bm.ManagerId });
            modelBuilder.Entity<MenuDetail>().HasKey(md => new { md.MenuId, md.VariantId });
            modelBuilder.Entity<OrderItem>().HasKey(oi => new { oi.OrderId, oi.VariantId });

            // Cấu hình Unique Constraints (Ràng buộc duy nhất)
            modelBuilder.Entity<Branch>().HasIndex(b => b.BranchName).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(e => e.Username).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(e => e.CitizenId).IsUnique();
            modelBuilder.Entity<ProductCategory>().HasIndex(pc => pc.CategoryName).IsUnique();
            modelBuilder.Entity<MasterProduct>().HasIndex(mp => mp.ProductName).IsUnique();

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => new { pv.ProductId, pv.SizeVariant }).IsUnique();

            modelBuilder.Entity<AttendanceLog>()
                .HasIndex(a => new { a.RosterId, a.EmployeeId }).IsUnique();

            // Cấu hình Column Type cho các thuộc tính decimal để tránh cảnh báo mất dữ liệu
            modelBuilder.Entity<AttendanceLog>(entity =>
            {
                entity.Property(e => e.CheckInConfidence).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.CheckOutConfidence).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<CashHandover>(entity =>
            {
                entity.Property(e => e.ActualCash).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.BankTransferRevenue).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.CashRefundAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.InitialCash).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.MachineCashRevenue).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TheoreticalCash).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.RefundAmount).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 2)");
            });

            // Configure Material and Recipe relationships
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasIndex(m => new { m.MaterialName, m.Supplier }).IsUnique();
                entity.Property(m => m.UnitPrice).HasColumnType("decimal(18, 2)");
                entity.Property(m => m.StockQuantity).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.Property(r => r.Quantity).HasColumnType("decimal(18, 2)");

                entity.HasOne(r => r.ProductVariant)
                      .WithMany()
                      .HasForeignKey(r => r.VariantId)
                      .OnDelete(DeleteBehavior.Cascade); // Deleting variant deletes its recipe lines

                entity.HasOne(r => r.Material)
                      .WithMany()
                      .HasForeignKey(r => r.MaterialId)
                      .OnDelete(DeleteBehavior.Restrict); // Keep materials safe from automatic cascade
            });

            modelBuilder.Entity<MaterialCategory>().HasIndex(mc => mc.CategoryName).IsUnique();

            modelBuilder.Entity<WarehouseReceipt>(entity =>
            {
                entity.HasIndex(wr => wr.ReceiptCode).IsUnique();
                entity.Property(wr => wr.TotalAmount).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<WarehouseReceiptItem>(entity =>
            {
                entity.Property(wri => wri.Quantity).HasColumnType("decimal(18, 2)");
                entity.Property(wri => wri.UnitPrice).HasColumnType("decimal(18, 2)");
                entity.Property(wri => wri.Amount).HasColumnType("decimal(18, 2)");

                entity.HasOne(wri => wri.WarehouseReceipt)
                      .WithMany(wr => wr.Items)
                      .HasForeignKey(wri => wri.ReceiptId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wri => wri.Material)
                      .WithMany()
                      .HasForeignKey(wri => wri.MaterialId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BranchInventory>(entity =>
            {
                entity.HasIndex(bi => new { bi.BranchId, bi.MaterialId }).IsUnique();
                entity.Property(bi => bi.StockQuantity).HasColumnType("decimal(18, 2)");
                entity.Property(bi => bi.LowStockThreshold).HasColumnType("decimal(18, 2)");

                entity.HasOne(bi => bi.Branch)
                      .WithMany()
                      .HasForeignKey(bi => bi.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bi => bi.Material)
                      .WithMany()
                      .HasForeignKey(bi => bi.MaterialId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BranchSupplyRequestItem>(entity =>
            {
                entity.Property(ri => ri.QuantityRequested).HasColumnType("decimal(18, 2)");
                entity.Property(ri => ri.QuantityReleased).HasColumnType("decimal(18, 2)");

                entity.HasOne(ri => ri.BranchSupplyRequest)
                      .WithMany(r => r.Items)
                      .HasForeignKey(ri => ri.RequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ri => ri.Material)
                      .WithMany()
                      .HasForeignKey(ri => ri.MaterialId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //base.OnModelCreating(modelBuilder);
        }
    }
}