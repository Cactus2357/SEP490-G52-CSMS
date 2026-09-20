using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "WarehouseManager,RManager")]
    public class WarehouseController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IWarehouseSupplyService _warehouseSupplyService;

        public WarehouseController(
            CSMSAppDbContext context,
            INotificationService notificationService,
            IWarehouseSupplyService warehouseSupplyService)
        {
            _context = context;
            _notificationService = notificationService;
            _warehouseSupplyService = warehouseSupplyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchString, string? selectedCategory, string? selectedKind, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.Materials.AsQueryable();

            // Search by material name
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                if (_context.Database.IsSqlServer())
                {
                    query = query.Where(m => EF.Functions.Collate(m.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(searchString));
                }
                else
                {
                    query = query.Where(m => m.MaterialName.Contains(searchString));
                }
            }

            // Filter by Category (Loại NgL)
            if (!string.IsNullOrWhiteSpace(selectedCategory))
            {
                query = query.Where(m => m.Category == selectedCategory);
            }

            // Filter by MaterialKind (Kiểu NgL)
            if (!string.IsNullOrWhiteSpace(selectedKind))
            {
                query = query.Where(m => m.MaterialKind == selectedKind);
            }

            // Total count for pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var materials = await query
                .OrderBy(m => m.MaterialCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Fetch distinct categories and kinds for dropdowns
            var categories = await _context.MaterialCategories.OrderBy(c => c.CategoryName).Select(c => c.CategoryName).ToListAsync();
            var kinds = await _context.Materials.Select(m => m.MaterialKind).Distinct().ToListAsync();

            var viewModel = new WarehouseIndexViewModel
            {
                Materials = materials,
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
        public async Task<IActionResult> GetMaterialDetails(int id)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == id);
            if (material == null)
            {
                return NotFound();
            }

            return Json(new
            {
                materialId = material.MaterialId,
                materialCode = material.MaterialCode,
                materialName = material.MaterialName,
                materialKind = material.MaterialKind,
                physicalState = material.PhysicalState,
                category = material.Category,
                supplier = material.Supplier,
                unitPrice = material.UnitPrice.ToString("N0") + " đ/" + material.StorageUnit,
                rawUnitPrice = material.UnitPrice,
                origin = material.Origin ?? "",
                storageUnit = material.StorageUnit,
                stockQuantity = material.StockQuantity.ToString("G29") + " " + material.StorageUnit
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.MaterialCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            return Json(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Tên loại nguyên liệu là bắt buộc." });
            }

            var trimmedName = name.Trim();
            var exists = await _context.MaterialCategories
                .AnyAsync(c => c.CategoryName.ToLower() == trimmedName.ToLower());

            if (exists)
            {
                return Json(new { success = false, message = "Lỗi trùng tên loại nguyên liệu." });
            }

            var category = new MaterialCategory
            {
                CategoryName = trimmedName,
                Description = description?.Trim()
            };

            _context.MaterialCategories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, categoryId = category.CategoryId, categoryName = category.CategoryName });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id, string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Tên loại nguyên liệu là bắt buộc." });
            }

            var trimmedName = name.Trim();
            var category = await _context.MaterialCategories.FindAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Loại nguyên liệu không tồn tại." });
            }

            var exists = await _context.MaterialCategories
                .AnyAsync(c => c.CategoryName.ToLower() == trimmedName.ToLower() && c.CategoryId != id);

            if (exists)
            {
                return Json(new { success = false, message = "Lỗi trùng tên loại nguyên liệu." });
            }

            var oldName = category.CategoryName;
            category.CategoryName = trimmedName;
            category.Description = description?.Trim();

            // Cascade rename category string in materials
            var materials = await _context.Materials.Where(m => m.Category == oldName).ToListAsync();
            foreach (var mat in materials)
            {
                mat.Category = trimmedName;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMaterial(string name, string physicalState, string materialKind, string category, decimal price, string? supplier, string? origin)
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "Tên nguyên liệu là bắt buộc." });
            if (string.IsNullOrWhiteSpace(physicalState)) return Json(new { success = false, message = "Kiểu nguyên liệu (Dạng đặc/lỏng) là bắt buộc." });
            if (string.IsNullOrWhiteSpace(materialKind)) return Json(new { success = false, message = "Kiểu nguyên liệu (Thô/Thành phẩm) là bắt buộc." });
            if (string.IsNullOrWhiteSpace(category)) return Json(new { success = false, message = "Loại nguyên liệu là bắt buộc." });
            if (price <= 0) return Json(new { success = false, message = "Giá mua hàng phải lớn hơn 0." });

            var trimmedName = name.Trim();
            var trimmedSupplier = supplier?.Trim() ?? string.Empty;

            var exists = await _context.Materials.AnyAsync(m => m.MaterialName.ToLower() == trimmedName.ToLower() && m.Supplier.ToLower() == trimmedSupplier.ToLower());
            if (exists)
            {
                return Json(new { success = false, message = "Lỗi trùng tên nguyên liệu cho nhà cung cấp này." });
            }

            var storageUnit = physicalState == "Dạng lỏng" ? "lít" : "kg";

            var lastMaterial = await _context.Materials
                .OrderByDescending(m => m.MaterialId)
                .FirstOrDefaultAsync();
            var nextNum = (lastMaterial?.MaterialId ?? 0) + 1;
            var code = $"NL{nextNum:D3}";

            var material = new Material
            {
                MaterialCode = code,
                MaterialName = trimmedName,
                PhysicalState = physicalState,
                MaterialKind = materialKind.Trim(),
                Category = category,
                UnitPrice = price,
                Supplier = trimmedSupplier,
                Origin = origin?.Trim(),
                StorageUnit = storageUnit,
                StockQuantity = 0
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMaterial(int id, string name, string physicalState, string materialKind, string category, decimal price, string? supplier, string? origin)
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "Tên nguyên liệu là bắt buộc." });
            if (string.IsNullOrWhiteSpace(physicalState)) return Json(new { success = false, message = "Kiểu nguyên liệu (Dạng đặc/lỏng) là bắt buộc." });
            if (string.IsNullOrWhiteSpace(materialKind)) return Json(new { success = false, message = "Kiểu nguyên liệu (Thô/Thành phẩm) là bắt buộc." });
            if (string.IsNullOrWhiteSpace(category)) return Json(new { success = false, message = "Loại nguyên liệu là bắt buộc." });
            if (price <= 0) return Json(new { success = false, message = "Giá mua hàng phải lớn hơn 0." });

            var material = await _context.Materials.FindAsync(id);
            if (material == null) return Json(new { success = false, message = "Nguyên liệu không tồn tại." });

            var trimmedName = name.Trim();
            var trimmedSupplier = supplier?.Trim() ?? string.Empty;

            var exists = await _context.Materials.AnyAsync(m =>
                m.MaterialName.ToLower() == trimmedName.ToLower() &&
                m.Supplier.ToLower() == trimmedSupplier.ToLower() &&
                m.MaterialId != id);

            if (exists)
            {
                return Json(new { success = false, message = "Lỗi trùng tên nguyên liệu cho nhà cung cấp này." });
            }

            material.MaterialName = trimmedName;
            material.PhysicalState = physicalState;
            material.MaterialKind = materialKind.Trim();
            material.Category = category;
            material.UnitPrice = price;
            material.Supplier = trimmedSupplier;
            material.Origin = origin?.Trim();
            material.StorageUnit = physicalState == "Dạng lỏng" ? "lít" : "kg";

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> ImportHistory(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var today = DateTime.Today;
            var defaultFrom = new DateTime(today.Year, today.Month, 1);
            var defaultTo = today;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                TempData["ImportError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
                return RedirectToAction("ImportHistory", new { fromDate = defaultFrom.ToString("yyyy-MM-dd"), toDate = defaultTo.ToString("yyyy-MM-dd") });
            }

            var query = _context.WarehouseReceiptItems
                .Include(i => i.WarehouseReceipt)
                .Include(i => i.Material)
                .Where(i => i.WarehouseReceipt != null && i.WarehouseReceipt.ImportDate.Date >= filterFrom.Date && i.WarehouseReceipt.ImportDate.Date <= filterTo.Date);

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var list = await query
                .OrderByDescending(i => i.WarehouseReceipt!.ImportDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new WarehouseImportHistoryViewModel
            {
                Items = list,
                FromDate = filterFrom,
                ToDate = filterTo,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportImportHistory(DateTime? fromDate, DateTime? toDate)
        {
            var today = DateTime.Today;
            var defaultFrom = new DateTime(today.Year, today.Month, 1);
            var defaultTo = today;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                return BadRequest("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            var query = _context.WarehouseReceiptItems
                .Include(i => i.WarehouseReceipt)
                .Include(i => i.Material)
                .AsQueryable();

            var toDateEnd = filterTo.AddDays(1).AddTicks(-1);
            query = query.Where(i => i.WarehouseReceipt != null &&
                                     i.WarehouseReceipt.ImportDate >= filterFrom &&
                                     i.WarehouseReceipt.ImportDate <= toDateEnd);

            var items = await query
                .OrderByDescending(i => i.WarehouseReceipt!.ImportDate)
                .ToListAsync();

            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("Mã phiếu nhập,Ngày nhập,Nhà cung cấp,Tên nguyên liệu,Số lượng nhập,Đơn vị,Đơn giá (VNĐ),Thành tiền (VNĐ),Người nhận,Người giao,SĐT người giao,Trạng thái");

            foreach (var item in items)
            {
                var r = item.WarehouseReceipt;
                var importTimeStr = r != null ? r.ImportDate.ToVietnamTimeString("dd/MM/yyyy HH:mm") : "";
                var line = $"\"{r?.ReceiptCode}\",\"{importTimeStr}\",\"{r?.Supplier}\",\"{item.Material?.MaterialName}\",{item.Quantity},\"{item.Material?.StorageUnit}\",{item.UnitPrice},{item.Amount},\"{r?.ReceiverName}\",\"{r?.DelivererName}\",\"{r?.DelivererPhone}\",\"{r?.Status}\"";
                csvBuilder.AppendLine(line);
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = $"BaoCaoNhapHang_{filterFrom:yyyyMMdd}_{filterTo:yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> CreateReceipt()
        {
            ViewBag.CurrentTime = DateTime.UtcNow.ToString("dd/MM/yyyy : HH\\hmm");
            var materials = await _context.Materials.OrderBy(m => m.MaterialName).ToListAsync();
            return View(materials);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReceipt([FromBody] ReceiptSubmitModel model)
        {
            if (model == null) return Json(new { success = false, message = "Dữ liệu trống." });
            if (string.IsNullOrWhiteSpace(model.DelivererName)) return Json(new { success = false, message = "Họ & Tên Người giao là bắt buộc." });
            var trimmedDeliverer = model.DelivererName.Trim();
            if (trimmedDeliverer.Length < 2 || !System.Text.RegularExpressions.Regex.IsMatch(trimmedDeliverer, @"^[\p{L}\s_]{2,100}$"))
            {
                return Json(new { success = false, message = "Tên người giao hàng chỉ bao gồm chữ cái, dấu gạch dưới và khoảng trắng (tối thiểu 2 ký tự, không chứa số hoặc ký tự đặc biệt khác)." });
            }

            if (string.IsNullOrWhiteSpace(model.ReceiverName)) return Json(new { success = false, message = "Vui lòng nhập tên người nhận hàng." });
            var trimmedReceiver = model.ReceiverName.Trim();
            if (trimmedReceiver.Length < 2 || !System.Text.RegularExpressions.Regex.IsMatch(trimmedReceiver, @"^[\p{L}\s_]{2,100}$"))
            {
                return Json(new { success = false, message = "Tên người nhận hàng chỉ bao gồm chữ cái, dấu gạch dưới và khoảng trắng (tối thiểu 2 ký tự, không chứa số hoặc ký tự đặc biệt khác)." });
            }
            if (model.Items == null || model.Items.Count == 0) return Json(new { success = false, message = "Phiếu nhập phải có nhất một nguyên liệu." });

            var materials = await _context.Materials.ToListAsync();
            string? supplier = null;

            foreach (var item in model.Items)
            {
                if (item.Quantity <= 0) return Json(new { success = false, message = "Số lượng phải là số dương." });
                if (item.UnitPrice <= 0) return Json(new { success = false, message = "Giá mua hàng phải lớn hơn 0." });

                var mat = materials.FirstOrDefault(m => m.MaterialId == item.MaterialId);
                if (mat == null) return Json(new { success = false, message = "Nguyên liệu không tồn tại." });

                if (supplier == null)
                {
                    supplier = mat.Supplier;
                }
                else if (supplier.ToLower() != mat.Supplier.ToLower())
                {
                    return Json(new { success = false, message = "Mỗi đơn chỉ nhập các nguyên liệu cùng nhà cung cấp." });
                }
            }

            var lastReceipt = await _context.WarehouseReceipts.OrderByDescending(r => r.ReceiptId).FirstOrDefaultAsync();
            var nextNum = (lastReceipt?.ReceiptId ?? 0) + 1;
            var code = $"PN-{nextNum:D3}";

            decimal totalAmount = model.Items.Sum(item => item.Quantity * item.UnitPrice);
            var creatorName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Warehouse Manager";

            var receipt = new WarehouseReceipt
            {
                ReceiptCode = code,
                ImportDate = DateTime.UtcNow,
                Supplier = supplier ?? "N/A",
                TotalAmount = totalAmount,
                Status = "Đã nhập kho",
                DelivererName = model.DelivererName.Trim(),
                DelivererPhone = model.DelivererPhone?.Trim(),
                ReceiverName = model.ReceiverName.Trim(),
                CreatedBy = creatorName
            };

            _context.WarehouseReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            foreach (var item in model.Items)
            {
                var mat = materials.First(m => m.MaterialId == item.MaterialId);
                mat.StockQuantity += item.Quantity;

                var receiptItem = new WarehouseReceiptItem
                {
                    ReceiptId = receipt.ReceiptId,
                    MaterialId = item.MaterialId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Quantity * item.UnitPrice
                };
                _context.WarehouseReceiptItems.Add(receiptItem);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, receiptCode = code });
        }

        [HttpGet]
        public async Task<IActionResult> GetReceiptDetails(string code)
        {
            var receipt = await _context.WarehouseReceipts
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.ReceiptCode == code);

            if (receipt == null) return NotFound();

            var itemsResult = receipt.Items.Select(i => new
            {
                materialName = i.Material?.MaterialName ?? "",
                materialKind = i.Material?.MaterialKind ?? "",
                category = i.Material?.Category ?? "",
                storageUnit = i.Material?.StorageUnit ?? "",
                supplier = i.Material?.Supplier ?? "",
                quantity = i.Quantity.ToString("G29"),
                unitPrice = i.UnitPrice.ToString("N0") + " đ",
                amount = i.Amount.ToString("N0") + " đ"
            });

            return Json(new
            {
                receiptCode = receipt.ReceiptCode,
                importDate = receipt.ImportDate.ToVietnamTimeString("dd/MM/yyyy HH:mm"),
                importDateRelative = receipt.ImportDate.ToRelativeTimeString(),
                supplier = receipt.Supplier,
                totalAmount = receipt.TotalAmount.ToString("N0") + " đ",
                delivererName = receipt.DelivererName,
                delivererPhone = receipt.DelivererPhone ?? "Không có",
                receiverName = receipt.ReceiverName,
                createdBy = receipt.CreatedBy,
                items = itemsResult
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExportRequests(
            DateTime? fromDate, DateTime? toDate, string status = "Tất cả", int page = 1, string sortBy = "expected_asc", string priority = "all", string? branchId = null)
        {
            var today = DateTime.Today;
            var defaultFrom = today.AddMonths(-3);
            var defaultTo = today;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                TempData["ExportError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
                return RedirectToAction("ExportRequests", new { fromDate = defaultFrom.ToString("yyyy-MM-dd"), toDate = defaultTo.ToString("yyyy-MM-dd"), status = status, sortBy = sortBy, priority = priority, branchId = branchId });
            }

            var viewModel = await _warehouseSupplyService.GetExportRequestsAsync(filterFrom, filterTo, status, page, 10, sortBy, priority, branchId);
            var branches = await _context.Branches.AsNoTracking().Where(b => b.Status == "Active").OrderBy(b => b.BranchName).ToListAsync();
            viewModel.Branches = branches;
            viewModel.SelectedBranchId = branchId;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportRequestsCsv(DateTime? fromDate, DateTime? toDate, string status, string sortBy = "expected_asc", string priority = "all", string? branchId = null)
        {
            var today = DateTime.Today;
            var defaultFrom = new DateTime(today.Year, today.Month, 1);
            var defaultTo = today;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            var query = _context.BranchSupplyRequests
                .Include(r => r.Branch)
                .Where(r => r.RequestDate.Date >= filterFrom.Date && r.RequestDate.Date <= filterTo.Date);

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(r => r.BranchId == branchId);
            }

            var list = await query.OrderByDescending(r => r.RequestDate).ToListAsync();

            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("Mã đơn,Ngày yêu cầu,Ngày cần nhận,Chi nhánh,Trạng thái,Ghi chú tổng kho,Người duyệt,Người giao,SĐT người giao,Đơn vị vận chuyển,Người nhận,SĐT người nhận,Kết quả đồng kiểm");

            foreach (var r in list)
            {
                var code = r.RequestCode;
                var reqDate = r.RequestDate.ToVietnamTimeString("dd/MM/yyyy HH:mm");
                var expDate = r.ExpectedDeliveryDate.HasValue ? r.ExpectedDeliveryDate.Value.ToString("dd/MM/yyyy") : "";
                var branchName = $"\"{r.Branch?.BranchName?.Replace("\"", "\"\"")}\"";
                var rStatus = r.Status;
                var note = $"\"{r.WarehouseNote?.Replace("\"", "\"\"")}\"";
                var approved = r.ApprovedBy ?? "";
                var deliverer = r.DelivererName ?? "";
                var phone = r.DelivererPhone ?? "";
                var provider = r.DeliveryProvider ?? "";
                var receiver = r.ReceiverName ?? "";
                var rPhone = r.ReceiverPhone ?? "";
                var insp = r.InspectionStatus ?? "";

                csvBuilder.AppendLine($"{code},{reqDate},{expDate},{branchName},{rStatus},{note},{approved},{deliverer},{phone},{provider},{receiver},{rPhone},{insp}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = $"YeuCauXuatKho_{filterFrom:yyyyMMdd}_{filterTo:yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchRequestDetails(string code)
        {
            var details = await _warehouseSupplyService.GetRequestDetailsJsonAsync(code);
            if (details == null) return NotFound();
            return Json(details);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReleasedQuantities([FromBody] UpdateReleasedQuantitiesModel model)
        {
            if (model == null) return Json(new { success = false, message = "Dữ liệu trống." });

            var result = await _warehouseSupplyService.UpdateReleasedQuantitiesAsync(model.RequestCode, model.Items, model.WarehouseNote);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequestAndPrepare(string code)
        {
            var approverName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Warehouse Manager";
            var result = await _warehouseSupplyService.ApproveAndPrepareShipmentAsync(code, approverName);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmShipment(string code, string? delivererName, string? delivererPhone, string? deliveryProvider)
        {
            var result = await _warehouseSupplyService.ConfirmShipmentAsync(code, delivererName, delivererPhone, deliveryProvider);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(string code, string? reason)
        {
            var approverName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Warehouse Manager";
            var result = await _warehouseSupplyService.RejectSupplyRequestAsync(code, approverName, reason);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Statistics(
            int? materialId, string? category, string? search, string? range = "month", DateTime? fromDate = null, DateTime? toDate = null, string? branchId = null)
        {
            var today = DateTime.Today;
            DateTime start;
            DateTime end;

            switch (range?.ToLower())
            {
                case "7days":
                    start = today.AddDays(-6);
                    end = today;
                    break;
                case "quarter":
                    int currentQuarter = (today.Month - 1) / 3;
                    start = new DateTime(today.Year, currentQuarter * 3 + 1, 1);
                    end = today;
                    break;
                case "year":
                    start = new DateTime(today.Year, 1, 1);
                    end = today;
                    break;
                case "custom":
                    start = fromDate ?? today.AddMonths(-1);
                    end = toDate ?? today;
                    break;
                case "month":
                default:
                    range = "month";
                    start = fromDate ?? new DateTime(today.Year, today.Month, 1);
                    end = toDate ?? today;
                    break;
            }

            if (end < start)
            {
                (start, end) = (end, start);
            }

            var startUtc = start.Date;
            var endUtc = end.Date.AddDays(1).AddTicks(-1);

            // 1. Fetch materials list for dropdown and table
            var allMaterials = await _context.Materials.AsNoTracking().OrderBy(m => m.MaterialName).ToListAsync();
            var categories = allMaterials.Select(m => m.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c).ToList();

            var filteredMaterials = allMaterials.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(category) && category != "Tất cả")
            {
                filteredMaterials = filteredMaterials.Where(m => m.Category == category);
            }
            if (materialId.HasValue && materialId.Value > 0)
            {
                filteredMaterials = filteredMaterials.Where(m => m.MaterialId == materialId.Value);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                filteredMaterials = filteredMaterials.Where(m => (m.MaterialName ?? "").ToLower().Contains(term) || (m.MaterialCode ?? "").ToLower().Contains(term));
            }
            var targetMaterialsList = filteredMaterials.ToList();

            // 2. Fetch Import Items in range
            var importQuery = _context.WarehouseReceiptItems.AsNoTracking()
                .Include(i => i.WarehouseReceipt)
                .Where(i => i.WarehouseReceipt != null && i.WarehouseReceipt.ImportDate >= startUtc && i.WarehouseReceipt.ImportDate <= endUtc);

            if (materialId.HasValue && materialId.Value > 0)
            {
                importQuery = importQuery.Where(i => i.MaterialId == materialId.Value);
            }
            var importItems = await importQuery.ToListAsync();

            // 3. Fetch Export Items in range (Approved/Shipped/Completed)
            var validExportStatuses = new[] { "Đã xuất kho", "Đã hoàn thành", "Đã nhận - Có hàng lỗi" };
            var exportQuery = _context.BranchSupplyRequestItems.AsNoTracking()
                .Include(i => i.BranchSupplyRequest)
                .ThenInclude(r => r.Branch)
                .Include(i => i.Material)
                .Where(i => i.BranchSupplyRequest != null && validExportStatuses.Contains(i.BranchSupplyRequest.Status)
                    && i.BranchSupplyRequest.RequestDate >= startUtc && i.BranchSupplyRequest.RequestDate <= endUtc);

            if (materialId.HasValue && materialId.Value > 0)
            {
                exportQuery = exportQuery.Where(i => i.MaterialId == materialId.Value);
            }
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                exportQuery = exportQuery.Where(i => i.BranchSupplyRequest!.BranchId == branchId);
            }
            var exportItems = await exportQuery.ToListAsync();

            // 4. Build Detailed Rows
            var statRows = new List<MaterialStatRowDto>();
            foreach (var mat in targetMaterialsList)
            {
                var matImports = importItems.Where(i => i.MaterialId == mat.MaterialId).ToList();
                var matExports = exportItems.Where(i => i.MaterialId == mat.MaterialId).ToList();

                decimal impQty = matImports.Sum(i => i.Quantity);
                decimal impCost = matImports.Sum(i => i.Amount);
                decimal expQty = matExports.Sum(i => i.QuantityReleased ?? i.QuantityRequested);
                decimal expValue = expQty * mat.UnitPrice;

                statRows.Add(new MaterialStatRowDto
                {
                    MaterialId = mat.MaterialId,
                    MaterialCode = mat.MaterialCode,
                    MaterialName = mat.MaterialName,
                    Category = mat.Category,
                    StorageUnit = mat.StorageUnit,
                    CurrentStock = mat.StockQuantity,
                    UnitPrice = mat.UnitPrice,
                    ImportQuantity = impQty,
                    ImportCost = impCost,
                    ExportQuantity = expQty,
                    ExportEstimatedValue = expValue
                });
            }

            // 5. Chart 1: Bar Chart (So sánh Nhập vs Xuất theo Top 7 nguyên liệu)
            var barChart = new ChartComparisonDto();
            var topActive = statRows.OrderByDescending(r => r.ImportQuantity + r.ExportQuantity).Take(7).ToList();
            foreach (var r in topActive)
            {
                barChart.Labels.Add(r.MaterialName);
                barChart.ImportQuantities.Add(r.ImportQuantity);
                barChart.ExportQuantities.Add(r.ExportQuantity);
            }

            // 6. Chart 2: Line Chart (Xu hướng dòng thời gian - Timeline trend)
            var lineChart = new ChartTimelineDto();
            var dayCount = (end.Date - start.Date).Days + 1;
            if (dayCount <= 31)
            {
                for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
                {
                    var dayImports = importItems.Where(i => i.WarehouseReceipt?.ImportDate.Date == d).Sum(i => i.Quantity);
                    var dayExports = exportItems.Where(i => i.BranchSupplyRequest?.RequestDate.Date == d).Sum(i => i.QuantityReleased ?? i.QuantityRequested);
                    lineChart.Dates.Add(d.ToString("dd/MM"));
                    lineChart.ImportQuantities.Add(dayImports);
                    lineChart.ExportQuantities.Add(dayExports);
                }
            }
            else
            {
                var currentPeriod = start.Date;
                while (currentPeriod <= end.Date)
                {
                    var periodEnd = currentPeriod.AddDays(6);
                    if (periodEnd > end.Date) periodEnd = end.Date;

                    var periodImports = importItems.Where(i => i.WarehouseReceipt != null && i.WarehouseReceipt.ImportDate.Date >= currentPeriod && i.WarehouseReceipt.ImportDate.Date <= periodEnd).Sum(i => i.Quantity);
                    var periodExports = exportItems.Where(i => i.BranchSupplyRequest != null && i.BranchSupplyRequest.RequestDate.Date >= currentPeriod && i.BranchSupplyRequest.RequestDate.Date <= periodEnd).Sum(i => i.QuantityReleased ?? i.QuantityRequested);

                    lineChart.Dates.Add($"{currentPeriod:dd/MM}-{periodEnd:dd/MM}");
                    lineChart.ImportQuantities.Add(periodImports);
                    lineChart.ExportQuantities.Add(periodExports);

                    currentPeriod = periodEnd.AddDays(1);
                }
            }

            // 7. Chart 3: Horizontal Bar Chart (Top 8 nguyên liệu xuất kho nhiều nhất)
            var horizontalChart = new ChartRankDto();
            var topExported = statRows.Where(r => r.ExportQuantity > 0).OrderByDescending(r => r.ExportQuantity).Take(8).ToList();
            if (!topExported.Any())
            {
                topExported = statRows.OrderByDescending(r => r.ImportQuantity).Take(8).ToList();
            }
            foreach (var r in topExported)
            {
                horizontalChart.Labels.Add(r.MaterialName);
                horizontalChart.Quantities.Add(r.ExportQuantity > 0 ? r.ExportQuantity : r.ImportQuantity);
                horizontalChart.Units.Add(r.StorageUnit);
            }

            var vm = new WarehouseStatisticsViewModel
            {
                FromDate = start,
                ToDate = end,
                SelectedRange = range,
                SearchTerm = search,
                SelectedCategory = category,
                SelectedMaterialId = materialId,
                TotalImportQuantity = importItems.Sum(i => i.Quantity),
                TotalImportCost = importItems.Sum(i => i.Amount),
                TotalExportQuantity = exportItems.Sum(i => i.QuantityReleased ?? i.QuantityRequested),
                TotalExportEstimatedValue = exportItems.Sum(i => (i.QuantityReleased ?? i.QuantityRequested) * (i.Material?.UnitPrice ?? 0)),
                TotalImportReceiptsCount = importItems.Select(i => i.ReceiptId).Distinct().Count(),
                TotalExportRequestsCount = exportItems.Select(i => i.RequestId).Distinct().Count(),
                TotalMaterialsCount = allMaterials.Count,
                LowStockCount = allMaterials.Count(m => m.StockQuantity <= 10),
                Categories = categories,
                MaterialOptions = allMaterials.Select(m => new MaterialSelectOptionDto
                {
                    MaterialId = m.MaterialId,
                    MaterialCode = m.MaterialCode,
                    MaterialName = m.MaterialName,
                    Category = m.Category
                }).ToList(),
                MaterialStats = statRows,
                BarChartData = barChart,
                LineChartData = lineChart,
                HorizontalBarChartData = horizontalChart,
                Branches = await _context.Branches.AsNoTracking().Where(b => b.Status == "Active").OrderBy(b => b.BranchName).ToListAsync(),
                SelectedBranchId = branchId
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticsCsv(
            int? materialId, string? category, string? search, string? range = "month", DateTime? fromDate = null, DateTime? toDate = null, string? branchId = null)
        {
            var today = DateTime.Today;
            DateTime start = fromDate ?? new DateTime(today.Year, today.Month, 1);
            DateTime end = toDate ?? today;

            if (range == "7days") { start = today.AddDays(-6); end = today; }
            else if (range == "quarter") { start = new DateTime(today.Year, ((today.Month - 1) / 3) * 3 + 1, 1); end = today; }
            else if (range == "year") { start = new DateTime(today.Year, 1, 1); end = today; }

            if (end < start) (start, end) = (end, start);
            var startUtc = start.Date;
            var endUtc = end.Date.AddDays(1).AddTicks(-1);

            var materials = await _context.Materials.AsNoTracking().OrderBy(m => m.MaterialName).ToListAsync();
            var filtered = materials.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(category) && category != "Tất cả")
                filtered = filtered.Where(m => m.Category == category);
            if (materialId.HasValue && materialId.Value > 0)
                filtered = filtered.Where(m => m.MaterialId == materialId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                filtered = filtered.Where(m => (m.MaterialName ?? "").ToLower().Contains(term) || (m.MaterialCode ?? "").ToLower().Contains(term));
            }
            var targetMaterials = filtered.ToList();

            var importItems = await _context.WarehouseReceiptItems.AsNoTracking()
                .Include(i => i.WarehouseReceipt)
                .Where(i => i.WarehouseReceipt != null && i.WarehouseReceipt.ImportDate >= startUtc && i.WarehouseReceipt.ImportDate <= endUtc)
                .ToListAsync();

            var validExportStatuses = new[] { "Đã xuất kho", "Đã hoàn thành", "Đã nhận - Có hàng lỗi" };
            var exportItemsQuery = _context.BranchSupplyRequestItems.AsNoTracking()
                .Include(i => i.BranchSupplyRequest)
                .Where(i => i.BranchSupplyRequest != null && validExportStatuses.Contains(i.BranchSupplyRequest.Status)
                    && i.BranchSupplyRequest.RequestDate >= startUtc && i.BranchSupplyRequest.RequestDate <= endUtc);

            if (!string.IsNullOrWhiteSpace(branchId))
            {
                exportItemsQuery = exportItemsQuery.Where(i => i.BranchSupplyRequest!.BranchId == branchId);
            }
            var exportItems = await exportItemsQuery.ToListAsync();

            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("Mã nguyên liệu,Tên nguyên liệu,Loại,Đơn vị lưu kho,Đơn giá (VNĐ),Lượng nhập trong kỳ,Chi phí nhập (VNĐ),Lượng xuất trong kỳ,Giá trị xuất ước tính (VNĐ),Biến động ròng,Tồn kho hiện tại");

            foreach (var mat in targetMaterials)
            {
                var impQty = importItems.Where(i => i.MaterialId == mat.MaterialId).Sum(i => i.Quantity);
                var impCost = importItems.Where(i => i.MaterialId == mat.MaterialId).Sum(i => i.Amount);
                var expQty = exportItems.Where(i => i.MaterialId == mat.MaterialId).Sum(i => i.QuantityReleased ?? i.QuantityRequested);
                var expVal = expQty * mat.UnitPrice;
                var net = impQty - expQty;

                var nameEscaped = $"\"{mat.MaterialName.Replace("\"", "\"\"")}\"";
                var catEscaped = $"\"{mat.Category?.Replace("\"", "\"\"")}\"";

                csvBuilder.AppendLine($"\"{mat.MaterialCode}\",{nameEscaped},{catEscaped},\"{mat.StorageUnit}\",{mat.UnitPrice},{impQty},{impCost},{expQty},{expVal},{net},{mat.StockQuantity}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = $"ThongKe_XuatNhapKho_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
    }

    public class ReceiptSubmitModel
    {
        public string DelivererName { get; set; } = string.Empty;
        public string? DelivererPhone { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public List<ReceiptItemSubmitModel> Items { get; set; } = new();
    }

    public class ReceiptItemSubmitModel
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class WarehouseIndexViewModel
    {
        public List<Material> Materials { get; set; } = new();
        public string? SearchString { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedKind { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<string> Kinds { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class WarehouseImportHistoryViewModel
    {
        public List<WarehouseReceiptItem> Items { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
