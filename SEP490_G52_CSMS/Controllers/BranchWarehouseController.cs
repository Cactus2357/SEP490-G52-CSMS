using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Services.Interfaces;
using System.Security.Claims;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "BranchManager,RManager")]
    public class BranchWarehouseController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly IWarehouseSupplyService _warehouseSupplyService;
        private readonly INotificationService _notificationService;

        public BranchWarehouseController(
            CSMSAppDbContext context,
            IWarehouseSupplyService warehouseSupplyService,
            INotificationService notificationService)
        {
            _context = context;
            _warehouseSupplyService = warehouseSupplyService;
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
                if (_context.Database.IsSqlServer())
                {
                    query = query.Where(bi => bi.Material != null && EF.Functions.Collate(bi.Material.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(searchString));
                }
                else
                {
                    query = query.Where(bi => bi.Material != null && bi.Material.MaterialName.Contains(searchString));
                }
            }

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                query = query.Where(bi => bi.Material != null && bi.Material.Category == selectedCategory);
            }

            if (!string.IsNullOrEmpty(selectedKind))
            {
                query = query.Where(bi => bi.Material != null && bi.Material.MaterialKind == selectedKind);
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var list = await query
                .OrderBy(bi => bi.Material!.MaterialName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Materials
                .Where(m => !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category)
                .Distinct()
                .ToListAsync();

            var kinds = await _context.Materials
                .Where(m => !string.IsNullOrEmpty(m.MaterialKind))
                .Select(m => m.MaterialKind)
                .Distinct()
                .ToListAsync();

            var viewModel = new BranchInventoryIndexViewModel
            {
                Items = list,
                SearchString = searchString,
                SelectedCategory = selectedCategory,
                SelectedKind = selectedKind,
                Categories = categories,
                Kinds = kinds,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryDetails(int id)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var item = await _context.BranchInventories
                .Include(bi => bi.Material)
                .FirstOrDefaultAsync(bi => bi.InventoryId == id && (bi.BranchId == branchId || User.IsInRole("RManager")));

            if (item == null) return NotFound();

            return Json(new
            {
                inventoryId = item.InventoryId,
                materialId = item.MaterialId,
                materialCode = item.Material?.MaterialCode ?? "",
                materialName = item.Material?.MaterialName ?? "",
                category = item.Material?.Category ?? "",
                materialKind = item.Material?.MaterialKind ?? "",
                stockQuantity = item.StockQuantity.ToString("G29") + " " + (item.Material?.StorageUnit ?? ""),
                rawStockQuantity = item.StockQuantity,
                lowStockThreshold = item.LowStockThreshold.ToString("G29") + " " + (item.Material?.StorageUnit ?? ""),
                rawLowStockThreshold = item.LowStockThreshold,
                storageUnit = item.Material?.StorageUnit ?? "",
                physicalState = item.Material?.PhysicalState ?? "",
                supplier = item.Material?.Supplier ?? "N/A",
                origin = item.Material?.Origin ?? "N/A"
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchCentralCatalog(string? term)
        {
            try
            {
                var (branchId, _) = await GetUserBranchAsync();
                if (string.IsNullOrEmpty(branchId)) return Json(new List<object>());

                term = term?.Trim() ?? "";

                var trackedMaterialIds = await _context.BranchInventories
                    .Where(bi => bi.BranchId == branchId)
                    .Select(bi => bi.MaterialId)
                    .ToListAsync();

                var query = _context.Materials
                    .Where(m => !trackedMaterialIds.Contains(m.MaterialId));

                if (!string.IsNullOrEmpty(term))
                {
                    if (_context.Database.IsSqlServer())
                    {
                        query = query.Where(m => EF.Functions.Collate(m.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(term));
                    }
                    else
                    {
                        query = query.Where(m => m.MaterialName.ToLower().Contains(term.ToLower()));
                    }
                }

                var materials = await query
                    .OrderBy(m => m.MaterialName)
                    .Take(15)
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
            catch
            {
                try
                {
                    var (branchId, _) = await GetUserBranchAsync();
                    var trackedIds = await _context.BranchInventories
                        .Where(bi => bi.BranchId == branchId)
                        .Select(bi => bi.MaterialId)
                        .ToListAsync();

                    var fallbackMaterials = await _context.Materials
                        .Where(m => !trackedIds.Contains(m.MaterialId) && (string.IsNullOrEmpty(term) || m.MaterialName.Contains(term)))
                        .OrderBy(m => m.MaterialName)
                        .Take(15)
                        .Select(m => new
                        {
                            materialId = m.MaterialId,
                            materialName = m.MaterialName,
                            category = m.Category,
                            materialKind = m.MaterialKind
                        })
                        .ToListAsync();

                    return Json(fallbackMaterials);
                }
                catch
                {
                    return Json(new List<object>());
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateThreshold(int materialId, decimal threshold)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            if (threshold < 0)
            {
                return Json(new { success = false, message = "Mức cảnh báo tồn kho tối thiểu không được âm." });
            }

            var item = await _context.BranchInventories
                .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.MaterialId == materialId);

            if (item == null)
            {
                return Json(new { success = false, message = "Nguyên liệu không tồn tại trong kho chi nhánh." });
            }

            item.LowStockThreshold = threshold;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật định mức cảnh báo thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> AdjustStockQuantity([FromBody] BranchStockAdjustmentDto model)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Không thể xác định chi nhánh của bạn." });

            if (model == null) return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ." });

            if (model.ActualStock < 0)
            {
                return Json(new { success = false, message = "Số lượng tồn kho thực tế không được nhỏ hơn 0." });
            }

            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                return Json(new { success = false, message = "Vui lòng chọn hoặc nhập lý do điều chỉnh tồn kho." });
            }

            var query = _context.BranchInventories
                .Include(bi => bi.Material)
                .Where(bi => bi.BranchId == branchId);

            BranchInventory? item = null;
            if (model.InventoryId.HasValue && model.InventoryId.Value > 0)
            {
                item = await query.FirstOrDefaultAsync(bi => bi.InventoryId == model.InventoryId.Value);
            }
            else if (model.MaterialId.HasValue && model.MaterialId.Value > 0)
            {
                item = await query.FirstOrDefaultAsync(bi => bi.MaterialId == model.MaterialId.Value);
            }

            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nguyên liệu trong kho chi nhánh." });
            }

            if (model.LowStockThreshold.HasValue && model.LowStockThreshold.Value < 0)
            {
                return Json(new { success = false, message = "Ngưỡng cảnh báo tồn kho không được âm." });
            }

            decimal oldStock = item.StockQuantity;
            decimal newStock = model.ActualStock;
            decimal diff = newStock - oldStock;

            item.StockQuantity = newStock;
            if (model.LowStockThreshold.HasValue && model.LowStockThreshold.Value >= 0)
            {
                item.LowStockThreshold = model.LowStockThreshold.Value;
            }
            await _context.SaveChangesAsync();

            // Send notification to Regional Manager
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Quản lý";
                string sign = diff > 0 ? "+" : "";
                string diffStr = $"{sign}{diff.ToString("G29")} {item.Material?.StorageUnit ?? ""}";
                string title = $"[Kiểm kê kho] Điều chỉnh tồn: {item.Material?.MaterialName ?? "Nguyên liệu"}";
                string message = $"Chi nhánh {branchName}: Người thực hiện {userName} đã cập nhật tồn kho '{item.Material?.MaterialName}' từ {oldStock.ToString("G29")} thành {newStock.ToString("G29")} {item.Material?.StorageUnit} (Chênh lệch: {diffStr}). Lý do: {model.Reason.Trim()}. Ghi chú: {model.Note?.Trim() ?? "Không có"}";

                await _notificationService.SendAsync(new NotificationEvent(
                    Title: title,
                    Message: message,
                    RecipientRole: "RManager",
                    ResourceUrl: "/BranchWarehouse",
                    BranchId: branchId
                ));
            }
            catch
            {
                // Ensure notification logging doesn't abort stock update
            }

            return Json(new
            {
                success = true,
                message = $"Cập nhật tồn kho thực tế cho '{item.Material?.MaterialName}' thành công!",
                inventoryId = item.InventoryId,
                materialId = item.MaterialId,
                materialName = item.Material?.MaterialName,
                oldStock = oldStock,
                newStock = newStock,
                diff = diff,
                storageUnit = item.Material?.StorageUnit,
                isLow = newStock <= item.LowStockThreshold
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUntrackedMaterials()
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new List<object>());

            var trackedMaterialIds = await _context.BranchInventories
                .Where(bi => bi.BranchId == branchId)
                .Select(bi => bi.MaterialId)
                .ToListAsync();

            var untracked = await _context.Materials
                .Where(m => !trackedMaterialIds.Contains(m.MaterialId))
                .Select(m => new
                {
                    materialId = m.MaterialId,
                    materialName = m.MaterialName,
                    category = m.Category,
                    materialKind = m.MaterialKind,
                    storageUnit = m.StorageUnit
                })
                .ToListAsync();

            return Json(untracked);
        }

        [HttpPost]
        public async Task<IActionResult> AddMaterialToBranch(int materialId, decimal lowStockThreshold = 10m)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            var materialExists = await _context.Materials.AnyAsync(m => m.MaterialId == materialId);
            if (!materialExists)
            {
                return Json(new { success = false, message = "Nguyên liệu không tồn tại trong hệ thống." });
            }

            var alreadyTracked = await _context.BranchInventories.AnyAsync(bi => bi.BranchId == branchId && bi.MaterialId == materialId);
            if (alreadyTracked)
            {
                return Json(new { success = false, message = "Nguyên liệu này đã có trong danh sách theo dõi của chi nhánh." });
            }

            var branchInventory = new BranchInventory
            {
                BranchId = branchId,
                MaterialId = materialId,
                StockQuantity = 0,
                LowStockThreshold = lowStockThreshold >= 0 ? lowStockThreshold : 10m
            };

            _context.BranchInventories.Add(branchInventory);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm nguyên liệu thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBranchMaterial(int? materialId, string? materialName, decimal? lowStockThreshold = 10m)
        {
            try
            {
                var (branchId, branchName) = await GetUserBranchAsync();
                if (string.IsNullOrEmpty(branchId))
                {
                    return Json(new { success = false, message = "Không thể xác định chi nhánh của bạn. Vui lòng đăng nhập lại." });
                }

                materialName = materialName?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(materialName) && (!materialId.HasValue || materialId.Value <= 0))
                {
                    return Json(new { success = false, message = "Tên nguyên liệu là bắt buộc." });
                }

                var trackedMaterialIds = await _context.BranchInventories
                    .Where(bi => bi.BranchId == branchId)
                    .Select(bi => bi.MaterialId)
                    .ToListAsync();

                Material? material = null;
                if (materialId.HasValue && materialId.Value > 0)
                {
                    material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == materialId.Value);
                }

                if (material == null && !string.IsNullOrEmpty(materialName))
                {
                    // Find by name in central materials (case-insensitive)
                    // First try untracked
                    material = await _context.Materials.FirstOrDefaultAsync(m =>
                        !trackedMaterialIds.Contains(m.MaterialId) &&
                        m.MaterialName.ToLower() == materialName.ToLower());

                    if (material == null)
                    {
                        material = await _context.Materials.FirstOrDefaultAsync(m =>
                            m.MaterialName.ToLower() == materialName.ToLower());
                    }

                    if (material == null && _context.Database.IsSqlServer())
                    {
                        material = await _context.Materials.FirstOrDefaultAsync(m =>
                            EF.Functions.Collate(m.MaterialName, "SQL_Latin1_General_CP1_CI_AI") == materialName);
                    }
                }

                if (material == null)
                {
                    return Json(new { success = false, message = "Tên nguyên liệu không tồn tại trong kho tổng. Tên nguyên liệu phải khớp với nguyên liệu tạo từ kho tổng." });
                }

                if (trackedMaterialIds.Contains(material.MaterialId))
                {
                    return Json(new { success = false, message = $"Nguyên liệu '{material.MaterialName}' đã tồn tại trong danh sách tồn kho chi nhánh." });
                }

                decimal threshold = (lowStockThreshold.HasValue && lowStockThreshold.Value >= 0) ? lowStockThreshold.Value : 10m;

                var branchInventory = new BranchInventory
                {
                    BranchId = branchId,
                    MaterialId = material.MaterialId,
                    StockQuantity = 0,
                    LowStockThreshold = threshold
                };

                _context.BranchInventories.Add(branchInventory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã thêm nguyên liệu '{material.MaterialName}' vào kho chi nhánh thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm nguyên liệu vào kho chi nhánh: " + ex.Message });
            }
        }


        // --- 2. CREATE BRANCH SUPPLY REQUEST (UC62) ---
        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Content("Lỗi chi nhánh.");

            ViewBag.BranchName = branchName;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var employee = await _context.Employees.FindAsync(userId);
                ViewBag.DefaultReceiverName = employee?.FullName ?? "";
                ViewBag.DefaultReceiverPhone = employee?.PhoneNumber ?? "";
            }

            var trackedInventories = await _warehouseSupplyService.GetBranchInventoryListAsync(branchId);
            return View(trackedInventories);
        }

        [HttpGet]
        public async Task<IActionResult> SearchTrackedMaterials(string term)
        {
            var (branchId, _) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new List<object>());

            var materials = await _warehouseSupplyService.SearchTrackedMaterialsAsync(branchId, term);
            return Json(materials);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitSupplyRequest([FromBody] SupplyRequestSubmitModel model)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "BManager";
            var result = await _warehouseSupplyService.CreateSupplyRequestAsync(branchId, branchName, model, userName);
            return Json(new { success = result.Success, message = result.Message, requestCode = result.RequestCode });
        }


        // --- 3. VIEW SUPPLY REQUEST HISTORY (UC63) ---
        [HttpGet]
        public async Task<IActionResult> RequestHistory(DateTime? fromDate, DateTime? toDate, string status = "Tất cả", int page = 1)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Content("Lỗi chi nhánh.");

            ViewBag.BranchName = branchName;

            var today = DateTime.Today;
            var defaultFrom = today.AddMonths(-3);
            var defaultTo = today;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                TempData["HistoryError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
                return RedirectToAction("RequestHistory", new { fromDate = defaultFrom.ToString("yyyy-MM-dd"), toDate = defaultTo.ToString("yyyy-MM-dd"), status = status });
            }

            var viewModel = await _warehouseSupplyService.GetBranchRequestHistoryAsync(branchId, filterFrom, filterTo, status, page, 10);
            return View(viewModel);
        }


        // --- 4. REQUEST DETAIL & CANCEL & RECEIVE CONFIRMATION (UC64) ---
        [HttpGet]
        public async Task<IActionResult> GetRequestDetails(string code)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var details = await _warehouseSupplyService.GetRequestDetailsJsonAsync(code, branchId);
            if (details == null) return NotFound();

            return Json(details);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(string code)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var result = await _warehouseSupplyService.CancelSupplyRequestAsync(code, branchId);
            return Json(new { success = result.Success, message = result.Message });
        }

        // --- 5. CO-INSPECTION & DEFECT REPORTING BEFORE RECEIPT ---
        [HttpPost]
        public async Task<IActionResult> InspectAndReceiveGoods([FromForm] GoodsInspectionSubmitDto model)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            var currentUserName = User.FindFirstValue(ClaimTypes.Name) ?? "BManager";
            var result = await _warehouseSupplyService.InspectAndReceiveGoodsAsync(branchId, branchName, model, currentUserName);
            return Json(new { success = result.Success, message = result.Message, hasDefects = result.HasDefects });
        }

        // Backward compatibility handler
        [HttpPost]
        public async Task<IActionResult> ConfirmReceipt(string code, string delivererName, string delivererPhone)
        {
            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            var result = await _warehouseSupplyService.ConfirmReceiptLegacyAsync(code, branchId, branchName, delivererName, delivererPhone);
            return Json(new { success = result.Success, message = result.Message, hasDefects = result.HasDefects });
        }
    }
}
