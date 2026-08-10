using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "RManager")]
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
                selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            else
            {
                if (!DateTime.TryParse(dateRange + "-01", out selectedMonth))
                {
                    selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
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
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync();

            foreach (var b in branches)
            {
                var bOrders = allMonthOrders.Where(o => o.BranchId == b.BranchId).ToList();
                var cashRevenue = bOrders.Where(o => o.PaymentMethod == "Cash").Sum(o => o.TotalAmount);
                var transferRevenue = bOrders.Where(o => o.PaymentMethod != "Cash").Sum(o => o.TotalAmount);

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
                BranchList = branches.Select(b => new SelectListItem
                {
                    Value = b.BranchId,
                    Text = b.BranchName
                }).ToList()
            };
            return View(vm);
        }
    }
}
