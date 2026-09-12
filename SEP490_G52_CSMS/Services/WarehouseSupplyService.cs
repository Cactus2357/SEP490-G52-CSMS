using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_G52_CSMS.Models.Sales;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Services
{
    public class WarehouseSupplyService : IWarehouseSupplyService
    {
        private readonly IWarehouseSupplyRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<WarehouseSupplyService> _logger;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public WarehouseSupplyService(
            IWarehouseSupplyRepository repository,
            INotificationService notificationService,
            IFileStorageService fileStorageService,
            ILogger<WarehouseSupplyService> logger)
        {
            _repository = repository;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // =========================================================================
        // 1. BRANCH WAREHOUSE OPERATIONS
        // =========================================================================

        public async Task<(bool Success, string Message, string? RequestCode)> CreateSupplyRequestAsync(
            string branchId, string branchName, SupplyRequestSubmitModel model, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(branchId))
            {
                return (false, "Lỗi không xác định được chi nhánh.", null);
            }

            if (model == null || model.Items == null || !model.Items.Any())
            {
                return (false, "Đơn hàng phải có ít nhất một nguyên liệu.", null);
            }

            if (string.IsNullOrWhiteSpace(model.ReceiverName))
            {
                return (false, "Vui lòng nhập họ tên người nhận hàng.", null);
            }

            if (string.IsNullOrWhiteSpace(model.ReceiverPhone))
            {
                return (false, "Vui lòng nhập số điện thoại người nhận hàng.", null);
            }

            if (model.ExpectedDeliveryDate.HasValue && model.ExpectedDeliveryDate.Value.Date < DateTime.Today)
            {
                return (false, "Ngày mong muốn nhận hàng không được ở quá khứ.", null);
            }

            foreach (var item in model.Items)
            {
                if (item.Quantity <= 0)
                {
                    return (false, "Số lượng nguyên liệu yêu cầu phải lớn hơn 0.", null);
                }
            }

            try
            {
                var requestCode = await _repository.GenerateNextRequestCodeAsync();

                var request = new BranchSupplyRequest
                {
                    RequestCode = requestCode,
                    BranchId = branchId,
                    RequestDate = DateTime.UtcNow,
                    Status = "Chờ duyệt",
                    ReceiverName = model.ReceiverName.Trim(),
                    ReceiverPhone = model.ReceiverPhone.Trim(),
                    ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                    RequestNote = model.RequestNote?.Trim(),
                    Items = model.Items.Select(i => new BranchSupplyRequestItem
                    {
                        MaterialId = i.MaterialId,
                        QuantityRequested = i.Quantity,
                        QuantityReleased = null
                    }).ToList()
                };

                await _repository.AddRequestAsync(request);
                await _repository.SaveChangesAsync();

                // Send notification to Warehouse Manager
                try
                {
                    await _notificationService.SendAsync(new NotificationEvent(
                        Title: "Yêu cầu nhập hàng mới",
                        Message: $"Chi nhánh {branchName} đã gửi đơn yêu cầu nhập hàng mới ({requestCode}) với {request.Items.Count} nguyên liệu.",
                        RecipientRole: "WarehouseManager",
                        ResourceUrl: "/Warehouse/ExportRequests",
                        BranchId: branchId
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send notification for new supply request {Code}", requestCode);
                }

                return (true, "Tạo đơn yêu cầu nhập hàng thành công!", requestCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supply request for branch {BranchId}", branchId);
                return (false, "Lỗi hệ thống khi tạo đơn yêu cầu: " + ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> CancelSupplyRequestAsync(string requestCode, string branchId)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode, branchId);
            if (req == null)
            {
                return (false, "Đơn yêu cầu không tồn tại.");
            }

            if (req.Status != "Chờ duyệt")
            {
                return (false, "Chỉ có thể hủy đơn khi đang ở trạng thái Chờ duyệt.");
            }

            req.Status = "Đã hủy";
            await _repository.SaveChangesAsync();

            return (true, "Đã hủy đơn yêu cầu nhập kho thành công.");
        }

        public async Task<(bool Success, string Message, bool HasDefects)> InspectAndReceiveGoodsAsync(
            string branchId, string branchName, GoodsInspectionSubmitDto model, string currentUserName)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RequestCode))
            {
                return (false, "Dữ liệu biên bản đồng kiểm không hợp lệ.", false);
            }

            if (string.IsNullOrWhiteSpace(model.InspectorName))
            {
                return (false, "Vui lòng nhập họ tên người đồng kiểm.", false);
            }

            var req = await _repository.GetRequestByCodeAsync(model.RequestCode, branchId);
            if (req == null)
            {
                return (false, "Đơn yêu cầu không tồn tại.", false);
            }

            if (req.Status != "Đã xuất kho")
            {
                return (false, "Trạng thái đơn hàng không hợp lệ để đồng kiểm nhận hàng (Đơn phải ở trạng thái Đã xuất kho).", false);
            }

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                decimal totalDefective = 0;
                var defectSummaries = new List<string>();

                for (int i = 0; i < model.Items.Count; i++)
                {
                    var itemInput = model.Items[i];
                    var reqItem = req.Items.FirstOrDefault(ri => ri.MaterialId == itemInput.MaterialId);
                    if (reqItem == null) continue;

                    if (itemInput.QuantityAccepted < 0 || itemInput.QuantityDefective < 0)
                    {
                        return (false, $"Số lượng kiểm đếm của \"{reqItem.Material?.MaterialName}\" không được âm.", false);
                    }

                    decimal releasedQty = reqItem.QuantityReleased ?? reqItem.QuantityRequested;
                    decimal totalChecked = itemInput.QuantityAccepted + itemInput.QuantityDefective;
                    if (totalChecked > releasedQty)
                    {
                        return (false, $"Tổng số lượng nhận ({totalChecked}) của \"{reqItem.Material?.MaterialName}\" không được vượt quá số lượng kho tổng đã xuất ({releasedQty}).", false);
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
                        return (false, $"Nguyên liệu \"{reqItem.Material?.MaterialName}\" chỉ được tải lên tối đa 5 ảnh bằng chứng.", false);
                    }

                    var uploadedUrls = new List<string>();
                    foreach (var file in filesToUpload)
                    {
                        if (file == null || file.Length == 0) continue;
                        if (file.Length > MaxFileSizeBytes)
                        {
                            return (false, $"Ảnh \"{file.FileName}\" vượt quá dung lượng tối đa 5MB.", false);
                        }
                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (!AllowedImageExtensions.Contains(ext))
                        {
                            return (false, $"Ảnh \"{file.FileName}\" không đúng định dạng cho phép (.jpg, .jpeg, .png, .webp).", false);
                        }

                        var uploadedUrl = await _fileStorageService.UploadFileAsync(file, "supply_defects");
                        if (!string.IsNullOrEmpty(uploadedUrl))
                        {
                            uploadedUrls.Add(uploadedUrl);
                        }
                    }

                    if (uploadedUrls.Count > 0)
                    {
                        reqItem.DefectImageUrl = JsonSerializer.Serialize(uploadedUrls);
                    }

                    if (itemInput.QuantityDefective > 0)
                    {
                        totalDefective += itemInput.QuantityDefective;
                        defectSummaries.Add($"{reqItem.Material?.MaterialName} ({itemInput.QuantityDefective} {reqItem.Material?.StorageUnit} - {reqItem.DefectType})");
                    }

                    // CRITICAL: ONLY accepted quantity is added to branch inventory!
                    if (itemInput.QuantityAccepted > 0)
                    {
                        var branchInv = await _repository.GetBranchInventoryAsync(branchId, itemInput.MaterialId);
                        if (branchInv == null)
                        {
                            branchInv = new BranchInventory
                            {
                                BranchId = branchId,
                                MaterialId = itemInput.MaterialId,
                                StockQuantity = itemInput.QuantityAccepted,
                                LowStockThreshold = 10m
                            };
                            await _repository.AddBranchInventoryAsync(branchInv);
                        }
                        else
                        {
                            branchInv.StockQuantity += itemInput.QuantityAccepted;
                        }
                    }
                }

                req.InspectedBy = model.InspectorName.Trim();
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

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                string responseMsg = totalDefective > 0
                    ? $"Đã hoàn tất đồng kiểm! Hệ thống đã ghi nhận biên bản {totalDefective} nguyên liệu lỗi và chỉ cộng số lượng đạt chuẩn vào kho chi nhánh."
                    : "Đồng kiểm thành công! Toàn bộ nguyên liệu đạt chuẩn đã được nhập vào kho chi nhánh.";

                return (true, responseMsg, totalDefective > 0);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during goods inspection for request {Code}", model.RequestCode);
                return (false, "Lỗi hệ thống khi đồng kiểm nhận hàng: " + ex.Message, false);
            }
        }

        public async Task<(bool Success, string Message, bool HasDefects)> ConfirmReceiptLegacyAsync(
            string requestCode, string branchId, string branchName, string delivererName, string delivererPhone)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode, branchId);
            if (req == null) return (false, "Đơn yêu cầu không tồn tại.", false);

            var model = new GoodsInspectionSubmitDto
            {
                RequestCode = requestCode,
                InspectorName = !string.IsNullOrWhiteSpace(delivererName) ? delivererName : "BManager",
                Items = req.Items.Select(i => new GoodsInspectionItemSubmitDto
                {
                    MaterialId = i.MaterialId,
                    QuantityReceived = i.QuantityReleased ?? i.QuantityRequested,
                    QuantityAccepted = i.QuantityReleased ?? i.QuantityRequested,
                    QuantityDefective = 0
                }).ToList()
            };

            return await InspectAndReceiveGoodsAsync(branchId, branchName, model, delivererName);
        }

        public async Task<List<BranchInventory>> GetBranchInventoryListAsync(string branchId)
        {
            return await _repository.GetBranchInventoriesAsync(branchId);
        }

        public async Task<List<MaterialSearchDto>> SearchTrackedMaterialsAsync(string branchId, string term)
        {
            var materials = await _repository.SearchTrackedMaterialsAsync(branchId, term);
            return materials.Select(m => new MaterialSearchDto
            {
                MaterialId = m.MaterialId,
                MaterialName = m.MaterialName ?? "",
                Category = m.Category ?? "",
                MaterialKind = m.MaterialKind ?? "",
                StorageUnit = m.StorageUnit ?? ""
            }).ToList();
        }

        public async Task<BranchSupplyRequestHistoryViewModel> GetBranchRequestHistoryAsync(
            string branchId, DateTime fromDate, DateTime toDate, string status, int page, int pageSize)
        {
            var requests = await _repository.GetBranchRequestsAsync(branchId, fromDate, toDate, status, page, pageSize);
            var totalCount = await _repository.GetBranchRequestsCountAsync(branchId, fromDate, toDate, status);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new BranchSupplyRequestHistoryViewModel
            {
                Requests = requests,
                FromDate = fromDate,
                ToDate = toDate,
                SelectedStatus = status,
                CurrentPage = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };
        }

        // =========================================================================
        // 2. CENTRAL WAREHOUSE OPERATIONS
        // =========================================================================

        public async Task<ExportRequestsViewModel> GetExportRequestsAsync(
            DateTime fromDate, DateTime toDate, string status, int page, int pageSize)
        {
            var requests = await _repository.GetCentralExportRequestsAsync(fromDate, toDate, status, page, pageSize);
            var totalCount = await _repository.GetCentralExportRequestsCountAsync(fromDate, toDate, status);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new ExportRequestsViewModel
            {
                Requests = requests,
                FromDate = fromDate,
                ToDate = toDate,
                SelectedStatus = status,
                CurrentPage = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };
        }

        public async Task<object?> GetRequestDetailsJsonAsync(string requestCode, string? branchId = null)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode, branchId);
            if (req == null) return null;

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
            }).ToList();

            return new
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
                warehouseNote = req.WarehouseNote ?? "",
                inspectedBy = req.InspectedBy ?? "",
                inspectedAt = req.InspectedAt.HasValue ? req.InspectedAt.Value.ToString("dd/MM/yyyy HH:mm") : null,
                inspectionStatus = req.InspectionStatus ?? "",
                items = itemsResult
            };
        }

        public async Task<(bool Success, string Message)> ApproveAndPrepareShipmentAsync(string requestCode, string approverName)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode);
            if (req == null) return (false, "Đơn yêu cầu không tồn tại.");

            if (req.Status != "Chờ duyệt")
            {
                return (false, "Chỉ có thể duyệt đơn khi đơn đang ở trạng thái 'Chờ duyệt'.");
            }

            bool hasReleasedValues = req.Items.Any(i => i.QuantityReleased.HasValue);
            if (!hasReleasedValues)
            {
                return (false, "Vui lòng so sánh tồn kho và nhập số lượng thực xuất trước khi duyệt đơn.");
            }

            req.Status = "Đang chuẩn bị xuất";
            req.ApprovedBy = approverName;
            req.ApprovedDate = DateTime.UtcNow;

            await _repository.SaveChangesAsync();

            // Notify branch
            try
            {
                await _notificationService.SendAsync(new NotificationEvent(
                    Title: "Đơn yêu cầu đã được duyệt",
                    Message: $"Tổng kho đã duyệt đơn yêu cầu {req.RequestCode} và đang chuẩn bị xuất kho.",
                    RecipientRole: "BranchManager",
                    ResourceUrl: "/BranchWarehouse/RequestHistory",
                    BranchId: req.BranchId
                ));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send approval notification for {Code}", requestCode);
            }

            return (true, "Duyệt đơn và chuyển sang trạng thái chuẩn bị xuất thành công.");
        }

        public async Task<(bool Success, string Message)> ConfirmShipmentAsync(
            string requestCode, string? delivererName, string? delivererPhone, string? deliveryProvider)
        {
            if (string.IsNullOrWhiteSpace(delivererName))
            {
                return (false, "Vui lòng nhập họ tên người vận chuyển / tài xế giao hàng.");
            }

            if (string.IsNullOrWhiteSpace(delivererPhone))
            {
                return (false, "Vui lòng nhập số điện thoại người vận chuyển để chi nhánh liên hệ.");
            }

            var req = await _repository.GetRequestByCodeAsync(requestCode);
            if (req == null) return (false, "Đơn yêu cầu không tồn tại.");

            if (req.Status != "Đang chuẩn bị xuất")
            {
                return (false, "Chỉ có thể xác nhận xuất kho khi đơn ở trạng thái 'Đang chuẩn bị xuất'.");
            }

            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                req.DelivererName = delivererName.Trim();
                req.DelivererPhone = delivererPhone.Trim();
                req.DeliveryProvider = deliveryProvider?.Trim();
                req.Status = "Đã xuất kho";
                if (string.IsNullOrWhiteSpace(req.WarehouseNote))
                {
                    req.WarehouseNote = "Đang giao hàng";
                }

                // Deduct released quantities from central warehouse inventory
                foreach (var item in req.Items)
                {
                    decimal deductQty = item.QuantityReleased ?? item.QuantityRequested;
                    if (deductQty > 0)
                    {
                        var material = item.Material ?? await _repository.GetMaterialByIdAsync(item.MaterialId);
                        if (material != null)
                        {
                            material.StockQuantity -= deductQty;
                            if (material.StockQuantity < 0) material.StockQuantity = 0;
                        }
                    }
                }

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify branch manager
                try
                {
                    await _notificationService.SendAsync(new NotificationEvent(
                        Title: "Hàng đang được vận chuyển",
                        Message: $"Đơn hàng {req.RequestCode} đã được xuất kho bởi tài xế {req.DelivererName} (SĐT: {req.DelivererPhone}). Vui lòng chuẩn bị đồng kiểm khi nhận hàng.",
                        RecipientRole: "BranchManager",
                        ResourceUrl: "/BranchWarehouse/RequestHistory",
                        BranchId: req.BranchId
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send shipment notification for {Code}", requestCode);
                }

                return (true, "Đã xác nhận xuất kho và chuyển đơn hàng sang trạng thái 'Đã xuất kho'.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error confirming shipment for request {Code}", requestCode);
                return (false, "Lỗi hệ thống khi xuất kho: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> RejectSupplyRequestAsync(string requestCode, string approverName, string? reason)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode);
            if (req == null) return (false, "Đơn yêu cầu không tồn tại.");

            if (req.Status != "Chờ duyệt")
            {
                return (false, "Chỉ có thể từ chối đơn khi đang ở trạng thái Chờ duyệt.");
            }

            req.Status = "Từ chối";
            req.ApprovedBy = approverName;
            req.ApprovedDate = DateTime.UtcNow;
            req.WarehouseNote = reason?.Trim();

            await _repository.SaveChangesAsync();

            // Notify branch
            try
            {
                await _notificationService.SendAsync(new NotificationEvent(
                    Title: "Đơn yêu cầu bị từ chối",
                    Message: $"Tổng kho đã từ chối đơn yêu cầu {req.RequestCode}. Lý do: {reason ?? "Không có"}.",
                    RecipientRole: "BranchManager",
                    ResourceUrl: "/BranchWarehouse/RequestHistory",
                    BranchId: req.BranchId
                ));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send rejection notification for {Code}", requestCode);
            }

            return (true, "Đã từ chối đơn yêu cầu thành công.");
        }

        public async Task<List<StockComparisonItemDto>> CompareStockAsync(string requestCode)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode);
            if (req == null) return new List<StockComparisonItemDto>();

            var result = new List<StockComparisonItemDto>();
            foreach (var item in req.Items)
            {
                var material = item.Material ?? await _repository.GetMaterialByIdAsync(item.MaterialId);
                decimal stock = material?.StockQuantity ?? 0;
                string status = stock >= item.QuantityRequested ? "Đủ" : "Thiếu";

                result.Add(new StockComparisonItemDto
                {
                    MaterialId = item.MaterialId,
                    MaterialName = material?.MaterialName ?? "",
                    StorageUnit = material?.StorageUnit ?? "",
                    RequestedQuantity = item.QuantityRequested,
                    CurrentStockQuantity = stock,
                    ReleasedQuantity = item.QuantityReleased ?? item.QuantityRequested,
                    Status = status
                });
            }

            return result;
        }

        public async Task<(bool Success, string Message)> UpdateReleasedQuantitiesAsync(
            string requestCode, List<RequestItemReleasedModel> items, string? warehouseNote)
        {
            var req = await _repository.GetRequestByCodeAsync(requestCode);
            if (req == null) return (false, "Đơn yêu cầu không tồn tại.");

            if (req.Status != "Chờ duyệt" && req.Status != "Đang chuẩn bị xuất")
            {
                return (false, "Không thể cập nhật số lượng thực xuất ở trạng thái hiện tại.");
            }

            foreach (var itemModel in items)
            {
                var reqItem = req.Items.FirstOrDefault(i => i.MaterialId == itemModel.MaterialId);
                if (reqItem != null && reqItem.Material != null)
                {
                    if (itemModel.QuantityReleased > reqItem.Material.StockQuantity)
                    {
                        return (false, $"Số lượng thực xuất của \"{reqItem.Material.MaterialName}\" vượt quá tồn kho hiện có ({reqItem.Material.StockQuantity.ToString("G29")} {reqItem.Material.StorageUnit}).");
                    }
                    reqItem.QuantityReleased = itemModel.QuantityReleased;
                }
                else if (reqItem != null)
                {
                    reqItem.QuantityReleased = itemModel.QuantityReleased;
                }
            }

            if (!string.IsNullOrWhiteSpace(warehouseNote))
            {
                req.WarehouseNote = warehouseNote.Trim();
            }

            await _repository.SaveChangesAsync();
            return (true, "Đã cập nhật số lượng thực xuất thành công.");
        }
    }
}
