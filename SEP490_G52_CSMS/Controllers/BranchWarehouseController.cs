using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
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
        private readonly IFileStorageService _fileStorageService;

        public BranchWarehouseController(CSMSAppDbContext context, INotificationService notificationService, IFileStorageService fileStorageService)
        {
            _context = context;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
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

            if (string.IsNullOrWhiteSpace(model.ReceiverName))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ tên người nhận hàng tại chi nhánh." });
            }

            if (string.IsNullOrWhiteSpace(model.ReceiverPhone))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại người nhận hàng." });
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
                RequestDate = DateTime.UtcNow,
                ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                ReceiverName = model.ReceiverName?.Trim(),
                ReceiverPhone = model.ReceiverPhone?.Trim(),
                RequestNote = model.RequestNote?.Trim(),
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
                ResourceUrl: "/Warehouse/ExportRequests",
                BranchId: branchId
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
                materialId = i.MaterialId,
                materialName = i.Material?.MaterialName ?? "",
                category = i.Material?.Category ?? "",
                materialKind = i.Material?.MaterialKind ?? "",
                storageUnit = i.Material?.StorageUnit ?? "",
                quantityRequested = i.QuantityRequested.ToString("G29"),
                quantityReleased = i.QuantityReleased.HasValue ? i.QuantityReleased.Value.ToString("G29") : null,
                quantityReceived = i.QuantityReceived.HasValue ? i.QuantityReceived.Value.ToString("G29") : null,
                quantityAccepted = i.QuantityAccepted.HasValue ? i.QuantityAccepted.Value.ToString("G29") : null,
                quantityDefective = i.QuantityDefective.HasValue ? i.QuantityDefective.Value.ToString("G29") : null,
                defectType = i.DefectType ?? "",
                defectNote = i.DefectNote ?? "",
                defectImageUrl = i.DefectImageUrl ?? ""
            });

            return Json(new
            {
                requestCode = req.RequestCode,
                status = req.Status,
                requestDate = req.RequestDate.ToString("dd/MM/yyyy HH:mm"),
                expectedDeliveryDate = req.ExpectedDeliveryDate.HasValue ? req.ExpectedDeliveryDate.Value.ToString("dd/MM/yyyy") : null,
                receiverName = req.ReceiverName ?? "",
                receiverPhone = req.ReceiverPhone ?? "",
                requestNote = req.RequestNote ?? "",
                approvedBy = req.ApprovedBy ?? "Chưa duyệt",
                approvedDate = req.ApprovedDate.HasValue ? req.ApprovedDate.Value.ToString("dd/MM/yyyy HH:mm") : null,
                delivererName = req.DelivererName ?? "",
                delivererPhone = req.DelivererPhone ?? "",
                deliveryProvider = req.DeliveryProvider ?? "",
                receivedDate = req.ReceivedDate.HasValue ? req.ReceivedDate.Value.ToString("dd/MM/yyyy HH:mm") : null,
                inspectedBy = req.InspectedBy ?? "",
                inspectedAt = req.InspectedAt.HasValue ? req.InspectedAt.Value.ToString("dd/MM/yyyy HH:mm") : null,
                inspectionStatus = req.InspectionStatus ?? "",
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

        // --- 5. CO-INSPECTION & DEFECT REPORTING BEFORE RECEIPT ---
        [HttpPost]
        public async Task<IActionResult> InspectAndReceiveGoods([FromForm] GoodsInspectionSubmitDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RequestCode))
            {
                return Json(new { success = false, message = "Dữ liệu biên bản đồng kiểm không hợp lệ." });
            }

            var (branchId, branchName) = await GetUserBranchAsync();
            if (string.IsNullOrEmpty(branchId)) return Json(new { success = false, message = "Lỗi chi nhánh." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var req = await _context.BranchSupplyRequests
                    .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                    .FirstOrDefaultAsync(r => r.RequestCode == model.RequestCode && r.BranchId == branchId);

                if (req == null) return Json(new { success = false, message = "Đơn yêu cầu không tồn tại." });

                if (req.Status != "Đã xuất kho")
                {
                    return Json(new { success = false, message = "Trạng thái đơn hàng không hợp lệ để đồng kiểm nhận hàng." });
                }

                decimal totalDefective = 0;
                var defectSummaries = new List<string>();

                for (int i = 0; i < model.Items.Count; i++)
                {
                    var itemInput = model.Items[i];
                    var reqItem = req.Items.FirstOrDefault(ri => ri.MaterialId == itemInput.MaterialId);
                    if (reqItem == null) continue;

                    if (itemInput.QuantityAccepted < 0 || itemInput.QuantityDefective < 0)
                    {
                        return Json(new { success = false, message = $"Số lượng kiểm đếm của \"{reqItem.Material?.MaterialName}\" không được âm." });
                    }

                    decimal releasedQty = reqItem.QuantityReleased ?? reqItem.QuantityRequested;
                    decimal totalChecked = itemInput.QuantityAccepted + itemInput.QuantityDefective;
                    if (totalChecked > releasedQty)
                    {
                        return Json(new { success = false, message = $"Tổng số lượng nhận ({totalChecked}) của \"{reqItem.Material?.MaterialName}\" không được vượt quá số lượng kho tổng đã xuất ({releasedQty})." });
                    }

                    reqItem.QuantityReceived = totalChecked;
                    reqItem.QuantityAccepted = itemInput.QuantityAccepted;
                    reqItem.QuantityDefective = itemInput.QuantityDefective;
                    reqItem.DefectType = itemInput.QuantityDefective > 0 ? (itemInput.DefectType ?? "Kém chất lượng / Hư hại") : null;
                    reqItem.DefectNote = itemInput.DefectNote?.Trim();

                    // Upload defect proof images if provided (max 5 images, <= 5MB each)
                    var filesToUpload = new List<IFormFile>();
                    if (itemInput.DefectImages != null && itemInput.DefectImages.Any())
                    {
                        filesToUpload.AddRange(itemInput.DefectImages);
                    }
                    else if (itemInput.DefectImage != null && itemInput.DefectImage.Length > 0)
                    {
                        filesToUpload.Add(itemInput.DefectImage);
                    }

                    if (filesToUpload.Count > 5)
                    {
                        return Json(new { success = false, message = $"Nguyên liệu \"{reqItem.Material?.MaterialName}\" chỉ được tải lên tối đa 5 ảnh bằng chứng." });
                    }

                    var uploadedUrls = new List<string>();
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };
                    foreach (var file in filesToUpload)
                    {
                        if (file == null || file.Length == 0) continue;
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            return Json(new { success = false, message = $"Ảnh \"{file.FileName}\" vượt quá dung lượng tối đa 5MB." });
                        }
                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (!allowedExts.Contains(ext))
                        {
                            return Json(new { success = false, message = $"Ảnh \"{file.FileName}\" không đúng định dạng cho phép (.jpg, .jpeg, .png, .webp)." });
                        }

                        var uploadedUrl = await _fileStorageService.UploadFileAsync(file, "supply_defects");
                        if (!string.IsNullOrEmpty(uploadedUrl))
                        {
                            uploadedUrls.Add(uploadedUrl);
                        }
                    }

                    if (uploadedUrls.Count > 0)
                    {
                        reqItem.DefectImageUrl = System.Text.Json.JsonSerializer.Serialize(uploadedUrls);
                    }

                    if (itemInput.QuantityDefective > 0)
                    {
                        totalDefective += itemInput.QuantityDefective;
                        defectSummaries.Add($"{reqItem.Material?.MaterialName} ({itemInput.QuantityDefective} {reqItem.Material?.StorageUnit} - {reqItem.DefectType})");
                    }

                    // CRITICAL: ONLY accepted quantity is added to branch inventory!
                    if (itemInput.QuantityAccepted > 0)
                    {
                        var branchInv = await _context.BranchInventories
                            .FirstOrDefaultAsync(bi => bi.BranchId == branchId && bi.MaterialId == itemInput.MaterialId);

                        if (branchInv == null)
                        {
                            branchInv = new BranchInventory
                            {
                                BranchId = branchId,
                                MaterialId = itemInput.MaterialId,
                                StockQuantity = itemInput.QuantityAccepted,
                                LowStockThreshold = 10m
                            };
                            _context.BranchInventories.Add(branchInv);
                        }
                        else
                        {
                            branchInv.StockQuantity += itemInput.QuantityAccepted;
                        }
                    }
                }

                var userName = User.FindFirstValue(ClaimTypes.Name) ?? "BManager";
                req.InspectedBy = !string.IsNullOrWhiteSpace(model.InspectorName) ? model.InspectorName.Trim() : userName;
                req.InspectedAt = DateTime.UtcNow;
                req.ReceivedDate = DateTime.UtcNow;

                if (totalDefective > 0)
                {
                    req.Status = "Đã nhận - Có hàng lỗi";
                    req.InspectionStatus = "Có hàng lỗi";
                    var defectDetailMsg = string.Join("; ", defectSummaries);

                    // Notify Warehouse Manager
                    await _notificationService.SendAsync(new NotificationEvent(
                        Title: "Cảnh báo: Đơn nhận có hàng lỗi",
                        Message: $"Chi nhánh {branchName} đã đồng kiểm đơn {req.RequestCode} và phát hiện {totalDefective} sản phẩm lỗi: {defectDetailMsg}.",
                        RecipientRole: "WarehouseManager",
                        ResourceUrl: "/Warehouse/ExportRequests",
                        BranchId: branchId
                    ));

                    // Notify Regional Manager
                    await _notificationService.SendAsync(new NotificationEvent(
                        Title: "Báo cáo lỗi nhận hàng chi nhánh",
                        Message: $"Chi nhánh {branchName} ghi nhận hàng giao bị lỗi cho đơn {req.RequestCode}: {defectDetailMsg}.",
                        RecipientRole: "RManager",
                        ResourceUrl: "/Warehouse/ExportRequests",
                        BranchId: branchId
                    ));
                }
                else
                {
                    req.Status = "Đã hoàn thành";
                    req.InspectionStatus = "Đạt 100%";

                    await _notificationService.SendAsync(new NotificationEvent(
                        Title: "Nhập kho hoàn tất",
                        Message: $"Chi nhánh {branchName} đã đồng kiểm đạt 100% và nhập kho thành công cho đơn {req.RequestCode}.",
                        RecipientRole: "WarehouseManager",
                        ResourceUrl: "/Warehouse/ExportRequests",
                        BranchId: branchId
                    ));
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                string responseMsg = totalDefective > 0
                    ? $"Đã hoàn tất đồng kiểm! Hệ thống đã ghi nhận biên bản {totalDefective} nguyên liệu lỗi và chỉ cộng số lượng đạt chuẩn vào kho chi nhánh."
                    : "Đồng kiểm thành công! Toàn bộ nguyên liệu đạt chuẩn đã được nhập vào kho chi nhánh.";

                return Json(new { success = true, message = responseMsg, hasDefects = totalDefective > 0 });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống khi đồng kiểm nhận hàng: " + ex.Message });
            }
        }

        // Backward compatibility handler
        [HttpPost]
        public async Task<IActionResult> ConfirmReceipt(string code, string delivererName, string delivererPhone)
        {
            var (branchId, _) = await GetUserBranchAsync();
            var req = await _context.BranchSupplyRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.RequestCode == code && r.BranchId == branchId);

            if (req == null) return Json(new { success = false, message = "Đơn yêu cầu không tồn tại." });

            var model = new GoodsInspectionSubmitDto
            {
                RequestCode = code,
                InspectorName = delivererName,
                Items = req.Items.Select(i => new GoodsInspectionItemSubmitDto
                {
                    MaterialId = i.MaterialId,
                    QuantityReceived = i.QuantityReleased ?? i.QuantityRequested,
                    QuantityAccepted = i.QuantityReleased ?? i.QuantityRequested,
                    QuantityDefective = 0
                }).ToList()
            };

            return await InspectAndReceiveGoods(model);
        }
    }

    public class SupplyRequestSubmitModel
    {
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? RequestNote { get; set; }
        public List<SupplyRequestItemSubmitModel> Items { get; set; } = new();
    }

    public class SupplyRequestItemSubmitModel
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class GoodsInspectionSubmitDto
    {
        public string RequestCode { get; set; } = string.Empty;
        public string? InspectorName { get; set; }
        public List<GoodsInspectionItemSubmitDto> Items { get; set; } = new();
    }

    public class GoodsInspectionItemSubmitDto
    {
        public int MaterialId { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityAccepted { get; set; }
        public decimal QuantityDefective { get; set; }
        public string? DefectType { get; set; }
        public string? DefectNote { get; set; }
        public List<IFormFile>? DefectImages { get; set; }
        public IFormFile? DefectImage { get; set; }
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
