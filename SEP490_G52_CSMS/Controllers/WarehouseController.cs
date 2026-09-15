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
                query = query.Where(m => EF.Functions.Collate(m.MaterialName, "SQL_Latin1_General_CP1_CI_AI").Contains(searchString));
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
                var line = $"\"{r?.ReceiptCode}\",\"{r?.ImportDate:dd/MM/yyyy HH:mm}\",\"{r?.Supplier}\",\"{item.Material?.MaterialName}\",{item.Quantity},\"{item.Material?.StorageUnit}\",{item.UnitPrice},{item.Amount},\"{r?.ReceiverName}\",\"{r?.DelivererName}\",\"{r?.DelivererPhone}\",\"{r?.Status}\"";
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
            if (string.IsNullOrWhiteSpace(model.ReceiverName)) return Json(new { success = false, message = "Vui lòng nhập tên người nhận hàng." });
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
                importDate = receipt.ImportDate.ToString("dd/MM/yyyy : HH\\hmm"),
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
        public async Task<IActionResult> ExportRequests(DateTime? fromDate, DateTime? toDate, string status = "Tất cả", int page = 1)
        {
            var today = DateTime.Today;
            var defaultFrom = today.AddMonths(-3);
            var defaultTo = today;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                TempData["ExportError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
                return RedirectToAction("ExportRequests", new { fromDate = defaultFrom.ToString("yyyy-MM-dd"), toDate = defaultTo.ToString("yyyy-MM-dd"), status = status });
            }

            var viewModel = await _warehouseSupplyService.GetExportRequestsAsync(filterFrom, filterTo, status, page, 10);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportRequestsCsv(DateTime? fromDate, DateTime? toDate, string status)
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
