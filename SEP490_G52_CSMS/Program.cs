using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CSMSAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddScoped<SEP490_G52_CSMS.Commons.IDAT_EmailHelper, SEP490_G52_CSMS.Commons.DAT_EmailHelper>();
builder.Services.AddScoped<SEP490_G52_CSMS.Reponsitories.IDAT_EmployeeRepository, SEP490_G52_CSMS.Reponsitories.DAT_EmployeeRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.IDAT_EmployeeService, SEP490_G52_CSMS.Services.DAT_EmployeeService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed database with default branches
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CSMSAppDbContext>();
        context.Database.Migrate();

        if (!context.Branches.Any())
        {
            context.Branches.AddRange(
                new SEP490_G52_CSMS.Models.Core.Branch
                {
                    BranchId = "CB001",
                    BranchName = "Chi nhánh Quận 1",
                    Address = "120 Lê Lợi, Phường Bến Thành, Quận 1, TPHCM",
                    OpeningTime = new TimeSpan(7, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    Status = "Active"
                },
                new SEP490_G52_CSMS.Models.Core.Branch
                {
                    BranchId = "CB002",
                    BranchName = "Chi nhánh Quận 3",
                    Address = "250 Nguyễn Thị Minh Khai, Quận 3, TPHCM",
                    OpeningTime = new TimeSpan(7, 0, 0),
                    ClosingTime = new TimeSpan(22, 30, 0),
                    Status = "Active"
                },
                new SEP490_G52_CSMS.Models.Core.Branch
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
            var manager1 = new SEP490_G52_CSMS.Models.Employees.Employee
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

            var manager2 = new SEP490_G52_CSMS.Models.Employees.Employee
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
                new SEP490_G52_CSMS.Models.Employees.BranchManager
                {
                    BranchId = "CB001",
                    ManagerId = manager1.EmployeeId,
                    AppointedDate = DateTime.Now
                },
                new SEP490_G52_CSMS.Models.Employees.BranchManager
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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DAT_Employee}/{action=Index}/{id?}");

app.Run();
