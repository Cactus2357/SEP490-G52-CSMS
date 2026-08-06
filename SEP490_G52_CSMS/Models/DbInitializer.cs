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

            try
            {
                // Skip if already seeded beyond the base 3 branches or if CB004/CB005 already exist
                if (context.Branches.Count() > 3 || context.Branches.Any(b => b.BranchId == "CB004" || b.BranchId == "CB005")) return;

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
                    new FixedShift { ShiftName = "Ca 1", StartTime = new TimeSpan(6,0,0), EndTime = new TimeSpan(12,0,0) },
                    new FixedShift { ShiftName = "Ca 2", StartTime = new TimeSpan(12,0,0), EndTime = new TimeSpan(18,0,0) },
                    new FixedShift { ShiftName = "Ca 3", StartTime = new TimeSpan(18,0,0), EndTime = new TimeSpan(22,0,0) },
                    new FixedShift { ShiftName = "Ca đêm", StartTime = new TimeSpan(22,0,0), EndTime = new TimeSpan(6,0,0) }
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
                            newManagerLinks.Add(new BranchManager { BranchId = branchId, ManagerId = mgr.EmployeeId, AppointedDate = DateTime.Now.AddMonths(-_rng.Next(1, 24)) });
                    }
                }
                context.BranchManagers.AddRange(newManagerLinks);
                context.SaveChanges();

                // ---------- 5. WEEKLY ROSTER GRID + ATTENDANCE LOGS ----------
                var today = DateTime.Today;
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

                // ---------- 6. CASH HANDOVERS ----------
                var handovers = new List<CashHandover>();
                foreach (var branchId in allBranchIds)
                {
                    var staff = employeesByBranch[branchId].Where(e => e.Role == "Cashier").ToList();
                    if (staff.Count < 2) staff = employeesByBranch[branchId].Take(2).ToList();
                    if (staff.Count < 2) continue;

                    for (int day = 0; day < 5; day++)
                    {
                        var outgoing = staff[_rng.Next(staff.Count)];
                        var incoming = staff.First(e => e.EmployeeId != outgoing.EmployeeId);
                        decimal initial = 500000m;
                        decimal totalRevenue = _rng.Next(1000000, 5000000);
                        decimal cashlessRevenue = Math.Round(totalRevenue * 0.6m);
                        decimal cashRevenue = totalRevenue - cashlessRevenue;
                        decimal theoretical = initial + cashRevenue;
                        decimal actual = theoretical + _rng.Next(-20000, 20000);

                        handovers.Add(new CashHandover
                        {
                            BranchId = branchId,
                            HandoverDate = startDate.AddDays(day),
                            ShiftId = shifts[_rng.Next(shifts.Count)].ShiftId,
                            OutgoingCashierId = outgoing.EmployeeId,
                            IncomingCashierId = incoming.EmployeeId,
                            InitialCash = initial,
                            MachineCashRevenue = cashlessRevenue,
                            TheoreticalCash = theoretical,
                            ActualCash = actual,
                            IsPasswordConfirmed = true
                        });
                    }
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
    }
}