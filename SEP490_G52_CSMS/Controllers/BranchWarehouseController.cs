using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager,RManager")]
    public class BranchWarehouseController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly INotificationService _notificationService;

        public BranchWarehouseController(CSMSAppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private async Task<(string BranchId, string BranchName)> GetUserBranchAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => e.EmployeeId == userId);
                if (employee != null && employee.Branch != null)
                {
                    return (employee.BranchId ?? "", employee.Branch.BranchName ?? "");
                }
            }
            return ("", "");
        }

        // --- 1. BRANCH INVENTORY ---
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string selectedCategory, string selectedKind, int page = 1)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId))
            {
                return Content("Không thể xác định chi nhánh của bạn. Vui lòng liên hệ Admin.");
            }

            ViewBag.BranchName = branchName;

            var query = _context.BranchInventories
                .Include(bi => bi.Material)
                .Where(bi => bi.BranchId == branchId);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(bi => bi.Material != null && EF.Functions.Collate(bi.Material.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(searchString));
            }

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                query = query.Where(bi => bi.Material != null && bi.Material.Category == selectedCategory);
            }

            if (!string.IsNullOrEmpty(selectedKind))
            {
                query = query.Where(bi => bi.Material != null && bi.Material.MaterialKind == selectedKind);
            }

            var allCategories = await _context.MaterialCategories.Select(c => c.CategoryName).Distinct().ToListAsync();
            var allKinds = await _context.Materials.Select(m => m.MaterialKind).Distinct().ToListAsync();

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = await query
                .OrderBy(bi => bi.Material != null ? bi.Material.MaterialName : "")
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BranchInventoryIndexViewModel
            {
                Items = items,
                SearchString = searchString,
                SelectedCategory = selectedCategory,
                SelectedKind = selectedKind,
                Categories = allCategories,
                Kinds = allKinds,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryDetails(int id)
        {
            var item = await _context.BranchInventories
                .Include(bi => bi.Material)
                .FirstOrDefaultAsync(bi => bi.InventoryId == id);

            if (item == null) return NotFound();

            return Json(new
            {
                inventoryId = item.InventoryId,
                materialCode = item.Material?.MaterialCode ?? "",
                materialName = item.Material?.MaterialName ?? "",
                category = item.Material?.Category ?? "",
                materialKind = item.Material?.MaterialKind ?? "",
                stockQuantity = item.StockQuantity.ToString("G29") + " " + (item.Material?.StorageUnit ?? ""),
                lowStockThreshold = item.LowStockThreshold.ToString("G29") + " " + (item.Material?.StorageUnit ?? ""),
                storageUnit = item.Material?.StorageUnit ?? "",
                physicalState = item.Material?.PhysicalState ?? "",
                supplier = item.Material?.Supplier ?? "N/A",
                origin = item.Material?.Origin ?? "N/A"
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchCentralCatalog(string term)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new List<object>());

            if (string.IsNullOrEmpty(term)) term = "";
            term = term.Trim().ToLower();

            var trackedMaterialIds = await _context.BranchInventories
                .Where(bi => bi.BranchId == branchId)
                .Select(bi => bi.MaterialId)
                .ToListAsync();

            var materials = await _context.Materials
                .Where(m => !trackedMaterialIds.Contains(m.MaterialId) && EF.Functions.Collate(m.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(term))
                .Take(10)
                .Select(m => new
                {
                    materialId = m.MaterialId,
                    materialName = m.MaterialName,
                    category = m.Category,
                    materialKind = m.MaterialKind
                })
                .ToListAsync();

            return Json(materials);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBranchMaterial(string materialName)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId))
            {
                return Json(new { success = false, message = "Không thể xác định chi nhánh của bạn." });
            }

            if (string.IsNullOrWhiteSpace(materialName))
            {
                return Json(new { success = false, message = "Tên nguyên liệu không được để trống." });
            }

            materialName = materialName.Trim();

            var centralMaterial = await _context.Materials
                .FirstOrDefaultAsync(m => m.MaterialName.ToLower() == materialName.ToLower());

            if (centralMaterial == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Tên nguyên liệu không khớp với danh mục kho tổng. Vui lòng chọn từ gợi ý hoặc liên hệ Quản lý kho."
                });
            }

            var exists = await _context.BranchInventories
                .AnyAsync(bi => bi.BranchId == branchId && bi.MaterialId == centralMaterial.MaterialId);

            if (exists)
            {
                return Json(new { success = false, message = "Nguyên liệu này đã được đăng ký theo dõi tại chi nhánh." });
            }

            var branchInventory = new BranchInventory
            {
                BranchId = branchId,
                MaterialId = centralMaterial.MaterialId,
                StockQuantity = 0m,
                LowStockThreshold = 10m
            };

            _context.BranchInventories.Add(branchInventory);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đăng ký nguyên liệu thành công!" });
        }


        // --- 2. CREATE BRANCH SUPPLY REQUEST (UC62) ---
        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Content("Lỗi chi nhánh.");

            ViewBag.BranchName = branchName;

            // Fetch inventories currently tracked at this branch to allow multi-select bulk request
            var trackedInventories = await _context.BranchInventories
                .Include(bi => bi.Material)
                .Where(bi => bi.BranchId == branchId && bi.Material != null)
                .ToListAsync();

            return View(trackedInventories);
        }

        [HttpGet]
        public async Task<IActionResult> SearchTrackedMaterials(string term)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new List<object>());

            if (string.IsNullOrEmpty(term)) term = "";
            term = term.Trim().ToLower();

            var materials = await _context.BranchInventories
                .Include(bi => bi.Material)
                .Where(bi => bi.BranchId == branchId && bi.Material != null && EF.Functions.Collate(bi.Material.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(term))
                .Select(bi => new
                {
                    materialId = bi.MaterialId,
                    materialName = bi.Material!.MaterialName,
                    category = bi.Material!.Category,
                    materialKind = bi.Material!.MaterialKind,
                    storageUnit = bi.Material!.StorageUnit
                })
                .Take(10)
                .ToListAsync();

            return Json(materials);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitSupplyRequest([FromBody] SupplyRequestSubmitModel model)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            if (model?.Items == null || model.Items.Count == 0)
            {
                return Json(new { success = false, message = "Đơn hàng phải có ít nhất một nguyên liệu." });
            }

            // Consolidate duplicates if sent directly (already consolidated on client side, but good for BR05 back-end safety)
            var consolidatedItems = model.Items
                .GroupBy(i => i.MaterialId)
                .Select(g => new SupplyRequestItemSubmitModel
                {
                    MaterialId = g.Key,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList();

            foreach (var item in consolidatedItems)
            {
                if (item.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Số lượng phải là số dương." });
                }

                var exists = await _context.Materials.AnyAsync(m => m.MaterialId == item.MaterialId);
                if (!exists)
                {
                    return Json(new { success = false, message = "Nguyên liệu không tồn tại trong hệ thống." });
                }
            }

            // Generate unique RequestCode YC-xxx
            var lastRequest = await _context.BranchSupplyRequests.OrderByDescending(r => r.RequestId).FirstOrDefaultAsync();
            var nextNum = (lastRequest?.RequestId ?? 0) + 1;
            var code = $"YC-{nextNum:D3}";

            var request = new BranchSupplyRequest
            {
                RequestCode = code,
                BranchId = branchId,
                RequestDate = DateTime.Now,
                Status = "Chờ duyệt"
            };

            _context.BranchSupplyRequests.Add(request);
            await _context.SaveChangesAsync();

            foreach (var item in consolidatedItems)
            {
                var reqItem = new BranchSupplyRequestItem
                {
                    RequestId = request.RequestId,
                    MaterialId = item.MaterialId,
                    QuantityRequested = item.Quantity
                };
                _context.BranchSupplyRequestItems.Add(reqItem);
            }

            // Raise notification event for WarehouseManager role
            var branch = await _context.Branches.FindAsync(branchId);
            var branchName = branch?.BranchName ?? "Chi nhánh";
            await _notificationService.SendAsync(new NotificationEvent(
                Title: "Yêu cầu xuất kho mới",
                Message: $"Chi nhánh {branchName} đã gửi một yêu cầu xuất kho mới với mã đơn: {code}.",
                RecipientRole: "WarehouseManager",
                ResourceUrl: "/Warehouse/ExportRequests"
            ));

            await _context.SaveChangesAsync();
            return Json(new { success = true, requestCode = code });
        }


        // --- 3. VIEW SUPPLY REQUEST HISTORY (UC63) ---
        [HttpGet]
        public async Task<IActionResult> RequestHistory(DateTime? fromDate, DateTime? toDate, string status = "Tất cả", int page = 1)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Content("Lỗi chi nhánh.");

            ViewBag.BranchName = branchName;

            // Default dates logic (BR03: current month range)
            var now = DateTime.Now;
            var defaultFrom = now.Date.AddMonths(-3);
            var defaultTo = now.Date;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                TempData["HistoryError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
                return RedirectToAction("RequestHistory", new { fromDate = defaultFrom.ToString("yyyy-MM-dd"), toDate = defaultTo.ToString("yyyy-MM-dd"), status = status });
            }

            var query = _context.BranchSupplyRequests
                .Where(r => r.BranchId == branchId && r.RequestDate.Date >= filterFrom.Date && r.RequestDate.Date <= filterTo.Date);

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(r => r.Status == status);
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var list = await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BranchSupplyRequestHistoryViewModel
            {
                Requests = list,
                FromDate = filterFrom,
                ToDate = filterTo,
                SelectedStatus = status,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }


        // --- 4. REQUEST DETAIL & CANCEL & RECEIVE CONFIRMATION (UC64) ---
        [HttpGet]
        public async Task<IActionResult> GetRequestDetails(string code)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var req = await _context.BranchSupplyRequests
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.RequestCode == code && r.BranchId == branchId);

            if (req == null) return NotFound();

            var itemsResult = req.Items.Select(i => new
            {
                materialName = i.Material?.MaterialName ?? "",
                category = i.Material?.Category ?? "",
                materialKind = i.Material?.MaterialKind ?? "",
                storageUnit = i.Material?.StorageUnit ?? "",
                quantityRequested = i.QuantityRequested.ToString("G29"),
                quantityReleased = i.QuantityReleased.HasValue ? i.QuantityReleased.Value.ToString("G29") : null
            });

            return Json(new
            {
                requestCode = req.RequestCode,
                status = req.Status,
                requestDate = req.RequestDate.ToString("dd/MM/yyyy HH:mm"),
                approvedBy = req.ApprovedBy ?? "Chưa duyệt",
                approvedDate = req.ApprovedDate.HasValue ? req.ApprovedDate.Value.ToString("dd/MM/yyyy") : null,
                delivererName = req.DelivererName ?? "",
                delivererPhone = req.DelivererPhone ?? "",
                receivedDate = req.ReceivedDate.HasValue ? req.ReceivedDate.Value.ToString("dd/MM/yyyy HH:mm") : null,
                warehouseNote = req.WarehouseNote ?? "",
                items = itemsResult
            });
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(string code)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var req = await _context.BranchSupplyRequests
                .FirstOrDefaultAsync(r => r.RequestCode == code && r.BranchId == branchId);

            if (req == null) return Json(new { success = false, message = "Đơn yêu cầu không tồn tại." });

            // BR01: Can only cancel if status is "Chờ duyệt"
            if (req.Status != "Chờ duyệt")
            {
                return Json(new { success = false, message = "Chỉ có thể hủy đơn khi đang ở trạng thái Chờ duyệt." });
            }

            req.Status = "Đã hủy";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy đơn yêu cầu nhập kho thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReceipt(string code, string delivererName, string delivererPhone)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var (branchId, _) = await GetUserBranchAsync();
                if (string.IsNullOrWhiteSpace(delivererName))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên người giao hàng." });
                }

                var req = await _context.BranchSupplyRequests
                    .Include(r => r.Items)
                    .FirstOrDefaultAsync(r => r.RequestCode == code && r.BranchId == branchId);

                if (req == null) return Json(new { success = false, message = "Đơn yêu cầu không tồn tại." });

                if (req.Status != "Đã xuất kho")
                {
                    return Json(new { success = false, message = "Trạng thái đơn hàng không hợp lệ để xác nhận nhận hàng." });
                }

                // Confirm physical receipt: Increment branch stock scoped to branchId (BR03)
                foreach (var item in req.Items)
                {
                    var releasedQty = item.QuantityReleased ?? 0;
                    if (releasedQty > 0)
                    {
                        var branchInv = await _context.BranchInventories
                            .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.MaterialId == item.MaterialId);

                        if (branchInv == null)
                        {
                            // Create one if it does not exist
                            branchInv = new BranchInventory
                            {
                                BranchId = branchId,
                                MaterialId = item.MaterialId,
                                StockQuantity = releasedQty,
                                LowStockThreshold = 10m
                            };
                            _context.BranchInventories.Add(branchInv);
                        }
                        else
                        {
                            branchInv.StockQuantity += releasedQty;
                        }
                    }
                }

                req.Status = "Đã hoàn thành";
                req.DelivererName = delivererName.Trim();
                req.DelivererPhone = delivererPhone?.Trim();
                req.ReceivedDate = DateTime.Now;

                // Raise notification event for WarehouseManager role
                var branch = await _context.Branches.FindAsync(branchId);
                var branchName = branch?.BranchName ?? "Chi nhánh";
                await _notificationService.SendAsync(new NotificationEvent(
                    Title: "Nhập kho hoàn tất",
                    Message: $"Chi nhánh {branchName} đã nhận hàng thành công và cập nhật tồn kho cho đơn {code}.",
                    RecipientRole: "WarehouseManager",
                    ResourceUrl: "/Warehouse/ExportRequests"
                ));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Xác nhận nhận hàng thành công. Tồn kho chi nhánh đã được cập nhật." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống khi xác nhận nhận hàng: " + ex.Message });
            }
        }
    }

    public class SupplyRequestSubmitModel
    {
        public List<SupplyRequestItemSubmitModel> Items { get; set; } = new();
    }

    public class SupplyRequestItemSubmitModel
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class BranchSupplyRequestHistoryViewModel
    {
        public List<BranchSupplyRequest> Requests { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SelectedStatus { get; set; } = "Tất cả";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class BranchInventoryIndexViewModel
    {
        public List<BranchInventory> Items { get; set; } = new();
        public string? SearchString { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedKind { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<string> Kinds { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
