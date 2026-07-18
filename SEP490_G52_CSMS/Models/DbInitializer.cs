using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SEP490_G52_CSMS.Models.Core;
using SEP490_G52_CSMS.Models.Employees;
using System;
using System.Linq;

namespace SEP490_G52_CSMS.Models
{
    public static class DbInitializer
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<CSMSAppDbContext>();
                context.Database.Migrate();

                if (!context.Branches.Any())
                {
                    context.Branches.AddRange(
                        new Branch
                        {
                            BranchId = "CB001",
                            BranchName = "Chi nhánh Quận 1",
                            Address = "120 Lê Lợi, Phường Bến Thành, Quận 1, TPHCM",
                            OpeningTime = new TimeSpan(7, 0, 0),
                            ClosingTime = new TimeSpan(22, 0, 0),
                            Status = "Active"
                        },
                        new Branch
                        {
                            BranchId = "CB002",
                            BranchName = "Chi nhánh Quận 3",
                            Address = "250 Nguyễn Thị Minh Khai, Quận 3, TPHCM",
                            OpeningTime = new TimeSpan(7, 0, 0),
                            ClosingTime = new TimeSpan(22, 30, 0),
                            Status = "Active"
                        },
                        new Branch
                        {
                            BranchId = "CB003",
                            BranchName = "Chi nhánh Bình Thạnh",
                            Address = "45 Điện Biên Phủ, Quận Bình Thạnh, TPHCM",
                            OpeningTime = new TimeSpan(6, 30, 0),
                            ClosingTime = new TimeSpan(22, 0, 0),
                            Status = "Active"
                        }
                    );
                    context.SaveChanges();
                }

                if (!context.Employees.Any(e => e.Role == "BranchManager"))
                {
                    var manager1 = new Employee
                    {
                        FullName = "Quản lý Quận 1",
                        Username = "manager1",
                        Password = SEP490_G52_CSMS.Commons.DAT_PasswordHasher.HashPassword("12345678"),
                        Email = "manager1@gmail.com",
                        PhoneNumber = "0900000001",
                        CitizenId = "000000000001",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Address = "120 Lê Lợi, Q1",
                        Role = "BranchManager",
                        EmploymentType = "Full-time",
                        BranchId = "CB001",
                        Status = "Active"
                    };

                    var manager2 = new Employee
                    {
                        FullName = "Quản lý Quận 3",
                        Username = "manager2",
                        Password = SEP490_G52_CSMS.Commons.DAT_PasswordHasher.HashPassword("12345678"),
                        Email = "manager2@gmail.com",
                        PhoneNumber = "0900000002",
                        CitizenId = "000000000002",
                        DateOfBirth = new DateTime(1992, 1, 1),
                        Address = "250 Nguyễn Thị Minh Khai, Q3",
                        Role = "BranchManager",
                        EmploymentType = "Full-time",
                        BranchId = "CB002",
                        Status = "Active"
                    };

                    context.Employees.AddRange(manager1, manager2);
                    context.SaveChanges();

                    context.BranchManagers.AddRange(
                        new BranchManager
                        {
                            BranchId = "CB001",
                            ManagerId = manager1.EmployeeId,
                            AppointedDate = DateTime.Now
                        },
                        new BranchManager
                        {
                            BranchId = "CB002",
                            ManagerId = manager2.EmployeeId,
                            AppointedDate = DateTime.Now
                        }
                    );
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            }
        }
    }
}
