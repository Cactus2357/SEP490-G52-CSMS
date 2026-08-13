using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Controllers
{
    [Authorize(Roles = "WarehouseManager,RManager")]
    public class WarehouseController : Controller
    {
        private readonly CSMSAppDbContext _context;
        private readonly INotificationService _notificationService;

        public WarehouseController(CSMSAppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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
                TempData["HistoryError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
            }

            var query = _context.WarehouseReceiptItems
                .Include(i => i.WarehouseReceipt)
                .Include(i => i.Material)
                .AsQueryable();

            if (filterTo >= filterFrom)
            {
                var toDateEnd = filterTo.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.WarehouseReceipt != null &&
                                         i.WarehouseReceipt.ImportDate >= filterFrom &&
                                         i.WarehouseReceipt.ImportDate <= toDateEnd);
            }

            const int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = await query
                .OrderByDescending(i => i.WarehouseReceipt!.ImportDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new WarehouseImportHistoryViewModel
            {
                Items = items,
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

            var toDateEnd = filterTo.AddDays(1).AddTicks(-1);
            var items = await _context.WarehouseReceiptItems
                .Include(i => i.WarehouseReceipt)
                .Include(i => i.Material)
                .Where(i => i.WarehouseReceipt != null &&
                             i.WarehouseReceipt.ImportDate >= filterFrom &&
                             i.WarehouseReceipt.ImportDate <= toDateEnd)
                .OrderByDescending(i => i.WarehouseReceipt!.ImportDate)
                .ToListAsync();

            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("Mã phiếu,Ngày nhập,Nhà cung cấp,Nguyên liệu,Số lượng nhập,Đơn vị,Tổng tiền");

            foreach (var item in items)
            {
                var receiptCode = item.WarehouseReceipt?.ReceiptCode ?? "";
                var importDate = item.WarehouseReceipt?.ImportDate.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                var supplier = item.WarehouseReceipt?.Supplier ?? "";
                var materialName = item.Material?.MaterialName ?? "";
                var qty = item.Quantity.ToString("G29");
                var unit = item.Material?.StorageUnit ?? "";
                var amount = item.Amount.ToString("F0");

                supplier = $"\"{supplier.Replace("\"", "\"\"")}\"";
                materialName = $"\"{materialName.Replace("\"", "\"\"")}\"";

                csvBuilder.AppendLine($"{receiptCode},{importDate},{supplier},{materialName},{qty},{unit},{amount}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = $"BaoCaoNhapHang_{filterFrom:yyyyMMdd}_{filterTo:yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> CreateReceipt()
        {
            ViewBag.CurrentTime = DateTime.Now.ToString("dd/MM/yyyy : HH\\hmm");
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
                ImportDate = DateTime.Now,
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
            var now = DateTime.Now;
            var defaultFrom = now.Date.AddMonths(-3);
            var defaultTo = now.Date;

            var filterFrom = fromDate ?? defaultFrom;
            var filterTo = toDate ?? defaultTo;

            if (filterTo < filterFrom)
            {
                TempData["ExportError"] = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";
                return RedirectToAction("ExportRequests", new { fromDate = defaultFrom.ToString("yyyy-MM-dd"), toDate = defaultTo.ToString("yyyy-MM-dd"), status = status });
            }

            var query = _context.BranchSupplyRequests
                .Include(r => r.Branch)
                .Where(r => r.RequestDate.Date >= filterFrom.Date && r.RequestDate.Date <= filterTo.Date);

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
                .OrderByDescending(r => r.Status == "Chờ duyệt" ? 1 : 0)
                .ThenByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ExportRequestsViewModel
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

        [HttpGet]
        public async Task<IActionResult> ExportRequestsCsv(DateTime? fromDate, DateTime? toDate, string status)
        {
            var now = DateTime.Now;
            var defaultFrom = new DateTime(now.Year, now.Month, 1);
            var defaultTo = now.Date;

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
            csvBuilder.AppendLine("Mã đơn,Ngày yêu cầu,Chi nhánh,Trạng thái,Ghi chú tổng kho,Người duyệt,Người giao,Sđt");

            foreach (var r in list)
            {
                var code = r.RequestCode;
                var reqDate = r.RequestDate.ToString("dd/MM/yyyy HH:mm");
                var branchName = $"\"{r.Branch?.BranchName?.Replace("\"", "\"\"")}\"";
                var rStatus = r.Status;
                var note = $"\"{r.WarehouseNote?.Replace("\"", "\"\"")}\"";
                var approved = r.ApprovedBy ?? "";
                var deliverer = r.DelivererName ?? "";
                var phone = r.DelivererPhone ?? "";

                csvBuilder.AppendLine($"{code},{reqDate},{branchName},{rStatus},{note},{approved},{deliverer},{phone}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
            var fileName = $"YeuCauXuatKho_{filterFrom:yyyyMMdd}_{filterTo:yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchRequestDetails(string code)
        {
            var req = await _context.BranchSupplyRequests
                .Include(r => r.Branch)
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.RequestCode == code);

            if (req == null) return NotFound();

            var bManager = await _context.Employees
                .FirstOrDefaultAsync(e => e.BranchId == req.BranchId && (e.Role == "BranchManager" || e.Role == "RManager"));

            var itemsResult = req.Items.Select(i => new
            {
                materialId = i.MaterialId,
                materialName = i.Material?.MaterialName ?? "",
                category = i.Material?.Category ?? "",
                materialKind = i.Material?.MaterialKind ?? "",
                storageUnit = i.Material?.StorageUnit ?? "",
                quantityRequested = i.QuantityRequested.ToString("G29"),
                quantityReleased = i.QuantityReleased.HasValue ? i.QuantityReleased.Value.ToString("G29") : "",
                centralStock = i.Material?.StockQuantity.ToString("G29") ?? "0"
            });

            return Json(new
            {
                requestCode = req.RequestCode,
                status = req.Status,
                requestDate = req.RequestDate.ToString("dd/MM/yyyy HH:mm"),
                branchName = req.Branch?.BranchName ?? "",
                creatorName = bManager?.FullName ?? "Quản lý chi nhánh",
                creatorPhone = bManager?.PhoneNumber ?? "Không có",
                approvedBy = req.ApprovedBy ?? "",
                delivererName = req.DelivererName ?? "",
                delivererPhone = req.DelivererPhone ?? "",
                warehouseNote = req.WarehouseNote ?? "",
                items = itemsResult
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReleasedQuantities([FromBody] UpdateReleasedQuantitiesModel model)
        {
            if (model == null) return Json(new { success = false, message = "Dữ liệu trống." });

            var req = await _context.BranchSupplyRequests
                .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.RequestCode == model.RequestCode);

            if (req == null) return Json(new { success = false, message = "Đơn hàng không tồn tại." });
            if (req.Status != "Chờ duyệt") return Json(new { success = false, message = "Chỉ có thể cập nhật số lượng khi đơn ở trạng thái Chờ duyệt." });

            foreach (var item in model.Items)
            {
                var reqItem = req.Items.FirstOrDefault(i => i.MaterialId == item.MaterialId);
                if (reqItem != null && reqItem.Material != null)
                {
                    if (item.QuantityReleased > reqItem.Material.StockQuantity)
                    {
                        return Json(new { success = false, message = $"Số lượng thực xuất của \"{reqItem.Material.MaterialName}\" vượt quá tồn kho hiện có ({reqItem.Material.StockQuantity.ToString("G29")} {reqItem.Material.StorageUnit})." });
                    }
                    reqItem.QuantityReleased = item.QuantityReleased;
                }
            }

            if (!string.IsNullOrWhiteSpace(model.WarehouseNote))
            {
                req.WarehouseNote = model.WarehouseNote.Trim();
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật số lượng thực xuất thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequestAndPrepare(string code)
        {
            var req = await _context.BranchSupplyRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.RequestCode == code);

            if (req == null) return Json(new { success = false, message = "Đơn hàng không tồn tại." });
            if (req.Status != "Chờ duyệt") return Json(new { success = false, message = "Trạng thái không hợp lệ." });

            bool hasReleasedValues = req.Items.Any(i => i.QuantityReleased.HasValue);
            if (!hasReleasedValues)
            {
                return Json(new { success = false, message = "Vui lòng so sánh tồn kho và nhập số lượng thực xuất trước khi duyệt đơn." });
            }

            req.Status = "Đang chuẩn bị xuất";
            req.ApprovedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Warehouse Manager";
            req.ApprovedDate = DateTime.Now;

            var bManager = await _context.Employees
                .FirstOrDefaultAsync(e => e.BranchId == req.BranchId && e.Role == "BranchManager");

            await _notificationService.SendAsync(new NotificationEvent(
                Title: "Yêu cầu nhập kho đã duyệt",
                Message: $"Đơn yêu cầu {req.RequestCode} của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.",
                RecipientUserId: bManager?.EmployeeId,
                RecipientRole: "BranchManager",
                ResourceUrl: "/BranchWarehouse/RequestHistory"
            ));

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã duyệt đơn và chuyển sang trạng thái Chuẩn bị xuất." });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmShipment(string code)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var req = await _context.BranchSupplyRequests
                    .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                    .FirstOrDefaultAsync(r => r.RequestCode == code);

                if (req == null) return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                if (req.Status != "Đang chuẩn bị xuất") return Json(new { success = false, message = "Trạng thái không hợp lệ." });

                foreach (var item in req.Items)
                {
                    var releasedQty = item.QuantityReleased ?? 0;
                    if (releasedQty > 0 && item.Material != null)
                    {
                        item.Material.StockQuantity -= releasedQty;
                        if (item.Material.StockQuantity < 0) item.Material.StockQuantity = 0;
                    }
                }

                req.Status = "Đã xuất kho";
                if (string.IsNullOrWhiteSpace(req.WarehouseNote))
                {
                    req.WarehouseNote = "Đang giao hàng";
                }

                var bManager = await _context.Employees
                    .FirstOrDefaultAsync(e => e.BranchId == req.BranchId && e.Role == "BranchManager");

                await _notificationService.SendAsync(new NotificationEvent(
                    Title: "Đơn hàng đang giao",
                    Message: $"Đơn yêu cầu {req.RequestCode} đã được xuất kho và đang trên đường giao tới chi nhánh.",
                    RecipientUserId: bManager?.EmployeeId,
                    RecipientRole: "BranchManager",
                    ResourceUrl: "/BranchWarehouse/RequestHistory"
                ));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Xác nhận đã xuất kho thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống khi xuất kho: " + ex.Message });
            }
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

    public class UpdateReleasedQuantitiesModel
    {
        public string RequestCode { get; set; } = string.Empty;
        public string? WarehouseNote { get; set; }
        public List<RequestItemReleasedModel> Items { get; set; } = new();
    }

    public class RequestItemReleasedModel
    {
        public int MaterialId { get; set; }
        public decimal QuantityReleased { get; set; }
    }

    public class ExportRequestsViewModel
    {
        public List<BranchSupplyRequest> Requests { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SelectedStatus { get; set; } = "Tất cả";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
