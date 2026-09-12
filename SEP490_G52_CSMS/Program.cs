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
builder.Services.AddScoped<ICashierWorkEligibilityService, CashierWorkEligibilityService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Add services to the container.
builder.Services.AddScoped<SEP490_G52_CSMS.Commons.IDAT_EmailHelper, SEP490_G52_CSMS.Commons.DAT_EmailHelper>();
builder.Services.AddScoped<IEmployeeRepository, SEP490_G52_CSMS.Repositories.EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, SEP490_G52_CSMS.Services.EmployeeService>();
builder.Services.AddHttpClient();
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
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Product
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, SEP490_G52_CSMS.Services.OrderService>();

// Menu Management
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, SEP490_G52_CSMS.Services.MenuService>();

// ShiftChange
builder.Services.AddScoped<IShiftChangeRepository, SEP490_G52_CSMS.Repositories.ShiftChangeRepository>();
builder.Services.AddScoped<IShiftChangeService, SEP490_G52_CSMS.Services.ShiftChangeService>();

// Branch
builder.Services.AddScoped<IBranchRepository, SEP490_G52_CSMS.Repositories.BranchRepository>();
builder.Services.AddScoped<IBranchService, SEP490_G52_CSMS.Services.BranchService>();

// CashHandover
builder.Services.AddScoped<ICashHandoverRepository, SEP490_G52_CSMS.Repositories.CashHandoverRepository>();
builder.Services.AddScoped<ICashHandoverService, SEP490_G52_CSMS.Services.CashHandoverService>();

// OrderManagement
builder.Services.AddScoped<IOrderManagementRepository, SEP490_G52_CSMS.Repositories.OrderManagementRepository>();
builder.Services.AddScoped<IOrderManagementService, SEP490_G52_CSMS.Services.OrderManagementService>();

// LeaveRequest
builder.Services.AddScoped<ILeaveRequestRepository, SEP490_G52_CSMS.Repositories.LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveRequestService, SEP490_G52_CSMS.Services.LeaveRequestService>();

// Notification (event-driven service)
builder.Services.AddScoped<INotificationService, SEP490_G52_CSMS.Services.NotificationService>();

// Voucher Management
builder.Services.AddScoped<IVoucherService, VoucherService>();

// File Storage (S3 / Local fallback)
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Warehouse & Branch Supply Request
builder.Services.AddScoped<IWarehouseSupplyRepository, WarehouseSupplyRepository>();
builder.Services.AddScoped<IWarehouseSupplyService, WarehouseSupplyService>();

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
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();