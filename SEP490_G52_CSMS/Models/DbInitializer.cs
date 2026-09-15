using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models.Attendance;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Models
{
    public static class DbInitializer
    {
        private static readonly Random _rng = new Random(12345);

        private static readonly string[] Ho = { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương" };
        private static readonly string[] Dem = { "Văn", "Thị", "Hữu", "Đức", "Minh", "Thanh", "Quốc", "Kim", "Ngọc", "Xuân" };
        private static readonly string[] Ten = { "An", "Bình", "Cường", "Dũng", "Em", "Giang", "Hà", "Hùng", "Khánh", "Lan", "Mai", "Nam", "Oanh", "Phúc", "Quân", "Sơn", "Thảo", "Uyên", "Vy", "Yến" };

        private static string RandomFullName() =>
            $"{Ho[_rng.Next(Ho.Length)]} {Dem[_rng.Next(Dem.Length)]} {Ten[_rng.Next(Ten.Length)]}";

        public static void Seed(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<CSMSAppDbContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            EnsureTablesCreated(context);

            try
            {
                // Seed RManager if it doesn't exist
                if (!context.Employees.Any(e => e.Role == "RManager"))
                {
                    context.Employees.Add(new Employee
                    {
                        FullName = "Regional Manager",
                        Username = "rmanager",
                        Password = Commons.DAT_PasswordHasher.HashPassword("12345678"),
                        Email = "rmanager@gmail.com",
                        PhoneNumber = "0900000000",
                        CitizenId = "000000000000",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Address = "Headquarters",
                        Role = "RManager",
                        EmploymentType = "Full-time",
                        BranchId = null,
                        Status = "Active"
                    });
                    context.SaveChanges();
                }

                // Seed WarehouseManager if it doesn't exist
                if (!context.Employees.Any(e => e.Role == "WarehouseManager"))
                {
                    context.Employees.Add(new Employee
                    {
                        FullName = "Warehouse Manager",
                        Username = "wmanager",
                        Password = Commons.DAT_PasswordHasher.HashPassword("12345678"),
                        Email = "wmanager@gmail.com",
                        PhoneNumber = "0911111111",
                        CitizenId = "111111111111",
                        DateOfBirth = new DateTime(1992, 1, 1),
                        Address = "Central Warehouse",
                        Role = "WarehouseManager",
                        EmploymentType = "Full-time",
                        BranchId = null,
                        Status = "Active"
                    });
                    context.SaveChanges();
                }

                // Skip branch-specific seeding if already seeded beyond the base 3 branches or if CB004/CB005 already exist
                if (context.Branches.Count() > 3 || context.Branches.Any(b => b.BranchId == "CB004" || b.BranchId == "CB005"))
                {
                    SeedMaterials(context);
                    return;
                }

                // ---------- 1. BRANCHES (2 more, total 5) ----------
                var newBranches = new List<Branch>
                {
                    new Branch { BranchId = "CB004", BranchName = "Chi nhánh Tân Bình", Address = "78 Cộng Hòa, Tân Bình, TPHCM", OpeningTime = new TimeSpan(6,30,0), ClosingTime = new TimeSpan(22,30,0), Status = "Active" },
                    new Branch { BranchId = "CB005", BranchName = "Chi nhánh Thủ Đức", Address = "12 Võ Văn Ngân, Thủ Đức, TPHCM", OpeningTime = new TimeSpan(7,0,0), ClosingTime = new TimeSpan(21,30,0), Status = "Active" }
                };
                context.Branches.AddRange(newBranches);
                context.SaveChanges();

                var allBranchIds = context.Branches.Select(b => b.BranchId).ToList();

                // ---------- 2. FIXED SHIFTS ----------
                var shifts = new List<FixedShift>
                {
                    new FixedShift { ShiftName = "Ca 1", StartTime = new TimeSpan(7,0,0), EndTime = new TimeSpan(12,0,0) },
                    new FixedShift { ShiftName = "Ca 2", StartTime = new TimeSpan(13,0,0), EndTime = new TimeSpan(18,0,0) },
                    new FixedShift { ShiftName = "Ca 3", StartTime = new TimeSpan(19,0,0), EndTime = new TimeSpan(23,59,59) }
                };
                context.FixedShifts.AddRange(shifts);
                context.SaveChanges();

                // ---------- 3. EMPLOYEES ----------
                string[] roles = { "Cashier", "Bartender", "Busser" };
                string[] empTypes = { "Full-time", "Part-time" };
                var employeesByBranch = new Dictionary<string, List<Employee>>();
                int globalEmpCounter = 3; // manager1/manager2 already exist

                foreach (var branchId in allBranchIds)
                {
                    var branchEmployees = new List<Employee>();
                    var roleCounter = new Dictionary<string, int>(); // per-branch, per-role numbering

                    int NextRoleNumber(string role)
                    {
                        roleCounter.TryGetValue(role, out var n);
                        n++;
                        roleCounter[role] = n;
                        return n;
                    }

                    // 1 manager per branch (CB001/CB002 already have managers, add for CB003-CB005)
                    bool needsManager = !context.BranchManagers.Any(bm => bm.BranchId == branchId);
                    if (needsManager)
                    {
                        globalEmpCounter++;
                        int mgrNum = NextRoleNumber("BranchManager");
                        var mgr = new Employee
                        {
                            FullName = RandomFullName(),
                            Username = $"BranchManager{branchId}{mgrNum:D2}",
                            Password = Commons.DAT_PasswordHasher.HashPassword("12345678"),
                            Email = $"manager{globalEmpCounter}@gmail.com",
                            PhoneNumber = $"09{globalEmpCounter:D8}",
                            CitizenId = $"{globalEmpCounter:D12}",
                            DateOfBirth = new DateTime(1988 + _rng.Next(0, 8), _rng.Next(1, 13), _rng.Next(1, 28)),
                            Address = "Địa chỉ quản lý " + branchId,
                            Role = "BranchManager",
                            EmploymentType = "Full-time",
                            BranchId = branchId,
                            Status = "Active"
                        };
                        branchEmployees.Add(mgr);
                    }

                    // ~20 staff per branch
                    int staffCount = _rng.Next(18, 23);
                    for (int i = 0; i < staffCount; i++)
                    {
                        globalEmpCounter++;
                        var role = roles[_rng.Next(roles.Length)];
                        int roleNum = NextRoleNumber(role);
                        branchEmployees.Add(new Employee
                        {
                            FullName = RandomFullName(),
                            Username = $"{role}{branchId}{roleNum:D2}",
                            Password = Commons.DAT_PasswordHasher.HashPassword("12345678"),
                            Email = $"emp{globalEmpCounter:D3}@gmail.com",
                            PhoneNumber = $"09{globalEmpCounter:D8}",
                            CitizenId = $"{globalEmpCounter:D12}",
                            DateOfBirth = new DateTime(1995 + _rng.Next(0, 8), _rng.Next(1, 13), _rng.Next(1, 28)),
                            Address = "Địa chỉ nhân viên " + branchId,
                            Role = role,
                            EmploymentType = empTypes[_rng.Next(empTypes.Length)],
                            BranchId = branchId,
                            Status = "Active"
                        });
                    }

                    context.Employees.AddRange(branchEmployees);
                    context.SaveChanges();
                    employeesByBranch[branchId] = branchEmployees;
                }

                // ---------- 4. BRANCH MANAGERS (for newly created managers) ----------
                var newManagerLinks = new List<BranchManager>();
                foreach (var branchId in allBranchIds)
                {
                    if (!context.BranchManagers.Any(bm => bm.BranchId == branchId))
                    {
                        var mgr = employeesByBranch[branchId].FirstOrDefault(e => e.Role == "BranchManager");
                        if (mgr != null)
                            newManagerLinks.Add(new BranchManager { BranchId = branchId, ManagerId = mgr.EmployeeId, AppointedDate = DateTime.UtcNow.AddMonths(-_rng.Next(1, 24)) });
                    }
                }
                context.BranchManagers.AddRange(newManagerLinks);
                context.SaveChanges();

                // ---------- 5. WEEKLY ROSTER GRID + ATTENDANCE LOGS ----------
                var today = DateTime.UtcNow.Date;
                var startDate = today.AddDays(-10); // 10 days: 5 past-with-attendance, 5 future/no-attendance
                var rosterEntries = new List<WeeklyRosterGrid>();

                foreach (var branchId in allBranchIds)
                {
                    var staff = employeesByBranch[branchId].Where(e => e.Role != "BranchManager").ToList();
                    for (int day = 0; day < 10; day++)
                    {
                        var date = startDate.AddDays(day);
                        // assign ~4 staff per day (rotating)
                        var todaysStaff = staff.OrderBy(_ => _rng.Next()).Take(Math.Min(4, staff.Count)).ToList();
                        foreach (var emp in todaysStaff)
                        {
                            rosterEntries.Add(new WeeklyRosterGrid
                            {
                                BranchId = branchId,
                                AssignmentDate = date,
                                ShiftId = shifts[_rng.Next(shifts.Count)].ShiftId,
                                EmployeeId = emp.EmployeeId
                            });
                        }
                    }
                }
                context.WeeklyRosterGrids.AddRange(rosterEntries);
                context.SaveChanges();

                // Attendance logs only for past days (day 0-4 of the 10-day window)
                var attendanceLogs = new List<AttendanceLog>();
                var cutoff = startDate.AddDays(5);
                foreach (var roster in context.WeeklyRosterGrids.Where(r => r.AssignmentDate < cutoff))
                {
                    bool present = _rng.NextDouble() > 0.1; // 90% attendance rate
                    attendanceLogs.Add(new AttendanceLog
                    {
                        RosterId = roster.RosterId,
                        EmployeeId = roster.EmployeeId,
                        CheckInTime = present ? roster.AssignmentDate.Add(new TimeSpan(_rng.Next(6, 9), _rng.Next(0, 60), 0)) : (DateTime?)null,
                        IsFaceCheckInValid = present,
                        CheckInConfidence = present ? (decimal)(90 + _rng.NextDouble() * 9) : (decimal?)null,
                        CheckInStatus = present ? "OnTime" : "Absent",
                        CheckOutTime = present ? roster.AssignmentDate.Add(new TimeSpan(_rng.Next(15, 22), _rng.Next(0, 60), 0)) : (DateTime?)null,
                        IsFaceCheckOutValid = present,
                        CheckOutConfidence = present ? (decimal)(90 + _rng.NextDouble() * 9) : (decimal?)null,
                        CheckOutStatus = present ? "CheckedOut" : "NotYetCheckOut",
                        OverallStatus = present ? "Present" : "Absent",
                        Notes = present ? null : "Nghỉ không phép"
                    });
                }
                context.AttendanceLogs.AddRange(attendanceLogs);
                context.SaveChanges();

                // ---------- 6. CASH HANDOVERS (Unique per Branch, Shift, Date) ----------
                var handovers = new List<CashHandover>();
                var seenHandovers = new HashSet<(string BranchId, int ShiftId, DateTime Date)>();

                var pastRosters = context.WeeklyRosterGrids
                    .Include(r => r.Employee)
                    .Where(r => r.AssignmentDate < cutoff && r.Employee != null && r.Employee.Role == "Cashier")
                    .OrderBy(r => r.AssignmentDate)
                    .ThenBy(r => r.ShiftId)
                    .ToList();

                foreach (var roster in pastRosters)
                {
                    var key = (roster.BranchId, roster.ShiftId, roster.AssignmentDate.Date);
                    if (seenHandovers.Contains(key)) continue;
                    seenHandovers.Add(key);

                    var branchStaff = employeesByBranch.GetValueOrDefault(roster.BranchId)
                        ?.Where(e => e.Role == "Cashier")
                        .ToList() ?? new List<Employee>();

                    var incoming = branchStaff.FirstOrDefault(e => e.EmployeeId != roster.EmployeeId) ?? roster.Employee!;

                    decimal initial = 500000m;
                    decimal totalRevenue = _rng.Next(1000000, 5000000);
                    decimal cashlessRevenue = Math.Round(totalRevenue * 0.6m);
                    decimal cashRevenue = totalRevenue - cashlessRevenue;
                    decimal theoretical = initial + cashRevenue;
                    decimal actual = theoretical + _rng.Next(-20000, 20000);

                    handovers.Add(new CashHandover
                    {
                        BranchId = roster.BranchId,
                        HandoverDate = roster.AssignmentDate.Date,
                        ShiftId = roster.ShiftId,
                        OutgoingCashierId = roster.EmployeeId,
                        IncomingCashierId = incoming.EmployeeId,
                        InitialCash = initial,
                        MachineCashRevenue = cashlessRevenue,
                        BankTransferRevenue = cashlessRevenue,
                        TheoreticalCash = theoretical,
                        ActualCash = actual,
                        IsPasswordConfirmed = true,
                        Status = "Closed",
                        OpenedAt = roster.AssignmentDate.Date.AddHours(6),
                        ClosedAt = roster.AssignmentDate.Date.AddHours(12)
                    });
                }
                context.CashHandovers.AddRange(handovers);
                context.SaveChanges();

                // ---------- 7. LEAVE APPLICATIONS ----------
                var leaveApps = new List<LeaveApplication>();
                var branchManagerLinks = context.BranchManagers.ToList();
                var allStaffFlat = employeesByBranch.Values.SelectMany(x => x).Where(e => e.Role != "BranchManager").ToList();

                foreach (var _ in Enumerable.Range(0, 15))
                {
                    var emp = allStaffFlat[_rng.Next(allStaffFlat.Count)];
                    var bmLink = branchManagerLinks.FirstOrDefault(bm => bm.BranchId == emp.BranchId);
                    var start = today.AddDays(_rng.Next(1, 20));
                    leaveApps.Add(new LeaveApplication
                    {
                        EmployeeId = emp.EmployeeId,
                        StartDate = start,
                        EndDate = start.AddDays(_rng.Next(1, 4)),
                        Reason = "Việc gia đình",
                        SubmittedAt = today.AddDays(-_rng.Next(1, 10)),
                        Status = new[] { "Pending", "Approved", "Rejected" }[_rng.Next(3)],
                        ApprovedBranchId = bmLink?.BranchId,
                        ApprovedManagerId = bmLink?.ManagerId
                    });
                }
                context.LeaveApplications.AddRange(leaveApps);

                // ---------- 8. SHIFT CHANGE REQUESTS ----------
                var shiftChangeReqs = new List<ShiftChangeRequest>();
                foreach (var _ in Enumerable.Range(0, 10))
                {
                    var emp = allStaffFlat[_rng.Next(allStaffFlat.Count)];
                    var bmLink = branchManagerLinks.FirstOrDefault(bm => bm.BranchId == emp.BranchId);
                    shiftChangeReqs.Add(new ShiftChangeRequest
                    {
                        RequestingEmployeeId = emp.EmployeeId,
                        Aspiration = "Đổi sang ca sáng",
                        Reason = "Lý do cá nhân",
                        SubmittedAt = today.AddDays(-_rng.Next(1, 10)),
                        Status = new[] { "Submitted", "Approved", "Rejected" }[_rng.Next(3)],
                        ApprovedBranchId = bmLink?.BranchId,
                        ApprovedManagerId = bmLink?.ManagerId
                    });
                }
                context.ShiftChangeRequests.AddRange(shiftChangeReqs);
                context.SaveChanges();

                // ---------- 9. PRODUCT CATEGORIES ----------
                var categories = new List<ProductCategory>
                {
                    new ProductCategory { CategoryName = "Cà phê", Description = "Các loại cà phê pha máy và pha phin" },
                    new ProductCategory { CategoryName = "Trà", Description = "Trà trái cây và trà truyền thống" },
                    new ProductCategory { CategoryName = "Nước ép & Sinh tố", Description = "Nước ép trái cây tươi và sinh tố" },
                    new ProductCategory { CategoryName = "Đá xay", Description = "Các loại đồ uống đá xay" },
                    new ProductCategory { CategoryName = "Bánh ngọt", Description = "Bánh ngọt và tráng miệng" },
                    new ProductCategory { CategoryName = "Topping", Description = "Topping thêm cho đồ uống" }
                };
                context.ProductCategories.AddRange(categories);
                context.SaveChanges();

                // ---------- 10. MASTER PRODUCTS ----------
                var productsByCategory = new Dictionary<string, string[]>
                {
                    ["Cà phê"] = new[] { "Cà phê đen", "Cà phê sữa", "Bạc xỉu", "Espresso" },
                    ["Trà"] = new[] { "Trà đào cam sả", "Trà vải", "Trà sen vàng", "Trà oolong" },
                    ["Nước ép & Sinh tố"] = new[] { "Nước ép cam", "Nước ép dưa hấu", "Nước ép táo", "Sinh tố bơ" },
                    ["Đá xay"] = new[] { "Frappuccino cà phê", "Matcha đá xay", "Chocolate đá xay", "Caramel đá xay" },
                    ["Bánh ngọt"] = new[] { "Bánh croissant", "Bánh tiramisu", "Bánh su kem", "Bánh cookie" },
                    ["Topping"] = new[] { "Trân châu đen", "Thạch dừa", "Pudding trứng", "Kem cheese" }
                };

                var masterProducts = new List<MasterProduct>();
                foreach (var cat in categories)
                {
                    foreach (var name in productsByCategory[cat.CategoryName])
                    {
                        masterProducts.Add(new MasterProduct
                        {
                            ProductName = name,
                            Description = $"{name} thơm ngon",
                            CategoryId = cat.CategoryId,
                            Status = "Active"
                        });
                    }
                }
                context.MasterProducts.AddRange(masterProducts);
                context.SaveChanges();

                // ---------- 11. PRODUCT VARIANTS ----------
                var variants = new List<ProductVariant>();
                foreach (var product in masterProducts)
                {
                    decimal basePrice = 25000 + _rng.Next(0, 15) * 1000;
                    variants.Add(new ProductVariant { ProductId = product.ProductId, SizeVariant = "S", SellingPrice = basePrice });
                    variants.Add(new ProductVariant { ProductId = product.ProductId, SizeVariant = "M", SellingPrice = basePrice + 5000 });
                    if (_rng.NextDouble() > 0.3) // most products also have L
                        variants.Add(new ProductVariant { ProductId = product.ProductId, SizeVariant = "L", SellingPrice = basePrice + 10000 });
                }
                context.ProductVariants.AddRange(variants);
                context.SaveChanges();

                // ---------- 11.5 MATERIALS ----------
                SeedMaterials(context);

                // ---------- 12. BRANCH MENUS + MENU DETAILS ----------
                var branchMenus = new List<BranchMenu>();
                foreach (var branchId in allBranchIds)
                {
                    branchMenus.Add(new BranchMenu { BranchId = branchId, MenuName = $"Thực đơn {branchId}", IsActive = true });
                }
                context.BranchMenus.AddRange(branchMenus);
                context.SaveChanges();

                var menuDetails = new List<MenuDetail>();
                var allVariantIds = context.ProductVariants.Select(v => v.VariantId).ToList();
                foreach (var menu in branchMenus)
                {
                    // each branch offers ~80% of all variants
                    var offered = allVariantIds.OrderBy(_ => _rng.Next()).Take((int)(allVariantIds.Count * 0.8));
                    foreach (var variantId in offered)
                        menuDetails.Add(new MenuDetail { MenuId = menu.MenuId, VariantId = variantId });
                }
                context.MenuDetails.AddRange(menuDetails);
                context.SaveChanges();

                // ---------- 13. ORDERS + ORDER ITEMS ----------
                var orders = new List<Order>();
                var orderItems = new List<OrderItem>();
                int orderCounter = 1;
                string[] paymentMethods = { "Cash", "CreditCard", "Momo", "BankTransfer" };
                string[] paymentStatuses = { "Paid", "Unpaid" };
                string[] brewingStatuses = { "Waiting", "Brewing", "Done" };

                foreach (var branchId in allBranchIds)
                {
                    var cashiers = employeesByBranch[branchId].Where(e => e.Role == "Cashier").ToList();
                    if (!cashiers.Any()) cashiers = employeesByBranch[branchId];
                    var targetMenu = branchMenus.FirstOrDefault(m => m.BranchId == branchId);
                    var targetMenuId = targetMenu?.MenuId ?? 0;
                    var branchVariantIds = context.MenuDetails
                        .Where(md => md.MenuId == targetMenuId)
                        .Select(md => md.VariantId).ToList();
                    var variantPriceLookup = context.ProductVariants.ToDictionary(v => v.VariantId, v => v.SellingPrice);

                    int ordersForBranch = _rng.Next(10, 16);
                    for (int i = 0; i < ordersForBranch; i++)
                    {
                        var orderId = $"ORD{orderCounter:D5}";
                        orderCounter++;
                        var cashier = cashiers[_rng.Next(cashiers.Count)];
                        var itemCount = _rng.Next(1, 4);
                        var chosenVariants = branchVariantIds.OrderBy(_ => _rng.Next()).Take(itemCount).ToList();
                        decimal total = 0;

                        foreach (var variantId in chosenVariants)
                        {
                            int qty = _rng.Next(1, 3);
                            decimal price = variantPriceLookup[variantId];
                            total += price * qty;
                            orderItems.Add(new OrderItem { OrderId = orderId, VariantId = variantId, Quantity = qty, UnitPrice = price });
                        }

                        orders.Add(new Order
                        {
                            OrderId = orderId,
                            BranchId = branchId,
                            CashierId = cashier.EmployeeId,
                            CreatedAt = today.AddDays(-_rng.Next(0, 10)).AddHours(_rng.Next(6, 22)),
                            TotalAmount = total,
                            PaymentMethod = paymentMethods[_rng.Next(paymentMethods.Length)],
                            PaymentStatus = paymentStatuses[_rng.Next(paymentStatuses.Length)],
                            BrewingStatus = brewingStatuses[_rng.Next(brewingStatuses.Length)]
                        });
                    }
                }
                context.Orders.AddRange(orders);
                context.SaveChanges();
                context.OrderItems.AddRange(orderItems);
                context.SaveChanges();

                logger.LogInformation("Medium seed data generated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while generating medium seed data.");
            }
        }

        private static void SeedMaterials(CSMSAppDbContext context)
        {
            // Seed Categories first
            if (!context.MaterialCategories.Any())
            {
                context.MaterialCategories.AddRange(new List<MaterialCategory>
                {
                    new MaterialCategory { CategoryName = "Cà phê", Description = "Các loại nguyên liệu cà phê thô hoặc hạt" },
                    new MaterialCategory { CategoryName = "Sữa", Description = "Sữa đặc, sữa tươi và chế phẩm từ sữa" },
                    new MaterialCategory { CategoryName = "Gia vị", Description = "Đường, muối, bột ngọt, siro hương vị" }
                });
                context.SaveChanges();
            }

            if (context.Materials.Any(m => m.MaterialCode.StartsWith("MAT")))
            {
                context.Recipes.RemoveRange(context.Recipes);
                context.Materials.RemoveRange(context.Materials);
                context.SaveChanges();
            }

            if (!context.Materials.Any())
            {
                var materials = new List<Material>
                {
                    new Material { MaterialCode = "NL001", MaterialName = "Cà phê hạt", MaterialKind = "Thô", Category = "Cà phê", PhysicalState = "Dạng đặc", Supplier = "Trung Nguyên", UnitPrice = 150000m, StorageUnit = "kg", StockQuantity = 500m, Origin = "Buôn Ma Thuột" },
                    new Material { MaterialCode = "NL002", MaterialName = "Sữa đặc", MaterialKind = "Thành phẩm", Category = "Sữa", PhysicalState = "Dạng lỏng", Supplier = "Vinamilk", UnitPrice = 35000m, StorageUnit = "lít", StockQuantity = 200m },
                    new Material { MaterialCode = "NL003", MaterialName = "Đường cát trắng", MaterialKind = "Thô", Category = "Gia vị", PhysicalState = "Dạng đặc", Supplier = "Đường Biên Hòa", UnitPrice = 20000m, StorageUnit = "kg", StockQuantity = 150m },
                    new Material { MaterialCode = "NL004", MaterialName = "Sữa tươi ít đường", MaterialKind = "Thành phẩm", Category = "Sữa", PhysicalState = "Dạng lỏng", Supplier = "Dalat Milk", UnitPrice = 42000m, StorageUnit = "lít", StockQuantity = 300m },
                    new Material { MaterialCode = "NL005", MaterialName = "Hạt sen tươi", MaterialKind = "Thô", Category = "Gia vị", PhysicalState = "Dạng đặc", Supplier = "Sen Việt", UnitPrice = 85000m, StorageUnit = "kg", StockQuantity = 80m, Origin = "Đồng Tháp" },
                    new Material { MaterialCode = "NL006", MaterialName = "Đào ngâm", MaterialKind = "Thành phẩm", Category = "Gia vị", PhysicalState = "Dạng đặc", Supplier = "Kronos", UnitPrice = 65000m, StorageUnit = "kg", StockQuantity = 120m },
                    new Material { MaterialCode = "NL007", MaterialName = "Trà Oolong túi lọc", MaterialKind = "Thô", Category = "Cà phê", PhysicalState = "Dạng đặc", Supplier = "Phúc Long", UnitPrice = 120000m, StorageUnit = "kg", StockQuantity = 100m },
                    new Material { MaterialCode = "NL008", MaterialName = "Siro Bạc hà", MaterialKind = "Thành phẩm", Category = "Gia vị", PhysicalState = "Dạng lỏng", Supplier = "Monin", UnitPrice = 180000m, StorageUnit = "lít", StockQuantity = 60m }
                };
                context.Materials.AddRange(materials);
                context.SaveChanges();
            }

            // Seed Warehouse Receipts matching the screenshot
            if (!context.WarehouseReceipts.Any())
            {
                var caphe = context.Materials.FirstOrDefault(m => m.MaterialCode == "NL001");
                var sua = context.Materials.FirstOrDefault(m => m.MaterialCode == "NL002");

                if (caphe != null && sua != null)
                {
                    var r1 = new WarehouseReceipt
                    {
                        ReceiptCode = "PN-001",
                        ImportDate = new DateTime(2026, 6, 12, 9, 30, 0),
                        Supplier = "Trung Nguyên",
                        TotalAmount = 75000000m,
                        Status = "Đã nhập kho",
                        DelivererName = "Nguyễn Văn A",
                        DelivererPhone = "0397321999",
                        ReceiverName = "Nguyễn Hữu B",
                        CreatedBy = "Nguyễn Hữu B"
                    };
                    context.WarehouseReceipts.Add(r1);
                    context.SaveChanges();

                    context.WarehouseReceiptItems.Add(new WarehouseReceiptItem
                    {
                        ReceiptId = r1.ReceiptId,
                        MaterialId = caphe.MaterialId,
                        Quantity = 500m,
                        UnitPrice = 150000m,
                        Amount = 75000000m
                    });

                    var r2 = new WarehouseReceipt
                    {
                        ReceiptCode = "PN-002",
                        ImportDate = new DateTime(2026, 6, 12, 14, 15, 0),
                        Supplier = "Vinamilk",
                        TotalAmount = 3500000m,
                        Status = "Đã nhập kho",
                        DelivererName = "Nguyễn Thị C",
                        DelivererPhone = "0905123456",
                        ReceiverName = "Nguyễn Hữu B",
                        CreatedBy = "Nguyễn Hữu B"
                    };
                    context.WarehouseReceipts.Add(r2);
                    context.SaveChanges();

                    context.WarehouseReceiptItems.Add(new WarehouseReceiptItem
                    {
                        ReceiptId = r2.ReceiptId,
                        MaterialId = sua.MaterialId,
                        Quantity = 100m,
                        UnitPrice = 35000m,
                        Amount = 3500000m
                    });

                    var r3 = new WarehouseReceipt
                    {
                        ReceiptCode = "PN-003",
                        ImportDate = new DateTime(2026, 6, 15, 10, 0, 0),
                        Supplier = "Trung Nguyên",
                        TotalAmount = 45000000m,
                        Status = "Đã nhập kho",
                        DelivererName = "Nguyễn Văn A",
                        DelivererPhone = "0397321999",
                        ReceiverName = "Nguyễn Hữu B",
                        CreatedBy = "Nguyễn Hữu B"
                    };
                    context.WarehouseReceipts.Add(r3);
                    context.SaveChanges();

                    context.WarehouseReceiptItems.Add(new WarehouseReceiptItem
                    {
                        ReceiptId = r3.ReceiptId,
                        MaterialId = caphe.MaterialId,
                        Quantity = 300m,
                        UnitPrice = 150000m,
                        Amount = 45000000m
                    });

                    context.SaveChanges();
                }
            }

            // Cleanup legacy receipts
            var legacyReceipts = context.WarehouseReceipts.Where(r => r.DelivererName == "").ToList();
            if (legacyReceipts.Any())
            {
                foreach (var r in legacyReceipts)
                {
                    r.DelivererName = r.Supplier == "Vinamilk" ? "Nguyễn Thị C" : "Nguyễn Văn A";
                    r.DelivererPhone = r.Supplier == "Vinamilk" ? "0905123456" : "0397321999";
                    r.ReceiverName = "Nguyễn Hữu B";
                    r.CreatedBy = "Nguyễn Hữu B";
                }
                context.SaveChanges();
            }

            // Seed Branch Inventories for active branches
            if (!context.BranchInventories.Any())
            {
                var materials = context.Materials.ToList();
                var branchIds = new[] { "CB004", "CB005" };

                foreach (var branchId in branchIds)
                {
                    foreach (var m in materials)
                    {
                        decimal stock = m.MaterialCode == "NL001" ? 500m :
                                        m.MaterialCode == "NL002" ? 200m :
                                        m.MaterialCode == "NL003" ? 150m : 50m;

                        context.BranchInventories.Add(new BranchInventory
                        {
                            BranchId = branchId,
                            MaterialId = m.MaterialId,
                            StockQuantity = stock,
                            LowStockThreshold = 10m
                        });
                    }
                }
                context.SaveChanges();
            }

            if (!context.BranchSupplyRequests.Any())
            {
                var caphe = context.Materials.FirstOrDefault(m => m.MaterialCode == "NL001");
                var sua = context.Materials.FirstOrDefault(m => m.MaterialCode == "NL002");
                var duong = context.Materials.FirstOrDefault(m => m.MaterialCode == "NL003");

                if (caphe != null && sua != null && duong != null)
                {
                    // YC-004: Đã xuất kho (Đang giao hàng)
                    var y4 = new BranchSupplyRequest
                    {
                        RequestCode = "YC-004",
                        BranchId = "CB004",
                        RequestDate = new DateTime(2026, 6, 12, 10, 0, 0),
                        Status = "Đã xuất kho",
                        WarehouseNote = "Đang giao hàng",
                        ApprovedBy = "Nguyễn Hữu B",
                        ApprovedDate = new DateTime(2026, 6, 20, 14, 0, 0)
                    };
                    context.BranchSupplyRequests.Add(y4);
                    context.SaveChanges();

                    context.BranchSupplyRequestItems.AddRange(
                        new BranchSupplyRequestItem { RequestId = y4.RequestId, MaterialId = caphe.MaterialId, QuantityRequested = 100m, QuantityReleased = 100m },
                        new BranchSupplyRequestItem { RequestId = y4.RequestId, MaterialId = sua.MaterialId, QuantityRequested = 90m, QuantityReleased = 50m },
                        new BranchSupplyRequestItem { RequestId = y4.RequestId, MaterialId = duong.MaterialId, QuantityRequested = 100m, QuantityReleased = 100m }
                    );

                    // YC-005: Đã hoàn thành (Đã giao hàng)
                    var y5 = new BranchSupplyRequest
                    {
                        RequestCode = "YC-005",
                        BranchId = "CB004",
                        RequestDate = new DateTime(2026, 6, 12, 11, 0, 0),
                        Status = "Đã hoàn thành",
                        WarehouseNote = "Đã giao đủ",
                        ApprovedBy = "Nguyễn Hữu B",
                        ApprovedDate = new DateTime(2026, 6, 20, 14, 0, 0),
                        DelivererName = "Mai Xuân A",
                        DelivererPhone = "039783926",
                        ReceivedDate = new DateTime(2026, 6, 20, 16, 30, 0)
                    };
                    context.BranchSupplyRequests.Add(y5);
                    context.SaveChanges();

                    context.BranchSupplyRequestItems.AddRange(
                        new BranchSupplyRequestItem { RequestId = y5.RequestId, MaterialId = caphe.MaterialId, QuantityRequested = 100m, QuantityReleased = 100m },
                        new BranchSupplyRequestItem { RequestId = y5.RequestId, MaterialId = sua.MaterialId, QuantityRequested = 90m, QuantityReleased = 50m },
                        new BranchSupplyRequestItem { RequestId = y5.RequestId, MaterialId = duong.MaterialId, QuantityRequested = 100m, QuantityReleased = 100m }
                    );

                    // YC-006: Chờ duyệt
                    var y6 = new BranchSupplyRequest
                    {
                        RequestCode = "YC-006",
                        BranchId = "CB004",
                        RequestDate = new DateTime(2026, 6, 15, 13, 24, 0),
                        Status = "Chờ duyệt",
                        WarehouseNote = ""
                    };
                    context.BranchSupplyRequests.Add(y6);
                    context.SaveChanges();

                    context.BranchSupplyRequestItems.AddRange(
                        new BranchSupplyRequestItem { RequestId = y6.RequestId, MaterialId = caphe.MaterialId, QuantityRequested = 100m },
                        new BranchSupplyRequestItem { RequestId = y6.RequestId, MaterialId = sua.MaterialId, QuantityRequested = 90m },
                        new BranchSupplyRequestItem { RequestId = y6.RequestId, MaterialId = duong.MaterialId, QuantityRequested = 100m }
                    );

                    context.SaveChanges();
                }
            }

            SeedRecipes(context);
        }

        private static void SeedRecipes(CSMSAppDbContext context)
        {
            if (context.Recipes.Any()) return;

            var materials = context.Materials.ToDictionary(m => m.MaterialCode, m => m.MaterialId);
            if (!materials.ContainsKey("NL001")) return;

            int capheId = materials["NL001"];      // Cà phê hạt (g)
            int suaDacId = materials["NL002"];     // Sữa đặc (ml)
            int duongId = materials["NL003"];      // Đường cát trắng (g)
            int suaTuoiId = materials["NL004"];    // Sữa tươi ít đường (ml)
            int senId = materials.GetValueOrDefault("NL005");        // Hạt sen tươi (g)
            int daoId = materials.GetValueOrDefault("NL006");        // Đào ngâm (g)
            int traOolongId = materials.GetValueOrDefault("NL007");  // Trà Oolong túi lọc (g)
            int siroBacHaId = materials.GetValueOrDefault("NL008");  // Siro Bạc hà (ml)

            var products = context.MasterProducts
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductCategory)
                .ToList();

            var newRecipes = new List<Recipe>();

            foreach (var product in products)
            {
                if (product.ProductCategory != null && product.ProductCategory.CategoryName == "Bánh ngọt")
                {
                    continue; // Bánh ngọt không dùng công thức pha chế
                }

                foreach (var variant in product.ProductVariants)
                {
                    string size = variant.SizeVariant?.ToUpper() ?? "M";
                    decimal sizeMultiplier = size switch
                    {
                        "S" => 0.8m,
                        "L" => 1.25m,
                        _ => 1.0m
                    };

                    switch (product.ProductName)
                    {
                        case "Cà phê đen":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = capheId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            break;

                        case "Cà phê sữa":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = capheId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = suaDacId, Quantity = Math.Round(35m * sizeMultiplier, 1) });
                            break;

                        case "Bạc xỉu":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = capheId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = suaDacId, Quantity = Math.Round(40m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = suaTuoiId, Quantity = Math.Round(80m * sizeMultiplier, 1) });
                            break;

                        case "Espresso":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = capheId, Quantity = Math.Round(18m * sizeMultiplier, 1) });
                            break;

                        case "Trà đào cam sả":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = traOolongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = daoId, Quantity = Math.Round(40m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            break;

                        case "Trà oolong":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = traOolongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            break;

                        case "Trà sen vàng":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = traOolongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = senId, Quantity = Math.Round(40m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            break;

                        case "Trà vải":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = traOolongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            break;

                        case "Frappuccino cà phê":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = capheId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = suaTuoiId, Quantity = Math.Round(80m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = suaDacId, Quantity = Math.Round(30m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            break;

                        case "Matcha đá xay":
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = suaTuoiId, Quantity = Math.Round(100m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = siroBacHaId, Quantity = Math.Round(30m * sizeMultiplier, 1) });
                            newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            break;

                        default:
                            if (product.ProductCategory?.CategoryName == "Cà phê")
                            {
                                newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = capheId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                                newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            }
                            else if (product.ProductCategory?.CategoryName == "Trà")
                            {
                                newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = traOolongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                                newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(15m * sizeMultiplier, 1) });
                            }
                            else
                            {
                                newRecipes.Add(new Recipe { VariantId = variant.VariantId, MaterialId = duongId, Quantity = Math.Round(20m * sizeMultiplier, 1) });
                            }
                            break;
                    }
                }
            }

            if (newRecipes.Any())
            {
                context.Recipes.AddRange(newRecipes);
                context.SaveChanges();
            }

            // ---------- 14. BRANCH SETTINGS ----------
            if (!context.BranchSettings.Any())
            {
                var branchSettings = new List<BranchSetting>();
                var branches = context.Branches.ToList();
                foreach (var b in branches)
                {
                    branchSettings.Add(new BranchSetting
                    {
                        BranchId = b.BranchId,
                        BranchDisplayName = b.BranchName ?? "CSMS Coffee",
                        ContactPhone = b.PhoneNumber ?? "0988888888",
                        Address = b.Address ?? "",
                        OpeningHours = "06:30 - 22:30",
                        AutoPrintReceipt = true,
                        EnableSoundNotification = true,
                        BankCode = "MBBank",
                        AccountNumber = "0333333333",
                        AccountName = "CSMS CAFE",
                        TransferPrefix = "CSMS",
                        AutoConfirmOrder = true,
                        IsSePayActive = true
                    });
                }
                context.BranchSettings.AddRange(branchSettings);
                context.SaveChanges();
            }
        }

        public static void EnsureTablesCreated(CSMSAppDbContext context)
        {
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'branch_settings')
                    BEGIN
                        CREATE TABLE [branch_settings] (
                            [setting_id] int IDENTITY(1,1) NOT NULL,
                            [branch_id] nvarchar(50) NOT NULL,
                            [branch_display_name] nvarchar(100) NOT NULL DEFAULT 'CSMS Coffee',
                            [contact_phone] nvarchar(20) NOT NULL DEFAULT '0988888888',
                            [address] nvarchar(255) NOT NULL DEFAULT '',
                            [opening_hours] nvarchar(100) NOT NULL DEFAULT '06:30 - 22:30',
                            [auto_print_receipt] bit NOT NULL DEFAULT 1,
                            [enable_sound_notification] bit NOT NULL DEFAULT 1,
                            [bank_code] nvarchar(20) NOT NULL DEFAULT 'MBBank',
                            [account_number] nvarchar(50) NOT NULL DEFAULT '0333333333',
                            [account_name] nvarchar(100) NOT NULL DEFAULT 'CSMS CAFE',
                            [sepay_api_key] nvarchar(200) NULL DEFAULT '',
                            [webhook_secret_token] nvarchar(200) NULL DEFAULT '',
                            [transfer_prefix] nvarchar(50) NOT NULL DEFAULT 'CSMS',
                            [auto_confirm_order] bit NOT NULL DEFAULT 1,
                            [is_sepay_active] bit NOT NULL DEFAULT 1,
                            [updated_at] datetime2 NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT [PK_branch_settings] PRIMARY KEY ([setting_id])
                        );
                    END

                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'payment_method' AND max_length < 200)
                    BEGIN
                        ALTER TABLE [orders] ALTER COLUMN [payment_method] nvarchar(200) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payments')
                    BEGIN
                        CREATE TABLE [payments] (
                            [payment_id] nvarchar(50) NOT NULL,
                            [order_id] nvarchar(50) NOT NULL,
                            [branch_id] nvarchar(20) NULL,
                            [cashier_id] int NULL,
                            [payment_type] nvarchar(20) NOT NULL DEFAULT 'Payment',
                            [payment_method] nvarchar(50) NOT NULL,
                            [amount] decimal(18,2) NOT NULL,
                            [status] nvarchar(20) NOT NULL DEFAULT 'Success',
                            [transaction_code] nvarchar(100) NULL,
                            [customer_cash] decimal(18,2) NULL,
                            [change_amount] decimal(18,2) NULL,
                            [notes] nvarchar(255) NULL,
                            [created_at] datetime2 NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT [PK_payments] PRIMARY KEY ([payment_id]),
                            CONSTRAINT [FK_payments_orders] FOREIGN KEY ([order_id]) REFERENCES [orders]([order_id]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_payments_branches] FOREIGN KEY ([branch_id]) REFERENCES [branches]([branch_id]),
                            CONSTRAINT [FK_payments_employees] FOREIGN KEY ([cashier_id]) REFERENCES [employees]([employee_id])
                        );
                        CREATE INDEX [IX_payments_order_id] ON [payments]([order_id]);
                        CREATE INDEX [IX_payments_created_at] ON [payments]([created_at]);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'table_number')
                    BEGIN
                        ALTER TABLE [orders] ADD [table_number] nvarchar(50) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'customer_name')
                    BEGIN
                        ALTER TABLE [orders] ADD [customer_name] nvarchar(100) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'subtotal_amount')
                    BEGIN
                        ALTER TABLE [orders] ADD [subtotal_amount] decimal(18,2) NOT NULL DEFAULT 0;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'discount_amount')
                    BEGIN
                        ALTER TABLE [orders] ADD [discount_amount] decimal(18,2) NOT NULL DEFAULT 0;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'trade_discount_amount')
                    BEGIN
                        ALTER TABLE [orders] ADD [trade_discount_amount] decimal(18,2) NOT NULL DEFAULT 0;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'order_notes')
                    BEGIN
                        ALTER TABLE [orders] ADD [order_notes] nvarchar(255) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('cash_handovers') AND name = 'cash_refund_amount')
                    BEGIN
                        ALTER TABLE [cash_handovers] ADD [cash_refund_amount] decimal(18,2) NOT NULL DEFAULT 0;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('cash_handovers') AND name = 'handover_type')
                    BEGIN
                        ALTER TABLE [cash_handovers] ADD [handover_type] nvarchar(50) NOT NULL DEFAULT 'Normal';
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('cash_handovers') AND name = 'emergency_reason')
                    BEGIN
                        ALTER TABLE [cash_handovers] ADD [emergency_reason] nvarchar(255) NULL;
                    END

                    -- Cập nhật Ca 3 kết thúc lúc 23:59:59 (24h) và loại bỏ Ca đêm
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'fixed_shifts')
                    BEGIN
                        UPDATE [fixed_shifts] SET [start_time] = '18:00:00', [end_time] = '23:59:59' WHERE [shift_name] = 'Ca 3';

                        IF EXISTS (SELECT * FROM [fixed_shifts] WHERE [shift_name] LIKE N'%đêm%' OR [shift_name] LIKE '%Night%' OR [start_time] >= '22:00:00')
                        BEGIN
                            DECLARE @NightShiftId int;
                            DECLARE @Ca3ShiftId int;
                            SELECT TOP 1 @NightShiftId = [shift_id] FROM [fixed_shifts] WHERE [shift_name] LIKE N'%đêm%' OR [shift_name] LIKE '%Night%' OR [start_time] >= '22:00:00';
                            SELECT TOP 1 @Ca3ShiftId = [shift_id] FROM [fixed_shifts] WHERE [shift_name] = 'Ca 3';

                            IF @NightShiftId IS NOT NULL AND @Ca3ShiftId IS NOT NULL
                            BEGIN
                                UPDATE [weekly_roster_grids] SET [shift_id] = @Ca3ShiftId WHERE [shift_id] = @NightShiftId;
                                UPDATE [cash_handovers] SET [shift_id] = @Ca3ShiftId WHERE [shift_id] = @NightShiftId;
                                DELETE FROM [fixed_shifts] WHERE [shift_id] = @NightShiftId;
                            END
                        END
                    END

                    -- Dọn dẹp các bản ghi trùng lặp trong weekly_roster_grids nếu có
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'weekly_roster_grids')
                    BEGIN
                        ;WITH CTE AS (
                            SELECT [roster_id],
                                   ROW_NUMBER() OVER (PARTITION BY [employee_id], [shift_id], CAST([assignment_date] AS DATE) ORDER BY [roster_id]) as rn
                            FROM [weekly_roster_grids]
                        )
                        DELETE FROM CTE WHERE rn > 1;
                    END

                    -- Đảm bảo FK giữa payments và orders là NO ACTION (không delete cascade, không đổi sang null)
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_payments_orders' AND delete_referential_action_desc = 'CASCADE')
                    BEGIN
                        ALTER TABLE [payments] DROP CONSTRAINT [FK_payments_orders];
                        ALTER TABLE [payments] ADD CONSTRAINT [FK_payments_orders] FOREIGN KEY ([order_id]) REFERENCES [orders]([order_id]) ON DELETE NO ACTION;
                    END

                    -- Đảm bảo FK giữa orders và employees là NO ACTION (không delete cascade, không đổi sang null)
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_orders_employees_cashier_id' AND delete_referential_action_desc = 'CASCADE')
                    BEGIN
                        ALTER TABLE [orders] DROP CONSTRAINT [FK_orders_employees_cashier_id];
                        ALTER TABLE [orders] ADD CONSTRAINT [FK_orders_employees_cashier_id] FOREIGN KEY ([cashier_id]) REFERENCES [employees]([employee_id]) ON DELETE NO ACTION;
                    END

                    -- Đảm bảo bảng vouchers tồn tại
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'vouchers')
                    BEGIN
                        CREATE TABLE [vouchers] (
                            [voucher_id] INT IDENTITY(1,1) NOT NULL,
                            [voucher_code] NVARCHAR(50) NOT NULL,
                            [branch_id] NVARCHAR(20) NULL,
                            [discount_percent] DECIMAL(5, 2) NOT NULL,
                            [quantity] INT NOT NULL,
                            [used_count] INT NOT NULL DEFAULT 0,
                            [start_date] DATETIME2 NOT NULL,
                            [end_date] DATETIME2 NOT NULL,
                            [description] NVARCHAR(500) NULL,
                            [is_active] BIT NOT NULL DEFAULT 1,
                            [created_by] INT NULL,
                            [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT [PK_vouchers] PRIMARY KEY CLUSTERED ([voucher_id] ASC),
                            CONSTRAINT [FK_vouchers_branches] FOREIGN KEY ([branch_id]) REFERENCES [branches] ([branch_id]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_vouchers_employees] FOREIGN KEY ([created_by]) REFERENCES [employees] ([employee_id]) ON DELETE NO ACTION
                        );
                        CREATE NONCLUSTERED INDEX [IX_vouchers_voucher_code] ON [vouchers] ([voucher_code] ASC);
                        CREATE NONCLUSTERED INDEX [IX_vouchers_branch_id] ON [vouchers] ([branch_id] ASC);
                    END

                    -- Đảm bảo cột voucher_id trên orders
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'voucher_id')
                    BEGIN
                        ALTER TABLE [orders] ADD [voucher_id] INT NULL;
                        ALTER TABLE [orders] ADD CONSTRAINT [FK_orders_vouchers] FOREIGN KEY ([voucher_id]) REFERENCES [vouchers] ([voucher_id]) ON DELETE NO ACTION;
                    END

                    -- Đảm bảo cột voucher_code trên orders
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('orders') AND name = 'voucher_code')
                    BEGIN
                        ALTER TABLE [orders] ADD [voucher_code] NVARCHAR(50) NULL;
                    END

                    -- Đảm bảo các cột is_available, updated_by, updated_at trên menu_details
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('menu_details') AND name = 'is_available')
                    BEGIN
                        ALTER TABLE [menu_details] ADD [is_available] BIT NOT NULL DEFAULT 1;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('menu_details') AND name = 'updated_by')
                    BEGIN
                        ALTER TABLE [menu_details] ADD [updated_by] INT NULL;
                        ALTER TABLE [menu_details] ADD CONSTRAINT [FK_menu_details_employees_updated_by] FOREIGN KEY ([updated_by]) REFERENCES [employees]([employee_id]) ON DELETE NO ACTION;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('menu_details') AND name = 'updated_at')
                    BEGIN
                        ALTER TABLE [menu_details] ADD [updated_at] DATETIME2 NULL;
                    END

                    -- Đảm bảo các cột giao nhận, đồng kiểm trên branch_supply_requests
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'delivery_provider')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [delivery_provider] NVARCHAR(100) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'expected_delivery_date')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [expected_delivery_date] DATETIME2 NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'receiver_name')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [receiver_name] NVARCHAR(100) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'receiver_phone')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [receiver_phone] NVARCHAR(20) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'request_note')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [request_note] NVARCHAR(500) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'inspected_by')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [inspected_by] NVARCHAR(100) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'inspected_at')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [inspected_at] DATETIME2 NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_requests') AND name = 'inspection_status')
                    BEGIN
                        ALTER TABLE [branch_supply_requests] ADD [inspection_status] NVARCHAR(50) NULL;
                    END

                    -- Đảm bảo các cột đồng kiểm và báo lỗi trên branch_supply_request_items
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_request_items') AND name = 'quantity_received')
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ADD [quantity_received] DECIMAL(18,2) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_request_items') AND name = 'quantity_accepted')
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ADD [quantity_accepted] DECIMAL(18,2) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_request_items') AND name = 'quantity_defective')
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ADD [quantity_defective] DECIMAL(18,2) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_request_items') AND name = 'defect_type')
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ADD [defect_type] NVARCHAR(100) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_request_items') AND name = 'defect_note')
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ADD [defect_note] NVARCHAR(500) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('branch_supply_request_items') AND name = 'defect_image_url')
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ADD [defect_image_url] NVARCHAR(MAX) NULL;
                    END
                    ELSE
                    BEGIN
                        ALTER TABLE [branch_supply_request_items] ALTER COLUMN [defect_image_url] NVARCHAR(MAX) NULL;
                    END");
            }
            catch { }
        }
    }
}