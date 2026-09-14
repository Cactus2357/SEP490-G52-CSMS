using SEP490_G52_CSMS.Commons;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IDAT_EmailHelper _emailHelper;
        private readonly IFileStorageService? _fileStorageService;

        public EmployeeService(
            IEmployeeRepository repository,
            IDAT_EmailHelper emailHelper,
            IFileStorageService? fileStorageService = null)
        {
            _repository = repository;
            _emailHelper = emailHelper;
            _fileStorageService = fileStorageService;
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

        public async Task<EmployeeCreationResult> CreateEmployeeAccountAsync(EmployeeCreationDto dto)
        {
            try
            {
                var fullName = dto.FullName?.Trim() ?? "";
                var email = dto.Email?.Trim() ?? "";
                var phoneNumber = dto.PhoneNumber?.Trim() ?? "";
                var citizenId = dto.CitizenId?.Trim() ?? "";
                var role = dto.Role?.Trim() ?? "Cashier";
                var employmentType = dto.EmploymentType?.Trim() ?? "Full-time";
                var branchId = dto.BranchId?.Trim() ?? "";
                var address = !string.IsNullOrWhiteSpace(dto.Address) ? dto.Address.Trim() : "Chưa cập nhật";
                var dob = dto.DateOfBirth ?? DateTime.UtcNow.Date.AddYears(-20);

                // Basic validations
                if (string.IsNullOrWhiteSpace(fullName))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Họ và tên không được để trống." };
                if (string.IsNullOrWhiteSpace(email))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Email không được để trống." };
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Số điện thoại không được để trống." };
                if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)." };
                if (string.IsNullOrWhiteSpace(citizenId) || citizenId.Length != 12)
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Số CCCD phải đúng 12 chữ số." };

                // Business validations - Unique check
                if (await _repository.ExistsEmailAsync(email))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Email này đã tồn tại trên hệ thống." };
                if (await _repository.ExistsCitizenIdAsync(citizenId))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Số CCCD này đã tồn tại trên hệ thống." };
                if (await _repository.ExistsPhoneNumberAsync(phoneNumber))
                    return new EmployeeCreationResult { Success = false, ErrorMessage = "Số điện thoại này đã tồn tại trên hệ thống." };

                // 1. Generate formatted username
                string baseUsername = DAT_UsernameFormatter.Format(fullName);
                if (string.IsNullOrEmpty(baseUsername))
                {
                    baseUsername = "employee";
                }

                string finalUsername = baseUsername;
                int counter = 1;

                while (await _repository.ExistsUsernameAsync(finalUsername))
                {
                    finalUsername = $"{baseUsername}{counter}";
                    counter++;
                }

                // 2. Generate random password
                string plainPassword = DAT_PasswordGenerator.Generate(12);

                // 3. Hash password
                string hashedPassword = DAT_PasswordHasher.HashPassword(plainPassword);

                const long MaxFileSize = 10 * 1024 * 1024;
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

                // 4. Create Employee object
                var employee = new Employee
                {
                    FullName = fullName,
                    Username = finalUsername,
                    Password = hashedPassword,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    CitizenId = citizenId,
                    DateOfBirth = dob,
                    Address = address,
                    Role = role,
                    EmploymentType = employmentType,
                    BranchId = branchId,
                    Status = "Active",
                    FailedLoginAttempts = 0
                };

                // 5. Save initial record to get EmployeeId
                await _repository.AddAsync(employee);

                // Handle optional file uploads during account creation
                bool fileUpdated = false;
                if (dto.CccdFile != null && dto.CccdFile.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(dto.CccdFile.FileName).ToLower();
                    if (System.Linq.Enumerable.Contains(allowedExtensions, fileExtension) && dto.CccdFile.Length <= MaxFileSize)
                    {
                        if (_fileStorageService != null)
                        {
                            employee.CccdFilePath = await _fileStorageService.UploadFileAsync(dto.CccdFile, "employees");
                        }
                        else
                        {
                            string uploadsFolder = System.IO.Path.Combine("wwwroot", "uploads", "employees");
                            if (!System.IO.Directory.Exists(uploadsFolder))
                            {
                                System.IO.Directory.CreateDirectory(uploadsFolder);
                            }
                            string uniqueFileName = $"cccd_{employee.EmployeeId}_{Guid.NewGuid()}{fileExtension}";
                            string filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                            {
                                await dto.CccdFile.CopyToAsync(fileStream);
                            }
                            employee.CccdFilePath = $"/uploads/employees/{uniqueFileName}";
                        }
                        fileUpdated = true;
                    }
                }

                if (dto.ContractFile != null && dto.ContractFile.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(dto.ContractFile.FileName).ToLower();
                    if (System.Linq.Enumerable.Contains(allowedExtensions, fileExtension) && dto.ContractFile.Length <= MaxFileSize)
                    {
                        if (_fileStorageService != null)
                        {
                            employee.ContractFilePath = await _fileStorageService.UploadFileAsync(dto.ContractFile, "employees");
                        }
                        else
                        {
                            string uploadsFolder = System.IO.Path.Combine("wwwroot", "uploads", "employees");
                            if (!System.IO.Directory.Exists(uploadsFolder))
                            {
                                System.IO.Directory.CreateDirectory(uploadsFolder);
                            }
                            string uniqueFileName = $"contract_{employee.EmployeeId}_{Guid.NewGuid()}{fileExtension}";
                            string filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                            {
                                await dto.ContractFile.CopyToAsync(fileStream);
                            }
                            employee.ContractFilePath = $"/uploads/employees/{uniqueFileName}";
                        }
                        fileUpdated = true;
                    }
                }

                if (fileUpdated)
                {
                    await _repository.UpdateAsync(employee);
                }

                // 6. Send email
                _emailHelper.SendAccountCredentials(employee.Email, employee.FullName, employee.Username, plainPassword);

                return new EmployeeCreationResult
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
                return new EmployeeCreationResult
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

            employee.FaceData = faceData?.Trim();
            await _repository.UpdateAsync(employee);
            return true;
        }

        public async Task<EmployeeUpdateResult> UpdateEmployeeInfoAsync(int employeeId, UpdateEmployeeDto dto)
        {
            try
            {
                var employee = await _repository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    return new EmployeeUpdateResult { Success = false, ErrorMessage = "Không tìm thấy nhân viên." };
                }

                if (!string.IsNullOrWhiteSpace(dto.FullName))
                {
                    employee.FullName = dto.FullName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                {
                    string phone = dto.PhoneNumber.Trim();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$"))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)." };
                    }
                    if (await _repository.ExistsPhoneNumberExcludeSelfAsync(phone, employeeId))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Số điện thoại đã tồn tại trên hệ thống." };
                    }
                    employee.PhoneNumber = phone;
                }

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    string email = dto.Email.Trim();
                    if (await _repository.ExistsEmailExcludeSelfAsync(email, employeeId))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Email đã tồn tại trên hệ thống." };
                    }
                    employee.Email = email;
                }

                if (!string.IsNullOrWhiteSpace(dto.CitizenId))
                {
                    string citizenId = dto.CitizenId.Trim();
                    if (citizenId.Length != 12 || !System.Text.RegularExpressions.Regex.IsMatch(citizenId, @"^\d{12}$"))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Số CCCD phải chứa chính xác 12 chữ số." };
                    }
                    if (await _repository.ExistsCitizenIdExcludeSelfAsync(citizenId, employeeId))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Số CCCD đã tồn tại trên hệ thống." };
                    }
                    employee.CitizenId = citizenId;
                }

                if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value != default)
                {
                    employee.DateOfBirth = dto.DateOfBirth.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Address))
                {
                    employee.Address = dto.Address.Trim();
                }

                const long MaxFileSize = 10 * 1024 * 1024;
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

                if (dto.CccdFile != null && dto.CccdFile.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(dto.CccdFile.FileName).ToLower();
                    if (!System.Linq.Enumerable.Contains(allowedExtensions, fileExtension))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "File ảnh CCCD phải có định dạng .pdf, .jpg, .png hoặc .webp." };
                    }
                    if (dto.CccdFile.Length > MaxFileSize)
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Dung lượng file ảnh CCCD không được vượt quá 10MB." };
                    }

                    if (_fileStorageService != null)
                    {
                        employee.CccdFilePath = await _fileStorageService.UploadFileAsync(dto.CccdFile, "employees");
                    }
                    else
                    {
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
                }

                if (dto.ContractFile != null && dto.ContractFile.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(dto.ContractFile.FileName).ToLower();
                    if (!System.Linq.Enumerable.Contains(allowedExtensions, fileExtension))
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "File hợp đồng lao động phải có định dạng .pdf, .jpg, .png hoặc .webp." };
                    }
                    if (dto.ContractFile.Length > MaxFileSize)
                    {
                        return new EmployeeUpdateResult { Success = false, ErrorMessage = "Dung lượng file hợp đồng không được vượt quá 10MB." };
                    }

                    if (_fileStorageService != null)
                    {
                        employee.ContractFilePath = await _fileStorageService.UploadFileAsync(dto.ContractFile, "employees");
                    }
                    else
                    {
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
                }

                await _repository.UpdateAsync(employee);

                return new EmployeeUpdateResult { Success = true };
            }
            catch (Exception ex)
            {
                return new EmployeeUpdateResult { Success = false, ErrorMessage = $"Lỗi hệ thống khi cập nhật thông tin: {ex.Message}" };
            }
        }

        public async Task<EmployeeUpdateResult> UpdatePermissionsAsync(int employeeId, string role, string employmentType)
        {
            try
            {
                var employee = await _repository.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    return new EmployeeUpdateResult { Success = false, ErrorMessage = "Không tìm thấy nhân viên." };
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    return new EmployeeUpdateResult { Success = false, ErrorMessage = "Vai trò không được để trống." };
                }

                if (string.IsNullOrWhiteSpace(employmentType))
                {
                    return new EmployeeUpdateResult { Success = false, ErrorMessage = "Loại nhân viên không được để trống." };
                }

                employee.Role = role;
                employee.EmploymentType = employmentType;

                await _repository.UpdateAsync(employee);
                return new EmployeeUpdateResult { Success = true };
            }
            catch (Exception ex)
            {
                return new EmployeeUpdateResult { Success = false, ErrorMessage = $"Lỗi hệ thống khi cập nhật phân quyền: {ex.Message}" };
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
                if (roster.AssignmentDate.Date > DateTime.UtcNow.Date)
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

            Console.WriteLine($"[DEACTIVATION LOG] Employee ID: {employeeId}, Username: {employee.Username}, Time: {DateTime.UtcNow} UTC, Reason: {reason}, Notes: {notes}");

            return true;
        }
    }
}
