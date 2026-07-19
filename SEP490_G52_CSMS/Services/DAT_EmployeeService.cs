using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Reponsitories;

namespace SEP490_G52_CSMS.Services
{
    public class DAT_EmployeeService : IDAT_EmployeeService
    {
        private readonly IDAT_EmployeeRepository _repository;
        private readonly IDAT_EmailHelper _emailHelper;

        public DAT_EmployeeService(IDAT_EmployeeRepository repository, IDAT_EmailHelper emailHelper)
        {
            _repository = repository;
            _emailHelper = emailHelper;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesListAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<Branch>> GetBranchesAsync()
        {
            return await _repository.GetActiveBranchesAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            return await _repository.GetByIdAsync(employeeId);
        }

        public async Task<DAT_EmployeeCreationResult> CreateEmployeeAccountAsync(DAT_EmployeeCreationDto dto)
        {
            try
            {
                // Basic validations
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Họ và tên không được để trống." };
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Email không được để trống." };
                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Số điện thoại không được để trống." };
                if (string.IsNullOrWhiteSpace(dto.CitizenId) || dto.CitizenId.Length != 12)
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Số CCCD phải đúng 12 chữ số." };

                // Business validations - Unique check
                if (await _repository.ExistsEmailAsync(dto.Email.Trim()))
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Email đã tồn tại trên hệ thống." };
                if (await _repository.ExistsCitizenIdAsync(dto.CitizenId.Trim()))
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Số CCCD đã tồn tại trên hệ thống." };
                if (await _repository.ExistsPhoneNumberAsync(dto.PhoneNumber.Trim()))
                    return new DAT_EmployeeCreationResult { Success = false, ErrorMessage = "Số điện thoại đã tồn tại trên hệ thống." };

                // 1. Generate formatted non-accented username
                string baseUsername = DAT_UsernameFormatter.Format(dto.FullName);
                if (string.IsNullOrEmpty(baseUsername))
                {
                    baseUsername = "employee";
                }
                
                string finalUsername = baseUsername;
                int counter = 1;
                
                // Keep checking until a unique username is found
                while (await _repository.ExistsUsernameAsync(finalUsername))
                {
                    finalUsername = $"{baseUsername}{counter}";
                    counter++;
                }

                // 2. Generate secure random password
                string plainPassword = DAT_PasswordGenerator.Generate(12);

                // 3. Hash the password
                string hashedPassword = DAT_PasswordHasher.HashPassword(plainPassword);

                // 4. Create Employee object
                var employee = new Employee
                {
                    FullName = dto.FullName.Trim(),
                    Username = finalUsername,
                    Password = hashedPassword,
                    Email = dto.Email.Trim(),
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    CitizenId = dto.CitizenId.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address.Trim(),
                    Role = dto.Role,
                    EmploymentType = dto.EmploymentType,
                    BranchId = dto.BranchId,
                    Status = "Active",
                    FailedLoginAttempts = 0
                };

                // 5. Save to database
                await _repository.AddAsync(employee);

                // 6. Send credentials via email
                _emailHelper.SendAccountCredentials(employee.Email, employee.FullName, employee.Username, plainPassword);

                return new DAT_EmployeeCreationResult
                {
                    Success = true,
                    Employee = employee,
                    GeneratedUsername = finalUsername,
                    PlainPassword = plainPassword
                };
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $"\nChi tiết: {ex.InnerException.Message}" : "";
                return new DAT_EmployeeCreationResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi hệ thống khi tạo tài khoản: {ex.Message}{innerMessage}"
                };
            }
        }

        public async Task<bool> RegisterFaceDataAsync(int employeeId, string faceData)
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            if (employee == null) return false;

            // Normalize: trim whitespace to ensure consistent format for login matching
            employee.FaceData = faceData?.Trim();
            await _repository.UpdateAsync(employee);
            return true;
        }

        public async Task<DAT_EmployeeUpdateResult> UpdateEmployeeInfoAsync(int employeeId, DAT_UpdateEmployeeDto dto)
        {
            try
            {
                var employee = await _repository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Không tìm thấy nhân viên." };
                }

                // BR01: Date of Birth is mandatory.
                if (dto.DateOfBirth == default)
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Ngày sinh là bắt buộc." };
                }

                // BR02: Address is mandatory.
                if (string.IsNullOrWhiteSpace(dto.Address))
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Địa chỉ là bắt buộc." };
                }

                // BR04: Citizen Identification Number must contain exactly 12 digits.
                if (string.IsNullOrWhiteSpace(dto.CitizenId) || dto.CitizenId.Trim().Length != 12 || !System.Text.RegularExpressions.Regex.IsMatch(dto.CitizenId.Trim(), @"^\d{12}$"))
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Số CCCD phải chứa chính xác 12 chữ số." };
                }

                // BR03: Citizen Identification Number must be unique within the system.
                bool citizenIdExists = await _repository.ExistsCitizenIdExcludeSelfAsync(dto.CitizenId.Trim(), employeeId);
                if (citizenIdExists)
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Số CCCD đã tồn tại trên hệ thống." };
                }

                // Check and validate uploaded files
                // BR05: Uploaded files must be in PDF or JPG format.
                // BR06: Individual uploaded files must not exceed the system file size limit. Let's make it 5MB (5 * 1024 * 1024 bytes).
                const long MaxFileSize = 5 * 1024 * 1024;
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg" };

                // Handle CCCD File
                if (dto.CccdFile != null && dto.CccdFile.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(dto.CccdFile.FileName).ToLower();
                    if (!System.Linq.Enumerable.Contains(allowedExtensions, fileExtension))
                    {
                        return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "File ảnh CCCD phải có định dạng .pdf hoặc .jpg (.jpeg)." };
                    }
                    if (dto.CccdFile.Length > MaxFileSize)
                    {
                        return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Dung lượng file ảnh CCCD không được vượt quá 5MB." };
                    }

                    // Save file
                    string uploadsFolder = System.IO.Path.Combine("wwwroot", "uploads", "employees");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = $"cccd_{employeeId}_{Guid.NewGuid()}{fileExtension}";
                    string filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await dto.CccdFile.CopyToAsync(fileStream);
                    }
                    employee.CccdFilePath = $"/uploads/employees/{uniqueFileName}";
                }

                // Handle Contract File
                if (dto.ContractFile != null && dto.ContractFile.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(dto.ContractFile.FileName).ToLower();
                    if (!System.Linq.Enumerable.Contains(allowedExtensions, fileExtension))
                    {
                        return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "File hợp đồng lao động phải có định dạng .pdf hoặc .jpg (.jpeg)." };
                    }
                    if (dto.ContractFile.Length > MaxFileSize)
                    {
                        return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Dung lượng file hợp đồng không được vượt quá 5MB." };
                    }

                    // Save file
                    string uploadsFolder = System.IO.Path.Combine("wwwroot", "uploads", "employees");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = $"contract_{employeeId}_{Guid.NewGuid()}{fileExtension}";
                    string filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await dto.ContractFile.CopyToAsync(fileStream);
                    }
                    employee.ContractFilePath = $"/uploads/employees/{uniqueFileName}";
                }

                // Update text fields
                employee.DateOfBirth = dto.DateOfBirth;
                employee.Address = dto.Address.Trim();
                employee.CitizenId = dto.CitizenId.Trim();

                await _repository.UpdateAsync(employee);

                return new DAT_EmployeeUpdateResult { Success = true };
            }
            catch (Exception ex)
            {
                return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = $"Lỗi hệ thống khi cập nhật thông tin: {ex.Message}" };
            }
        }

        public async Task<DAT_EmployeeUpdateResult> UpdatePermissionsAsync(int employeeId, string role, string employmentType)
        {
            try
            {
                var employee = await _repository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Không tìm thấy nhân viên." };
                }

                // BR07: Only branch CB001 for now (mặc định CB001)
                if (employee.BranchId != "CB001")
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Bạn không có quyền cập nhật nhân viên thuộc chi nhánh khác." };
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Vai trò không được để trống." };
                }

                if (string.IsNullOrWhiteSpace(employmentType))
                {
                    return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = "Loại nhân viên không được để trống." };
                }

                employee.Role = role;
                employee.EmploymentType = employmentType;

                await _repository.UpdateAsync(employee);
                return new DAT_EmployeeUpdateResult { Success = true };
            }
            catch (Exception ex)
            {
                return new DAT_EmployeeUpdateResult { Success = false, ErrorMessage = $"Lỗi hệ thống khi cập nhật phân quyền: {ex.Message}" };
            }
        }

        public async Task<(bool CanDeactivate, List<UncompletedShiftDto> UncompletedShifts)> CheckDeactivationConstraintsAsync(int employeeId)
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return (false, new List<UncompletedShiftDto>());
            }

            var rosters = await _repository.GetRostersWithAttendanceAndHandoverAsync(employeeId);
            var uncompletedList = new List<UncompletedShiftDto>();

            foreach (var roster in rosters)
            {
                bool isUncompleted = false;
                if (roster.AssignmentDate.Date > DateTime.Today)
                {
                    isUncompleted = true;
                }
                else
                {
                    var log = roster.AttendanceLogs.FirstOrDefault(l => l.EmployeeId == employeeId);
                    if (log == null || log.CheckOutTime == null)
                    {
                        isUncompleted = true;
                    }
                    else if (employee.Role == "Cashier")
                    {
                        bool hasHandover = await _repository.HasCashHandoverAsync(employeeId, roster.ShiftId, roster.AssignmentDate);
                        if (!hasHandover)
                        {
                            isUncompleted = true;
                        }
                    }
                }

                if (isUncompleted)
                {
                    uncompletedList.Add(new UncompletedShiftDto
                    {
                        Date = roster.AssignmentDate.ToString("dd/MM"),
                        ShiftName = roster.FixedShift?.ShiftName ?? "Ca làm việc",
                        Status = "Chưa bàn giao"
                    });
                }
            }

            bool canDeactivate = uncompletedList.Count == 0;
            return (canDeactivate, uncompletedList);
        }

        public async Task<bool> DeactivateEmployeeAsync(int employeeId, string reason, string notes)
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            if (employee == null) return false;

            employee.Status = "Inactive";
            await _repository.UpdateAsync(employee);

            // Log details (simulates secure logging in history repository)
            Console.WriteLine($"[DEACTIVATION LOG] Employee ID: {employeeId}, Username: {employee.Username}, Time: {DateTime.Now}, Reason: {reason}, Notes: {notes}");

            return true;
        }
    }
}
