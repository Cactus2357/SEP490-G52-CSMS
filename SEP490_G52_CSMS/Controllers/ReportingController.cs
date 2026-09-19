using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using System.Text;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "RManager,BranchManager")]
    public class ReportingController : Controller
    {
        private readonly CSMSAppDbContext _context;

        public ReportingController(CSMSAppDbContext context)
        {
            _context = context;
        }

        // GET: /Reporting/Revenue
        public async Task<IActionResult> Revenue(string? branchId, string? dateRange)
        {
            var vm = new RevenueReportViewModel();
            var branches = await _context.Branches.ToListAsync();

            // Populate Branch Dropdown
            vm.BranchList = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả chi nhánh" }
            };
            vm.BranchList.AddRange(branches.Select(b => new SelectListItem
            {
                Value = b.BranchId,
                Text = b.BranchName
            }));

            // Default selection
            if (string.IsNullOrEmpty(branchId))
            {
                branchId = "all";
            }
            vm.SelectedBranchId = branchId;

            // Date Range parsing (Format: yyyy-MM)
            DateTime selectedMonth;
            if (string.IsNullOrEmpty(dateRange))
            {
                selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }
            else
            {
                if (!DateTime.TryParse(dateRange + "-01", out selectedMonth))
                {
                    selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
            }
            vm.SelectedMonth = selectedMonth.ToString("yyyy-MM");

            int daysInMonth = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);
            var startDate = selectedMonth;
            var endDate = selectedMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            // Fetch Orders in selected month with items & product details
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv!.MasterProduct)
                            .ThenInclude(mp => mp!.ProductCategory)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            if (branchId != "all")
            {
                ordersQuery = ordersQuery.Where(o => o.BranchId == branchId);
            }

            var ordersInMonth = await ordersQuery.ToListAsync();

            // 1. Key Metrics Summary
            vm.TotalRevenue = ordersInMonth.Sum(o => o.TotalAmount);
            vm.TotalOrders = ordersInMonth.Count;
            vm.TotalQuantitySold = ordersInMonth.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);

            // 2. Line Chart Data (Daily Revenue)
            for (int i = 1; i <= daysInMonth; i++)
            {
                vm.ChartLabels.Add($"{i:D2}/{selectedMonth.Month:D2}");
                var dailyTotal = ordersInMonth
                    .Where(o => o.CreatedAt.Day == i)
                    .Sum(o => o.TotalAmount);
                vm.ChartData.Add(dailyTotal);
            }

            // 3. Table Data Grid (Per Branch Overview)
            var allMonthOrders = await _context.Orders
                .Include(o => o.Payments)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync();

            foreach (var b in branches)
            {
                var bOrders = allMonthOrders.Where(o => o.BranchId == b.BranchId).ToList();
                var cashRevenue = bOrders.Sum(o => o.CashAmount > 0 ? o.CashAmount : (o.PaymentMethod == "Cash" ? o.TotalAmount : 0));
                var transferRevenue = bOrders.Sum(o => o.BankAmount > 0 ? o.BankAmount : (o.PaymentMethod != "Cash" ? o.TotalAmount : 0));

                vm.TableData.Add(new RevenueGridRow
                {
                    BranchName = b.BranchName,
                    CashRevenue = cashRevenue,
                    TransferRevenue = transferRevenue,
                    OrderCount = bOrders.Count
                });
            }

            // 4. Category Sales & Revenue Breakdown (Doughnut Chart)
            var categoryGrouped = ordersInMonth
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductVariant?.MasterProduct?.ProductCategory?.CategoryName ?? "Khác")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity),
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            foreach (var cat in categoryGrouped)
            {
                vm.CategoryLabels.Add(cat.CategoryName);
                vm.CategoryRevenueData.Add(cat.Revenue);
                vm.CategoryQuantityData.Add(cat.Quantity);
            }

            // 5. Product Sales Quantity (Bar Chart)
            var productGrouped = ordersInMonth
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductVariant?.MasterProduct?.ProductName ?? "Sản phẩm khác")
                .Select(g => new
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList();

            foreach (var prod in productGrouped)
            {
                vm.ProductSalesLabels.Add(prod.ProductName);
                vm.ProductSalesQuantityData.Add(prod.Quantity);
            }

            // 6. Top Products per Branch
            foreach (var b in branches)
            {
                var bItems = ordersInMonth
                    .Where(o => o.BranchId == b.BranchId)
                    .SelectMany(o => o.OrderItems)
                    .ToList();

                var topProd = bItems
                    .GroupBy(oi => oi.ProductVariant?.MasterProduct?.ProductName ?? "Sản phẩm")
                    .Select(g => new BranchTopProductRow
                    {
                        BranchName = b.BranchName,
                        ProductName = g.Key,
                        QuantitySold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity)
                    })
                    .OrderByDescending(x => x.QuantitySold)
                    .FirstOrDefault();

                if (topProd != null)
                {
                    vm.BranchTopProducts.Add(topProd);
                }
            }

            // 7. Material Supply Import / Export Statistics
            var receiptItems = await _context.WarehouseReceiptItems
                .Include(r => r.WarehouseReceipt)
                .Include(r => r.Material)
                .Where(r => r.WarehouseReceipt != null && r.WarehouseReceipt.ImportDate >= startDate && r.WarehouseReceipt.ImportDate <= endDate)
                .ToListAsync();

            var supplyRequestItems = await _context.BranchSupplyRequestItems
                .Include(s => s.BranchSupplyRequest)
                .Include(s => s.Material)
                .Where(s => s.BranchSupplyRequest != null && s.BranchSupplyRequest.RequestDate >= startDate && s.BranchSupplyRequest.RequestDate <= endDate)
                .ToListAsync();

            vm.TotalMaterialImportsAmount = receiptItems.Sum(r => r.Amount);

            var topMaterials = receiptItems
                .Select(r => r.Material?.MaterialName ?? "Khác")
                .Union(supplyRequestItems.Select(s => s.Material?.MaterialName ?? "Khác"))
                .Distinct()
                .Take(7)
                .ToList();

            foreach (var matName in topMaterials)
            {
                vm.MaterialStatLabels.Add(matName);
                vm.WarehouseImportData.Add(receiptItems.Where(r => r.Material?.MaterialName == matName).Sum(r => r.Quantity));
                vm.WarehouseExportData.Add(supplyRequestItems.Where(s => s.Material?.MaterialName == matName && s.BranchSupplyRequest?.Status == "Đã hoàn thành").Sum(s => s.QuantityReleased ?? s.QuantityRequested));
                vm.BranchImportRequestData.Add(supplyRequestItems.Where(s => s.Material?.MaterialName == matName).Sum(s => s.QuantityRequested));
            }

            return View(vm);
        }

        // GET: /Reporting/Export
        public async Task<IActionResult> Export(string? branchId)
        {
            var branches = await _context.Branches.ToListAsync();
            var vm = new ExportReportViewModel
            {
                BranchList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "all", Text = "Tất cả chi nhánh" }
                }
            };
            vm.BranchList.AddRange(branches.Select(b => new SelectListItem
            {
                Value = b.BranchId,
                Text = b.BranchName
            }));
            vm.SelectedBranchId = string.IsNullOrEmpty(branchId) ? "all" : branchId;

            return View(vm);
        }

        // GET: /Reporting/ExportRevenue
        [HttpGet]
        public async Task<IActionResult> ExportRevenue(string? branchId, DateTime? fromDate, DateTime? toDate)
        {
            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);

            var ordersQuery = _context.Orders
                .Include(o => o.Branch)
                .Include(o => o.Cashier)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv!.MasterProduct)
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end);

            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                ordersQuery = ordersQuery.Where(o => o.BranchId == branchId);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Mã đơn hàng,Chi nhánh,Thời gian tạo,Thu ngân,Khách hàng,Vị trí / Bàn,Tạm tính (VNĐ),Giảm giá (VNĐ),Chiết khấu (VNĐ),Tổng tiền (VNĐ),Hình thức thanh toán,Trạng thái thanh toán,Trạng thái pha chế,Chi tiết món,Ghi chú");

            foreach (var order in orders)
            {
                var branchName = order.Branch?.BranchName ?? order.BranchId ?? "";
                var timeStr = order.CreatedAt.ToVietnamTimeString("dd/MM/yyyy HH:mm:ss");
                var cashierName = order.Cashier?.FullName ?? order.Cashier?.Username ?? (order.CashierId > 0 ? order.CashierId.ToString() : "");
                var itemsSummary = string.Join("; ", order.OrderItems.Select(oi =>
                {
                    var prodName = oi.ProductVariant?.MasterProduct?.ProductName ?? "Sản phẩm";
                    var size = !string.IsNullOrEmpty(oi.ProductVariant?.SizeVariant) ? $" ({oi.ProductVariant.SizeVariant})" : "";
                    return $"{prodName}{size} x{oi.Quantity}";
                }));

                var line = $"{EscapeCsv(order.OrderId)},{EscapeCsv(branchName)},{EscapeCsv(timeStr)},{EscapeCsv(cashierName)},{EscapeCsv(order.CustomerName)},{EscapeCsv(order.TableNumber)},{order.SubtotalAmount},{order.DiscountAmount},{order.TradeDiscountAmount},{order.TotalAmount},{EscapeCsv(order.PaymentMethod)},{EscapeCsv(order.PaymentStatus)},{EscapeCsv(order.BrewingStatus)},{EscapeCsv(itemsSummary)},{EscapeCsv(order.OrderNotes)}";
                csvBuilder.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = !string.IsNullOrEmpty(branchId) && branchId != "all"
                ? $"BaoCaoDoanhThu_{branchId}_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv"
                : $"BaoCaoDoanhThu_TatCa_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Reporting/ExportTimesheet
        [HttpGet]
        public async Task<IActionResult> ExportTimesheet(string? branchId, DateTime? fromDate, DateTime? toDate)
        {
            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date;

            var rosterQuery = _context.WeeklyRosterGrids
                .Include(r => r.Branch)
                .Include(r => r.Employee)
                .Include(r => r.FixedShift)
                .Include(r => r.AttendanceLogs)
                .Where(r => r.AssignmentDate >= start && r.AssignmentDate <= end);

            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                rosterQuery = rosterQuery.Where(r => r.BranchId == branchId);
            }

            var rosters = await rosterQuery
                .OrderByDescending(r => r.AssignmentDate)
                .ThenBy(r => r.ShiftId)
                .ThenBy(r => r.EmployeeId)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Chi nhánh,Ngày làm việc,Tên ca,Khung giờ ca,Mã NV,Họ và tên,Tài khoản,Giờ Check-in,Xác thực khuôn mặt Check-in,Độ tin cậy Check-in,Trạng thái Check-in,Giờ Check-out,Xác thực khuôn mặt Check-out,Độ tin cậy Check-out,Trạng thái Check-out,Trạng thái chung,Ghi chú");

            foreach (var r in rosters)
            {
                var branchName = r.Branch?.BranchName ?? r.BranchId;
                var dateStr = r.AssignmentDate.ToString("dd/MM/yyyy");
                var shiftName = r.FixedShift?.ShiftName ?? "";
                var shiftTime = r.FixedShift != null ? $"{r.FixedShift.StartTime:hh\\:mm} - {r.FixedShift.EndTime:hh\\:mm}" : "";
                var empId = r.EmployeeId;
                var empName = r.Employee?.FullName ?? "";
                var empUsername = r.Employee?.Username ?? "";

                var logs = r.AttendanceLogs.ToList();
                if (!logs.Any())
                {
                    var line = $"{EscapeCsv(branchName)},{EscapeCsv(dateStr)},{EscapeCsv(shiftName)},{EscapeCsv(shiftTime)},{empId},{EscapeCsv(empName)},{EscapeCsv(empUsername)},\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Chưa chấm công\",{EscapeCsv(string.Empty)}";
                    csvBuilder.AppendLine(line);
                }
                else
                {
                    foreach (var att in logs)
                    {
                        var inTime = att.CheckInTime.ToVietnamTimeString("HH:mm:ss dd/MM/yyyy");
                        var outTime = att.CheckOutTime.ToVietnamTimeString("HH:mm:ss dd/MM/yyyy");
                        var inFace = att.IsFaceCheckInValid ? "Hợp lệ" : "Không hợp lệ";
                        var outFace = att.IsFaceCheckOutValid ? "Hợp lệ" : "Không hợp lệ";
                        var inConf = att.CheckInConfidence.HasValue ? $"{att.CheckInConfidence.Value:P0}" : "";
                        var outConf = att.CheckOutConfidence.HasValue ? $"{att.CheckOutConfidence.Value:P0}" : "";

                        var line = $"{EscapeCsv(branchName)},{EscapeCsv(dateStr)},{EscapeCsv(shiftName)},{EscapeCsv(shiftTime)},{empId},{EscapeCsv(empName)},{EscapeCsv(empUsername)},{EscapeCsv(inTime)},{EscapeCsv(inFace)},{EscapeCsv(inConf)},{EscapeCsv(att.CheckInStatus)},{EscapeCsv(outTime)},{EscapeCsv(outFace)},{EscapeCsv(outConf)},{EscapeCsv(att.CheckOutStatus)},{EscapeCsv(att.OverallStatus)},{EscapeCsv(att.Notes)}";
                        csvBuilder.AppendLine(line);
                    }
                }
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = !string.IsNullOrEmpty(branchId) && branchId != "all"
                ? $"BangCongLichLamViec_{branchId}_{start:yyyyMMdd}_{end:yyyyMMdd}.csv"
                : $"BangCongLichLamViec_TatCa_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Reporting/ExportMenu
        [HttpGet]
        public async Task<IActionResult> ExportMenu(string? branchId)
        {
            var branchesQuery = _context.Branches.AsQueryable();
            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                branchesQuery = branchesQuery.Where(b => b.BranchId == branchId);
            }
            var branches = await branchesQuery.OrderBy(b => b.BranchId).ToListAsync();
            var branchIds = branches.Select(b => b.BranchId).ToList();

            var branchMenus = await _context.BranchMenus
                .Include(m => m.MenuDetails)
                .Where(m => branchIds.Contains(m.BranchId!))
                .ToListAsync();

            var allMasterProducts = await _context.MasterProducts
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductVariants)
                .OrderBy(p => p.CategoryId)
                .ThenBy(p => p.ProductName)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Chi nhánh,Tên thực đơn,Danh mục,Mã sản phẩm,Tên sản phẩm,Mã biến thể,Kích cỡ / Phân loại,Đơn giá (VNĐ),Tình trạng phục vụ,Trạng thái sản phẩm");

            foreach (var b in branches)
            {
                var activeMenu = branchMenus.FirstOrDefault(m => m.BranchId == b.BranchId && m.IsActive)
                    ?? branchMenus.FirstOrDefault(m => m.BranchId == b.BranchId);

                foreach (var master in allMasterProducts)
                {
                    if (master.ProductVariants == null || !master.ProductVariants.Any())
                    {
                        var line = $"{EscapeCsv(b.BranchName)},{EscapeCsv(activeMenu?.MenuName ?? "Thực đơn chính")},{EscapeCsv(master.ProductCategory?.CategoryName ?? "Khác")},{master.ProductId},{EscapeCsv(master.ProductName)},\"\",\"\",0,\"Không có size\",{EscapeCsv(master.Status)}";
                        csvBuilder.AppendLine(line);
                        continue;
                    }

                    foreach (var pv in master.ProductVariants.OrderBy(v => v.SellingPrice))
                    {
                        var detail = activeMenu?.MenuDetails?.FirstOrDefault(d => d.VariantId == pv.VariantId);
                        var isAvailable = detail == null || detail.IsAvailable;
                        var servingStatus = isAvailable ? "Đang phục vụ" : "Tạm ngưng";

                        var line = $"{EscapeCsv(b.BranchName)},{EscapeCsv(activeMenu?.MenuName ?? "Thực đơn chính")},{EscapeCsv(master.ProductCategory?.CategoryName ?? "Khác")},{master.ProductId},{EscapeCsv(master.ProductName)},{pv.VariantId},{EscapeCsv(pv.SizeVariant ?? "Mặc định")},{pv.SellingPrice},{EscapeCsv(servingStatus)},{EscapeCsv(master.Status)}";
                        csvBuilder.AppendLine(line);
                    }
                }
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = !string.IsNullOrEmpty(branchId) && branchId != "all"
                ? $"ThucDon_{branchId}_{DateTime.Today:yyyyMMdd}.csv"
                : $"ThucDon_TatCa_{DateTime.Today:yyyyMMdd}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Reporting/Warehouse
        public async Task<IActionResult> Warehouse(string? branchId, string? reportScope, DateTime? fromDate, DateTime? toDate, string? status)
        {
            var vm = new WarehouseReportViewModel();
            var branches = await _context.Branches.ToListAsync();

            vm.BranchList = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả chi nhánh" }
            };
            vm.BranchList.AddRange(branches.Select(b => new SelectListItem
            {
                Value = b.BranchId,
                Text = b.BranchName
            }));

            vm.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả trạng thái" },
                new SelectListItem { Value = "Chờ duyệt", Text = "Chờ duyệt" },
                new SelectListItem { Value = "Đã xuất kho", Text = "Đã xuất kho" },
                new SelectListItem { Value = "Đã hoàn thành", Text = "Đã hoàn thành" },
                new SelectListItem { Value = "Từ chối", Text = "Từ chối" },
                new SelectListItem { Value = "Đã hủy", Text = "Đã hủy" }
            };

            vm.SelectedBranchId = string.IsNullOrEmpty(branchId) ? "all" : branchId;
            vm.SelectedScope = string.IsNullOrEmpty(reportScope) ? "all" : reportScope;
            vm.SelectedStatus = string.IsNullOrEmpty(status) ? "all" : status;

            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);
            vm.FromDate = start;
            vm.ToDate = end.AddDays(-1);

            // 1. Warehouse Receipts (Central Warehouse Imports)
            var receiptsQuery = _context.WarehouseReceipts
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Where(r => r.ImportDate >= start && r.ImportDate < end);

            var receipts = await receiptsQuery.OrderByDescending(r => r.ImportDate).ToListAsync();

            // 2. Branch Supply Requests (Warehouse Exports / Branch Imports)
            var supplyQuery = _context.BranchSupplyRequests
                .Include(s => s.Branch)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Material)
                .Where(s => s.RequestDate >= start && s.RequestDate < end);

            if (vm.SelectedBranchId != "all")
            {
                supplyQuery = supplyQuery.Where(s => s.BranchId == vm.SelectedBranchId);
            }
            if (vm.SelectedStatus != "all")
            {
                supplyQuery = supplyQuery.Where(s => s.Status == vm.SelectedStatus);
            }

            var supplyRequests = await supplyQuery.OrderByDescending(s => s.RequestDate).ToListAsync();

            // 3. Materials dictionary for standard pricing
            var allMaterials = await _context.Materials.ToListAsync();
            var materialPriceMap = allMaterials.ToDictionary(m => m.MaterialId, m => m.UnitPrice);

            // KPI Metrics
            vm.TotalImportCost = receipts.Sum(r => r.TotalAmount);
            vm.TotalImportReceiptsCount = receipts.Count;

            decimal totalExportVal = 0;
            decimal totalDefectiveQty = 0;
            int defectiveIncidents = 0;

            foreach (var req in supplyRequests)
            {
                foreach (var item in req.Items)
                {
                    if ((item.QuantityDefective ?? 0) > 0)
                    {
                        totalDefectiveQty += item.QuantityDefective!.Value;
                        defectiveIncidents++;
                    }
                }
            }

            vm.TotalExportRequestsCount = supplyRequests.Count;
            vm.CompletedRequestsCount = supplyRequests.Count(s => s.Status == "Đã hoàn thành");

            foreach (var req in supplyRequests.Where(s => s.Status == "Đã hoàn thành" || s.Status == "Đã xuất kho"))
            {
                foreach (var item in req.Items)
                {
                    var price = item.Material?.UnitPrice ?? (materialPriceMap.ContainsKey(item.MaterialId) ? materialPriceMap[item.MaterialId] : 0);
                    var qty = item.QuantityReleased ?? item.QuantityRequested;
                    totalExportVal += qty * price;
                }
            }
            vm.TotalExportValue = totalExportVal;
            vm.TotalDefectiveQuantity = totalDefectiveQty;
            vm.DefectiveIncidentsCount = defectiveIncidents;

            // 4. Trend Chart: Daily (or monthly) comparisons
            var daysCount = (vm.ToDate - vm.FromDate).Days + 1;
            if (daysCount <= 31)
            {
                for (int i = 0; i < daysCount; i++)
                {
                    var day = vm.FromDate.AddDays(i);
                    vm.TrendLabels.Add(day.ToString("dd/MM"));

                    var impDaily = receipts.Where(r => r.ImportDate.Date == day).Sum(r => r.TotalAmount);
                    vm.TrendImportData.Add(impDaily);

                    decimal expDaily = 0;
                    foreach (var req in supplyRequests.Where(s => s.RequestDate.Date == day && (s.Status == "Đã hoàn thành" || s.Status == "Đã xuất kho")))
                    {
                        foreach (var it in req.Items)
                        {
                            var price = it.Material?.UnitPrice ?? (materialPriceMap.ContainsKey(it.MaterialId) ? materialPriceMap[it.MaterialId] : 0);
                            expDaily += (it.QuantityReleased ?? it.QuantityRequested) * price;
                        }
                    }
                    vm.TrendExportData.Add(expDaily);
                }
            }
            else
            {
                var cur = new DateTime(vm.FromDate.Year, vm.FromDate.Month, 1);
                while (cur <= vm.ToDate)
                {
                    var next = cur.AddMonths(1);
                    vm.TrendLabels.Add(cur.ToString("MM/yyyy"));

                    var impMonth = receipts.Where(r => r.ImportDate >= cur && r.ImportDate < next).Sum(r => r.TotalAmount);
                    vm.TrendImportData.Add(impMonth);

                    decimal expMonth = 0;
                    foreach (var req in supplyRequests.Where(s => s.RequestDate >= cur && s.RequestDate < next && (s.Status == "Đã hoàn thành" || s.Status == "Đã xuất kho")))
                    {
                        foreach (var it in req.Items)
                        {
                            var price = it.Material?.UnitPrice ?? (materialPriceMap.ContainsKey(it.MaterialId) ? materialPriceMap[it.MaterialId] : 0);
                            expMonth += (it.QuantityReleased ?? it.QuantityRequested) * price;
                        }
                    }
                    vm.TrendExportData.Add(expMonth);
                    cur = next;
                }
            }

            // 5. Supplier Chart: Top suppliers by import cost
            var topSuppliers = receipts
                .GroupBy(r => string.IsNullOrEmpty(r.Supplier) ? "Khác" : r.Supplier)
                .Select(g => new { Supplier = g.Key, Total = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.Total)
                .Take(6)
                .ToList();

            foreach (var sup in topSuppliers)
            {
                vm.SupplierLabels.Add(sup.Supplier);
                vm.SupplierData.Add(sup.Total);
            }

            // 6. Branch Export Chart: Distribution by Branch
            foreach (var b in branches)
            {
                decimal bVal = 0;
                foreach (var req in supplyRequests.Where(s => s.BranchId == b.BranchId && (s.Status == "Đã hoàn thành" || s.Status == "Đã xuất kho")))
                {
                    foreach (var it in req.Items)
                    {
                        var price = it.Material?.UnitPrice ?? (materialPriceMap.ContainsKey(it.MaterialId) ? materialPriceMap[it.MaterialId] : 0);
                        bVal += (it.QuantityReleased ?? it.QuantityRequested) * price;
                    }
                }
                if (bVal > 0 || branches.Count <= 8)
                {
                    vm.BranchExportLabels.Add(b.BranchName);
                    vm.BranchExportData.Add(bVal);
                }
            }

            // 7. Top Materials Exported
            var topMats = supplyRequests
                .SelectMany(s => s.Items)
                .GroupBy(i => i.Material?.MaterialName ?? $"NL #{i.MaterialId}")
                .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.QuantityReleased ?? x.QuantityRequested) })
                .OrderByDescending(x => x.Qty)
                .Take(8)
                .ToList();

            foreach (var mat in topMats)
            {
                vm.TopMaterialLabels.Add(mat.Name);
                vm.TopMaterialData.Add(mat.Qty);
            }

            // Populate Tab 1: Receipts
            if (vm.SelectedScope != "branch")
            {
                vm.Receipts = receipts.Select(r => new WarehouseReceiptReportRow
                {
                    ReceiptId = r.ReceiptId,
                    ReceiptCode = r.ReceiptCode,
                    ImportDate = r.ImportDate,
                    Supplier = r.Supplier,
                    DelivererName = r.DelivererName,
                    DelivererPhone = r.DelivererPhone,
                    ReceiverName = r.ReceiverName,
                    CreatedBy = r.CreatedBy,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status,
                    Items = r.Items.Select(i => new ReceiptItemDetailDto
                    {
                        MaterialCode = i.Material?.MaterialCode ?? "",
                        MaterialName = i.Material?.MaterialName ?? "Nguyên liệu",
                        StorageUnit = i.Material?.StorageUnit ?? "",
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Amount = i.Amount
                    }).ToList()
                }).ToList();
            }

            // Populate Tab 2: Supply Requests
            if (vm.SelectedScope != "warehouse")
            {
                vm.SupplyRequests = supplyRequests.Select(s =>
                {
                    decimal estVal = 0;
                    var itemDtos = s.Items.Select(i =>
                    {
                        var price = i.Material?.UnitPrice ?? (materialPriceMap.ContainsKey(i.MaterialId) ? materialPriceMap[i.MaterialId] : 0);
                        var qty = i.QuantityReleased ?? i.QuantityRequested;
                        estVal += qty * price;
                        return new SupplyRequestItemDetailDto
                        {
                            MaterialCode = i.Material?.MaterialCode ?? "",
                            MaterialName = i.Material?.MaterialName ?? "Nguyên liệu",
                            StorageUnit = i.Material?.StorageUnit ?? "",
                            QuantityRequested = i.QuantityRequested,
                            QuantityReleased = i.QuantityReleased ?? 0,
                            QuantityReceived = i.QuantityReceived ?? 0,
                            QuantityAccepted = i.QuantityAccepted ?? 0,
                            QuantityDefective = i.QuantityDefective ?? 0,
                            DefectType = i.DefectType,
                            DefectNote = i.DefectNote,
                            UnitPrice = price
                        };
                    }).ToList();

                    return new BranchSupplyRequestReportRow
                    {
                        RequestId = s.RequestId,
                        RequestCode = s.RequestCode,
                        BranchId = s.BranchId,
                        BranchName = s.Branch?.BranchName ?? s.BranchId,
                        RequestDate = s.RequestDate,
                        Status = s.Status,
                        ApprovedBy = s.ApprovedBy,
                        ApprovedDate = s.ApprovedDate,
                        DelivererName = s.DelivererName,
                        DelivererPhone = s.DelivererPhone,
                        DeliveryProvider = s.DeliveryProvider,
                        ReceiverName = s.ReceiverName,
                        ReceivedDate = s.ReceivedDate,
                        InspectedBy = s.InspectedBy,
                        InspectedAt = s.InspectedAt,
                        InspectionStatus = s.InspectionStatus,
                        WarehouseNote = s.WarehouseNote,
                        RequestNote = s.RequestNote,
                        TotalEstimatedValue = estVal,
                        Items = itemDtos
                    };
                }).ToList();
            }

            // Populate Tab 3: System Inventory
            var branchInventories = await _context.BranchInventories
                .Include(bi => bi.Branch)
                .ToListAsync();

            foreach (var mat in allMaterials.OrderBy(m => m.Category).ThenBy(m => m.MaterialName))
            {
                var matBranchInvs = branchInventories.Where(bi => bi.MaterialId == mat.MaterialId);
                if (vm.SelectedBranchId != "all")
                {
                    matBranchInvs = matBranchInvs.Where(bi => bi.BranchId == vm.SelectedBranchId);
                }

                var bQty = matBranchInvs.Sum(bi => bi.StockQuantity);
                var threshold = matBranchInvs.Any() ? matBranchInvs.Max(bi => bi.LowStockThreshold) : 10;

                vm.Inventories.Add(new SystemInventoryReportRow
                {
                    MaterialId = mat.MaterialId,
                    MaterialCode = mat.MaterialCode,
                    MaterialName = mat.MaterialName,
                    Category = mat.Category ?? "",
                    MaterialKind = mat.MaterialKind ?? "",
                    StorageUnit = mat.StorageUnit,
                    StandardPrice = mat.UnitPrice,
                    CentralStockQuantity = mat.StockQuantity,
                    BranchStockQuantity = bQty,
                    LowStockThreshold = threshold
                });
            }

            // Populate Tab 4: Defective Items
            foreach (var req in supplyRequests)
            {
                foreach (var it in req.Items.Where(i => (i.QuantityDefective ?? 0) > 0))
                {
                    vm.DefectiveItems.Add(new DefectiveItemReportRow
                    {
                        RequestCode = req.RequestCode,
                        BranchName = req.Branch?.BranchName ?? req.BranchId,
                        InspectedAt = req.InspectedAt,
                        MaterialName = it.Material?.MaterialName ?? "Nguyên liệu",
                        StorageUnit = it.Material?.StorageUnit ?? "",
                        QuantityDefective = it.QuantityDefective ?? 0,
                        DefectType = it.DefectType ?? "Không xác định",
                        DefectNote = it.DefectNote,
                        InspectedBy = req.InspectedBy,
                        DefectImageUrl = it.DefectImageUrl
                    });
                }
            }

            return View(vm);
        }

        // GET: /Reporting/CashHandover
        public async Task<IActionResult> CashHandover(string? branchId, DateTime? fromDate, DateTime? toDate, string? handoverType, string? discrepancyStatus)
        {
            var vm = new CashHandoverReportViewModel();
            var branches = await _context.Branches.ToListAsync();

            vm.BranchList = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả chi nhánh" }
            };
            vm.BranchList.AddRange(branches.Select(b => new SelectListItem
            {
                Value = b.BranchId,
                Text = b.BranchName
            }));

            vm.HandoverTypeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả loại ca" },
                new SelectListItem { Value = "FirstShift", Text = "Đầu ca (FirstShift)" },
                new SelectListItem { Value = "MidShift", Text = "Giữa ca (MidShift)" },
                new SelectListItem { Value = "LastShift", Text = "Cuối ngày (LastShift)" },
                new SelectListItem { Value = "Emergency", Text = "Đột xuất (Emergency)" },
                new SelectListItem { Value = "Normal", Text = "Thông thường" }
            };

            vm.SelectedBranchId = string.IsNullOrEmpty(branchId) ? "all" : branchId;
            vm.SelectedHandoverType = string.IsNullOrEmpty(handoverType) ? "all" : handoverType;
            vm.SelectedDiscrepancyStatus = string.IsNullOrEmpty(discrepancyStatus) ? "all" : discrepancyStatus;

            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);
            vm.FromDate = start;
            vm.ToDate = end.AddDays(-1);

            var query = _context.CashHandovers
                .Include(c => c.Branch)
                .Include(c => c.FixedShift)
                .Include(c => c.OutgoingCashier)
                .Include(c => c.IncomingCashier)
                .Where(c => c.HandoverDate >= start && c.HandoverDate < end);

            if (vm.SelectedBranchId != "all")
            {
                query = query.Where(c => c.BranchId == vm.SelectedBranchId);
            }
            if (vm.SelectedHandoverType != "all")
            {
                query = query.Where(c => c.HandoverType == vm.SelectedHandoverType);
            }

            var list = await query.OrderByDescending(c => c.HandoverDate).ThenByDescending(c => c.ShiftId).ToListAsync();

            if (vm.SelectedDiscrepancyStatus == "balanced")
            {
                list = list.Where(c => (c.ActualCash - c.TheoreticalCash) == 0).ToList();
            }
            else if (vm.SelectedDiscrepancyStatus == "surplus")
            {
                list = list.Where(c => (c.ActualCash - c.TheoreticalCash) > 0).ToList();
            }
            else if (vm.SelectedDiscrepancyStatus == "shortage")
            {
                list = list.Where(c => (c.ActualCash - c.TheoreticalCash) < 0).ToList();
            }

            // KPI Metrics
            vm.TotalMachineCashRevenue = list.Sum(c => c.MachineCashRevenue);
            vm.TotalBankTransferRevenue = list.Sum(c => c.BankTransferRevenue);
            vm.TotalDepositedCash = list.Sum(c => c.DepositedCash);
            vm.TotalDiscrepancy = list.Sum(c => c.ActualCash - c.TheoreticalCash);
            vm.TotalHandoversCount = list.Count;
            vm.DiscrepancyIncidentsCount = list.Count(c => (c.ActualCash - c.TheoreticalCash) != 0);

            vm.Handovers = list.Select(c => new CashHandoverReportRow
            {
                HandoverId = c.HandoverId,
                BranchId = c.BranchId,
                BranchName = c.Branch?.BranchName ?? c.BranchId,
                HandoverDate = c.HandoverDate,
                ShiftId = c.ShiftId,
                ShiftName = c.FixedShift?.ShiftName ?? $"Ca #{c.ShiftId}",
                ShiftTime = c.FixedShift != null ? $"{c.FixedShift.StartTime:hh\\:mm} - {c.FixedShift.EndTime:hh\\:mm}" : "",
                OutgoingCashierName = c.OutgoingCashier?.FullName ?? c.OutgoingCashier?.Username ?? $"NV #{c.OutgoingCashierId}",
                IncomingCashierName = c.IncomingCashier?.FullName ?? c.IncomingCashier?.Username ?? (c.IncomingCashierId > 0 ? $"NV #{c.IncomingCashierId}" : "Không"),
                InitialCash = c.InitialCash,
                MachineCashRevenue = c.MachineCashRevenue,
                BankTransferRevenue = c.BankTransferRevenue,
                CashRefundAmount = c.CashRefundAmount,
                TheoreticalCash = c.TheoreticalCash,
                ActualCash = c.ActualCash,
                RetainedCash = c.RetainedCash,
                DepositedCash = c.DepositedCash,
                HandoverType = c.HandoverType,
                EmergencyReason = c.EmergencyReason,
                Notes = c.Notes,
                Status = c.Status,
                OpenedAt = c.OpenedAt,
                ClosedAt = c.ClosedAt
            }).ToList();

            return View(vm);
        }

        // GET: /Reporting/ExportWarehouseImports
        [HttpGet]
        public async Task<IActionResult> ExportWarehouseImports(DateTime? fromDate, DateTime? toDate)
        {
            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);

            var receipts = await _context.WarehouseReceipts
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Where(r => r.ImportDate >= start && r.ImportDate < end)
                .OrderByDescending(r => r.ImportDate)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Mã phiếu nhập,Ngày nhập kho,Nhà cung cấp,Người lập phiếu,Người giao hàng,SĐT người giao,Người nhận hàng,Trạng thái phiếu,Tổng tiền phiếu (VNĐ),Mã nguyên liệu,Tên nguyên liệu,Đơn vị tính,Số lượng nhập,Đơn giá nhập (VNĐ),Thành tiền (VNĐ)");

            foreach (var r in receipts)
            {
                var dateStr = r.ImportDate.ToVietnamTimeString("dd/MM/yyyy HH:mm:ss");
                if (!r.Items.Any())
                {
                    var line = $"{EscapeCsv(r.ReceiptCode)},{EscapeCsv(dateStr)},{EscapeCsv(r.Supplier)},{EscapeCsv(r.CreatedBy)},{EscapeCsv(r.DelivererName)},{EscapeCsv(r.DelivererPhone)},{EscapeCsv(r.ReceiverName)},{EscapeCsv(r.Status)},{r.TotalAmount},\"\",\"\",\"\",0,0,0";
                    csvBuilder.AppendLine(line);
                }
                else
                {
                    foreach (var item in r.Items)
                    {
                        var line = $"{EscapeCsv(r.ReceiptCode)},{EscapeCsv(dateStr)},{EscapeCsv(r.Supplier)},{EscapeCsv(r.CreatedBy)},{EscapeCsv(r.DelivererName)},{EscapeCsv(r.DelivererPhone)},{EscapeCsv(r.ReceiverName)},{EscapeCsv(r.Status)},{r.TotalAmount},{EscapeCsv(item.Material?.MaterialCode)},{EscapeCsv(item.Material?.MaterialName)},{EscapeCsv(item.Material?.StorageUnit)},{item.Quantity},{item.UnitPrice},{item.Amount}";
                        csvBuilder.AppendLine(line);
                    }
                }
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = $"BaoCaoNhapKhoTong_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Reporting/ExportWarehouseExports
        [HttpGet]
        public async Task<IActionResult> ExportWarehouseExports(string? branchId, DateTime? fromDate, DateTime? toDate)
        {
            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);

            var query = _context.BranchSupplyRequests
                .Include(s => s.Branch)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Material)
                .Where(s => s.RequestDate >= start && s.RequestDate < end);

            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                query = query.Where(s => s.BranchId == branchId);
            }

            var requests = await query.OrderByDescending(s => s.RequestDate).ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Mã yêu cầu,Chi nhánh,Ngày yêu cầu,Trạng thái đơn,Người duyệt,Ngày duyệt,Người giao hàng,SĐT giao,Đơn vị vận chuyển,Người nhận,SĐT nhận,Ngày nhận,Người đồng kiểm,Thời gian đồng kiểm,Kết quả kiểm định,Mã nguyên liệu,Tên nguyên liệu,Đơn vị tính,SL yêu cầu,SL thực xuất,SL thực nhận,SL đạt chuẩn,SL lỗi/hỏng,Loại lỗi,Ghi chú lỗi");

            foreach (var req in requests)
            {
                var branchName = req.Branch?.BranchName ?? req.BranchId;
                var reqDateStr = req.RequestDate.ToVietnamTimeString("dd/MM/yyyy HH:mm:ss");
                var appDateStr = req.ApprovedDate.HasValue ? req.ApprovedDate.Value.ToVietnamTimeString("dd/MM/yyyy HH:mm:ss") : "";
                var recDateStr = req.ReceivedDate.HasValue ? req.ReceivedDate.Value.ToVietnamTimeString("dd/MM/yyyy HH:mm:ss") : "";
                var inspDateStr = req.InspectedAt.HasValue ? req.InspectedAt.Value.ToVietnamTimeString("dd/MM/yyyy HH:mm:ss") : "";

                if (!req.Items.Any())
                {
                    var line = $"{EscapeCsv(req.RequestCode)},{EscapeCsv(branchName)},{EscapeCsv(reqDateStr)},{EscapeCsv(req.Status)},{EscapeCsv(req.ApprovedBy)},{EscapeCsv(appDateStr)},{EscapeCsv(req.DelivererName)},{EscapeCsv(req.DelivererPhone)},{EscapeCsv(req.DeliveryProvider)},{EscapeCsv(req.ReceiverName)},{EscapeCsv(req.ReceiverPhone)},{EscapeCsv(recDateStr)},{EscapeCsv(req.InspectedBy)},{EscapeCsv(inspDateStr)},{EscapeCsv(req.InspectionStatus)},\"\",\"\",\"\",0,0,0,0,0,\"\",\"\"";
                    csvBuilder.AppendLine(line);
                }
                else
                {
                    foreach (var item in req.Items)
                    {
                        var line = $"{EscapeCsv(req.RequestCode)},{EscapeCsv(branchName)},{EscapeCsv(reqDateStr)},{EscapeCsv(req.Status)},{EscapeCsv(req.ApprovedBy)},{EscapeCsv(appDateStr)},{EscapeCsv(req.DelivererName)},{EscapeCsv(req.DelivererPhone)},{EscapeCsv(req.DeliveryProvider)},{EscapeCsv(req.ReceiverName)},{EscapeCsv(req.ReceiverPhone)},{EscapeCsv(recDateStr)},{EscapeCsv(req.InspectedBy)},{EscapeCsv(inspDateStr)},{EscapeCsv(req.InspectionStatus)},{EscapeCsv(item.Material?.MaterialCode)},{EscapeCsv(item.Material?.MaterialName)},{EscapeCsv(item.Material?.StorageUnit)},{item.QuantityRequested},{item.QuantityReleased ?? 0},{item.QuantityReceived ?? 0},{item.QuantityAccepted ?? 0},{item.QuantityDefective ?? 0},{EscapeCsv(item.DefectType)},{EscapeCsv(item.DefectNote)}";
                        csvBuilder.AppendLine(line);
                    }
                }
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = !string.IsNullOrEmpty(branchId) && branchId != "all"
                ? $"BaoCaoXuatKhoCungUng_{branchId}_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv"
                : $"BaoCaoXuatKhoCungUng_TatCa_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Reporting/ExportInventory
        [HttpGet]
        public async Task<IActionResult> ExportInventory(string? branchId)
        {
            var materials = await _context.Materials.OrderBy(m => m.Category).ThenBy(m => m.MaterialName).ToListAsync();
            var branchInvs = await _context.BranchInventories
                .Include(bi => bi.Branch)
                .ToListAsync();

            var branchesQuery = _context.Branches.AsQueryable();
            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                branchesQuery = branchesQuery.Where(b => b.BranchId == branchId);
            }
            var branches = await branchesQuery.OrderBy(b => b.BranchId).ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Kho / Chi nhánh,Mã nguyên liệu,Tên nguyên liệu,Danh mục,Kiểu nguyên liệu,Đơn vị lưu kho,Đơn giá (VNĐ),Số lượng tồn kho,Định mức tối thiểu,Trạng thái tồn kho");

            if (string.IsNullOrEmpty(branchId) || branchId == "all")
            {
                foreach (var mat in materials)
                {
                    var status = mat.StockQuantity <= 0 ? "Hết hàng" : (mat.StockQuantity <= 10 ? "Sắp hết" : "An toàn");
                    var line = $"\"Kho Tổng\",{EscapeCsv(mat.MaterialCode)},{EscapeCsv(mat.MaterialName)},{EscapeCsv(mat.Category)},{EscapeCsv(mat.MaterialKind)},{EscapeCsv(mat.StorageUnit)},{mat.UnitPrice},{mat.StockQuantity},10,{EscapeCsv(status)}";
                    csvBuilder.AppendLine(line);
                }
            }

            foreach (var b in branches)
            {
                foreach (var mat in materials)
                {
                    var inv = branchInvs.FirstOrDefault(i => i.BranchId == b.BranchId && i.MaterialId == mat.MaterialId);
                    var qty = inv?.StockQuantity ?? 0;
                    var threshold = inv?.LowStockThreshold ?? 10;
                    var status = qty <= 0 ? "Hết hàng" : (qty <= threshold ? "Sắp hết" : "An toàn");

                    var line = $"{EscapeCsv(b.BranchName)},{EscapeCsv(mat.MaterialCode)},{EscapeCsv(mat.MaterialName)},{EscapeCsv(mat.Category)},{EscapeCsv(mat.MaterialKind)},{EscapeCsv(mat.StorageUnit)},{mat.UnitPrice},{qty},{threshold},{EscapeCsv(status)}";
                    csvBuilder.AppendLine(line);
                }
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = !string.IsNullOrEmpty(branchId) && branchId != "all"
                ? $"BaoCaoTonKho_{branchId}_{DateTime.Today:yyyyMMdd}.csv"
                : $"BaoCaoTonKho_ToanHeThong_{DateTime.Today:yyyyMMdd}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Reporting/ExportCashHandover
        [HttpGet]
        public async Task<IActionResult> ExportCashHandover(string? branchId, DateTime? fromDate, DateTime? toDate)
        {
            var start = (fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var end = (toDate ?? DateTime.Today).Date.AddDays(1);

            var query = _context.CashHandovers
                .Include(c => c.Branch)
                .Include(c => c.FixedShift)
                .Include(c => c.OutgoingCashier)
                .Include(c => c.IncomingCashier)
                .Where(c => c.HandoverDate >= start && c.HandoverDate < end);

            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                query = query.Where(c => c.BranchId == branchId);
            }

            var handovers = await query
                .OrderByDescending(c => c.HandoverDate)
                .ThenByDescending(c => c.ShiftId)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Chi nhánh,Ngày bàn giao,Tên ca,Khung giờ,Loại bàn giao,Thu ngân giao ca,Thu ngân nhận ca,Tiền mặt đầu ca (VNĐ),Doanh thu tiền mặt máy (VNĐ),Doanh thu chuyển khoản (VNĐ),Tiền mặt hoàn trả (VNĐ),Tiền mặt lý thuyết (VNĐ),Tiền mặt thực đếm (VNĐ),Chênh lệch két (VNĐ),Tiền lưu két hôm sau (VNĐ),Tiền nộp két tổng (VNĐ),Trạng thái,Thời điểm mở ca,Thời điểm đóng ca,Lý do đột xuất,Ghi chú");

            foreach (var h in handovers)
            {
                var branchName = h.Branch?.BranchName ?? h.BranchId;
                var dateStr = h.HandoverDate.ToString("dd/MM/yyyy");
                var shiftName = h.FixedShift?.ShiftName ?? $"Ca #{h.ShiftId}";
                var shiftTime = h.FixedShift != null ? $"{h.FixedShift.StartTime:hh\\:mm} - {h.FixedShift.EndTime:hh\\:mm}" : "";
                var outName = h.OutgoingCashier?.FullName ?? h.OutgoingCashier?.Username ?? h.OutgoingCashierId.ToString();
                var inName = h.IncomingCashier?.FullName ?? h.IncomingCashier?.Username ?? (h.IncomingCashierId > 0 ? h.IncomingCashierId.ToString() : "");
                var openStr = h.OpenedAt.ToVietnamTimeString("HH:mm:ss dd/MM/yyyy");
                var closeStr = h.ClosedAt.HasValue ? h.ClosedAt.Value.ToVietnamTimeString("HH:mm:ss dd/MM/yyyy") : "";
                var disc = h.ActualCash - h.TheoreticalCash;

                var line = $"{EscapeCsv(branchName)},{EscapeCsv(dateStr)},{EscapeCsv(shiftName)},{EscapeCsv(shiftTime)},{EscapeCsv(h.HandoverType)},{EscapeCsv(outName)},{EscapeCsv(inName)},{h.InitialCash},{h.MachineCashRevenue},{h.BankTransferRevenue},{h.CashRefundAmount},{h.TheoreticalCash},{h.ActualCash},{disc},{h.RetainedCash},{h.DepositedCash},{EscapeCsv(h.Status)},{EscapeCsv(openStr)},{EscapeCsv(closeStr)},{EscapeCsv(h.EmergencyReason)},{EscapeCsv(h.Notes)}";
                csvBuilder.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = !string.IsNullOrEmpty(branchId) && branchId != "all"
                ? $"BaoCaoBanGiaoCa_{branchId}_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv"
                : $"BaoCaoBanGiaoCa_TatCa_{start:yyyyMMdd}_{end.AddDays(-1):yyyyMMdd}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        private static string EscapeCsv(object? value)
        {
            if (value == null) return "\"\"";
            var str = value.ToString() ?? "";
            return $"\"{str.Replace("\"", "\"\"")}\"";
        }
    }
}
