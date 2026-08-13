using SEP490_G52_CSMS.Commons.Constants;
using SEP490_G52_CSMS.Commons.Models;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.ViewModels;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;

        public BranchService(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchIndexViewModel> GetBranchesAsync(string? searchTerm, string? statusFilter, int? managerId, int pageIndex, int pageSize)
        {
            var branches = await _branchRepository.GetBranchesAsync(searchTerm, statusFilter, managerId, pageIndex, pageSize);
            var count = await _branchRepository.GetBranchCountAsync(searchTerm, statusFilter, managerId);
            var managerOptions = await GetManagerOptionsAsync();

            return new BranchIndexViewModel
            {
                Branches = branches.Select(b => new BranchListItemViewModel
                {
                    BranchId = b.BranchId,
                    BranchName = b.BranchName,
                    Address = b.Address,
                    Status = b.Status,
                    OpeningTime = b.OpeningTime.ToString(@"hh\:mm"),
                    ClosingTime = b.ClosingTime.ToString(@"hh\:mm"),
                    ManagerName = b.BranchManagers.FirstOrDefault()?.Manager?.FullName ?? BranchConstants.UnassignedManagerLabel
                }).ToList(),
                SearchTerm = searchTerm,
                StatusFilter = string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter,
                ManagerId = managerId,
                TotalCount = count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                ManagerOptions = managerOptions,
            };
        }

        public async Task<BranchCreateViewModel> GetBranchCreateModelAsync()
        {
            return new BranchCreateViewModel
            {
                Managers = await GetManagerOptionsAsync()
            };
        }

        public async Task<BranchEditViewModel?> GetBranchEditModelAsync(string branchId)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(branchId);
            if (branch == null)
            {
                return null;
            }

            return new BranchEditViewModel
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber,
                Email = branch.Email,
                OpeningTime = branch.OpeningTime,
                ClosingTime = branch.ClosingTime,
                Status = branch.Status,
                ManagerId = branch.BranchManagers.FirstOrDefault()?.ManagerId,
                Managers = await GetManagerOptionsAsync()
            };
        }

        public async Task<BranchDetailViewModel?> GetBranchDetailModelAsync(string branchId)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(branchId);
            if (branch == null)
            {
                return null;
            }

            var managerAssignment = branch.BranchManagers.FirstOrDefault();
            return new BranchDetailViewModel
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber ?? "-",
                Email = branch.Email ?? "-",
                OpeningTime = branch.OpeningTime.ToString(@"hh\:mm"),
                ClosingTime = branch.ClosingTime.ToString(@"hh\:mm"),
                Status = branch.Status,
                ManagerName = managerAssignment?.Manager?.FullName ?? BranchConstants.UnassignedManagerLabel,
                ManagerCode = managerAssignment?.Manager?.Username ?? "-",
                ManagerAssignedDate = managerAssignment?.AppointedDate.ToString("dd/MM/yyyy") ?? "-",
                CurrentManagerId = managerAssignment?.ManagerId,
                Managers = await GetManagerOptionsAsync()
            };
        }

        public async Task<BranchAssignManagerViewModel?> GetBranchAssignManagerModelAsync(string branchId)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(branchId);
            if (branch == null)
            {
                return null;
            }

            var currentAssignment = branch.BranchManagers.FirstOrDefault();
            return new BranchAssignManagerViewModel
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                CurrentManagerName = currentAssignment?.Manager?.FullName ?? BranchConstants.UnassignedManagerLabel,
                CurrentManagerCode = currentAssignment?.Manager?.Username ?? "-",
                CurrentManagerAssignedDate = currentAssignment?.AppointedDate.ToString("dd/MM/yyyy") ?? "-",
                SelectedManagerId = currentAssignment?.ManagerId,
                Managers = await GetManagerOptionsAsync()
            };
        }

        public async Task<OperationResult> AssignBranchManagerAsync(BranchAssignManagerViewModel model)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(model.BranchId);
            if (branch == null)
            {
                return OperationResult.Fail("Chi nhánh không tồn tại.");
            }

            await _branchRepository.RemoveBranchManagersAsync(model.BranchId);

            if (model.SelectedManagerId.HasValue && model.SelectedManagerId.Value > 0)
            {
                var assignment = new Models.Employees.BranchManager
                {
                    BranchId = model.BranchId,
                    ManagerId = model.SelectedManagerId.Value,
                    AppointedDate = DateTime.UtcNow
                };
                await _branchRepository.AddBranchManagerAsync(assignment);
            }

            return OperationResult.Ok("Đã gán quản lý mới cho chi nhánh.");
        }

        public async Task<OperationResult> DeactivateBranchAsync(string branchId)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(branchId);
            if (branch == null)
            {
                return OperationResult.Fail("Chi nhánh không tồn tại.");
            }

            branch.Status = BranchConstants.InactiveStatus;
            await _branchRepository.UpdateBranchAsync(branch);
            return OperationResult.Ok("Chi nhánh đã được ngừng hoạt động.");
        }

        public async Task<OperationResult> ActivateBranchAsync(string branchId)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(branchId);
            if (branch == null)
            {
                return OperationResult.Fail("Chi nhánh không tồn tại.");
            }

            branch.Status = BranchConstants.DefaultStatus;
            await _branchRepository.UpdateBranchAsync(branch);
            return OperationResult.Ok("Chi nhánh đã được kích hoạt.");
        }

        public async Task<OperationResult> CreateBranchAsync(BranchCreateViewModel model)
        {
            if (await _branchRepository.BranchNameExistsAsync(model.BranchName.Trim()))
            {
                return OperationResult.Fail("Tên chi nhánh đã tồn tại. Vui lòng chọn tên khác.");
            }

            var branchId = await _branchRepository.GenerateNextBranchIdAsync();
            var branch = new Branch
            {
                BranchId = branchId,
                BranchName = model.BranchName.Trim(),
                Address = model.Address.Trim(),
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                OpeningTime = model.OpeningTime,
                ClosingTime = model.ClosingTime,
                Status = BranchConstants.DefaultStatus,
            };

            await _branchRepository.AddBranchAsync(branch);

            if (model.ManagerId.HasValue && model.ManagerId > 0)
            {
                var assignment = new Models.Employees.BranchManager
                {
                    BranchId = branchId,
                    ManagerId = model.ManagerId.Value,
                    AppointedDate = DateTime.UtcNow
                };

                await _branchRepository.AddBranchManagerAsync(assignment);
            }

            return OperationResult.Ok("Chi nhánh mới đã được tạo thành công.");
        }

        public async Task<OperationResult> UpdateBranchAsync(BranchEditViewModel model)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(model.BranchId);
            if (branch == null)
            {
                return OperationResult.Fail("Chi nhánh không tồn tại.");
            }

            if (await _branchRepository.BranchNameExistsAsync(model.BranchName.Trim()) && !string.Equals(branch.BranchName, model.BranchName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult.Fail("Tên chi nhánh đã tồn tại. Vui lòng chọn tên khác.");
            }

            branch.BranchName = model.BranchName.Trim();
            branch.Address = model.Address.Trim();
            branch.PhoneNumber = model.PhoneNumber;
            branch.Email = model.Email;
            branch.OpeningTime = model.OpeningTime;
            branch.ClosingTime = model.ClosingTime;
            branch.Status = model.Status;

            await _branchRepository.UpdateBranchAsync(branch);
            await _branchRepository.RemoveBranchManagersAsync(model.BranchId);

            if (model.ManagerId.HasValue && model.ManagerId > 0)
            {
                var assignment = new Models.Employees.BranchManager
                {
                    BranchId = model.BranchId,
                    ManagerId = model.ManagerId.Value,
                    AppointedDate = DateTime.UtcNow
                };
                await _branchRepository.AddBranchManagerAsync(assignment);
            }

            return OperationResult.Ok("Cập nhật chi nhánh thành công.");
        }

        public async Task<OperationResult> DeleteBranchAsync(string branchId)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(branchId);
            if (branch == null)
            {
                return OperationResult.Fail("Chi nhánh không tồn tại.");
            }

            await _branchRepository.RemoveBranchManagersAsync(branchId);
            await _branchRepository.DeleteBranchAsync(branchId);

            return OperationResult.Ok("Chi nhánh đã được xóa thành công.");
        }

        private async Task<List<BranchManagerOption>> GetManagerOptionsAsync()
        {
            var managers = await _branchRepository.GetEligibleManagersAsync();
            var managerBranchMap = await _branchRepository.GetCurrentManagerBranchMapAsync();

            return managers.Select(m =>
            {
                managerBranchMap.TryGetValue(m.EmployeeId, out var branchName);
                return new BranchManagerOption
                {
                    ManagerId = m.EmployeeId,
                    ManagerName = m.FullName ?? m.Username ?? "Người quản lý",
                    EmployeeCode = m.Username ?? $"NV{m.EmployeeId:D3}",
                    CurrentBranchName = branchName
                };
            }).ToList();
        }
    }
}
