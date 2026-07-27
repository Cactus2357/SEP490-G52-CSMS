using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;

namespace SEP490_G52_CSMS.Services
{
    public class EmployeeCreationDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string CitizenId { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string EmploymentType { get; set; } = null!;
        public string BranchId { get; set; } = null!;
    }

    public class EmployeeCreationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Employee? Employee { get; set; }
        public string? GeneratedUsername { get; set; }
        public string? PlainPassword { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = null!;
        public string CitizenId { get; set; } = null!;
        public Microsoft.AspNetCore.Http.IFormFile? CccdFile { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? ContractFile { get; set; }
    }

    public class EmployeeUpdateResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UncompletedShiftDto
    {
        public string Date { get; set; } = null!;
        public string ShiftName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class DeactivateRequestDto
    {
        public int EmployeeId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Notes { get; set; }
    }

    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetEmployeesListAsync();
        Task<IEnumerable<Branch>> GetBranchesAsync();
        Task<EmployeeCreationResult> CreateEmployeeAccountAsync(EmployeeCreationDto dto);
        Task<bool> RegisterFaceDataAsync(int employeeId, string faceData);
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        Task<EmployeeUpdateResult> UpdateEmployeeInfoAsync(int employeeId, UpdateEmployeeDto dto);
        Task<EmployeeUpdateResult> UpdatePermissionsAsync(int employeeId, string role, string employmentType);
        Task<(bool CanDeactivate, List<UncompletedShiftDto> UncompletedShifts)> CheckDeactivationConstraintsAsync(int employeeId);
        Task<bool> DeactivateEmployeeAsync(int employeeId, string reason, string notes);
    }
}
