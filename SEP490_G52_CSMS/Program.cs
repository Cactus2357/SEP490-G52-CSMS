using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Repositories;
using SEP490_G52_CSMS.Repositories.Interfaces;
using SEP490_G52_CSMS.Services;
using SEP490_G52_CSMS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Database
// =========================
builder.Services.AddDbContext<CSMSAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWeeklyRosterRepository, WeeklyRosterRepository>();

builder.Services.AddScoped<IWeeklyRosterService, WeeklyRosterService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Add services to the container.
builder.Services.AddScoped<SEP490_G52_CSMS.Commons.IDAT_EmailHelper, SEP490_G52_CSMS.Commons.DAT_EmailHelper>();
builder.Services.AddScoped<SEP490_G52_CSMS.Repositories.IEmployeeRepository, SEP490_G52_CSMS.Repositories.EmployeeRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.IEmployeeService, SEP490_G52_CSMS.Services.EmployeeService>();
builder.Services.AddControllersWithViews();

// =========================
// Dependency Injection
// =========================

// Category
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login"; // Redirect về Login hoặc trang lỗi tự tạo
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Product
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// ShiftChange
builder.Services.AddScoped<SEP490_G52_CSMS.Repositories.IShiftChangeRepository, SEP490_G52_CSMS.Repositories.ShiftChangeRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.IShiftChangeService, SEP490_G52_CSMS.Services.ShiftChangeService>();

// Branch
builder.Services.AddScoped<SEP490_G52_CSMS.Repositories.IBranchRepository, SEP490_G52_CSMS.Repositories.BranchRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.IBranchService, SEP490_G52_CSMS.Services.BranchService>();

// CashHandover
builder.Services.AddScoped<SEP490_G52_CSMS.Repositories.ICashHandoverRepository, SEP490_G52_CSMS.Repositories.CashHandoverRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.ICashHandoverService, SEP490_G52_CSMS.Services.CashHandoverService>();

// OrderManagement
builder.Services.AddScoped<SEP490_G52_CSMS.Repositories.IOrderManagementRepository, SEP490_G52_CSMS.Repositories.OrderManagementRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.IOrderManagementService, SEP490_G52_CSMS.Services.OrderManagementService>();

// LeaveRequest
builder.Services.AddScoped<SEP490_G52_CSMS.Repositories.ILeaveRequestRepository, SEP490_G52_CSMS.Repositories.LeaveRequestRepository>();
builder.Services.AddScoped<SEP490_G52_CSMS.Services.ILeaveRequestService, SEP490_G52_CSMS.Services.LeaveRequestService>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SEP490_G52_CSMS.Models.DbInitializer.Seed(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();