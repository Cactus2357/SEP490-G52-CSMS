using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
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

// Rate Limiting (.NET 8)
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    rateLimiterOptions.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
        if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json"))
        {
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            await context.HttpContext.Response.WriteAsync("{\"error\": \"Quá nhiều yêu cầu. Vui lòng thử lại sau 1 phút.\"}", token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "<div style='font-family:sans-serif; text-align:center; padding:50px;'>" +
                "<h2 style='color:#d9534f;'>429 - Quá nhiều yêu cầu (Too Many Requests)</h2>" +
                "<p>Hệ thống tạm thời giới hạn tần suất gửi yêu cầu từ địa chỉ IP của bạn để đảm bảo an toàn.</p>" +
                "<p>Vui lòng đợi 1 phút trước khi thử lại.</p></div>", token);
        }
    };

    // Global sliding window: max 100 requests per minute per IP
    rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 10
            });
    });

    // Strict policy for Auth/Login: max 5 requests per minute per IP
    rateLimiterOptions.AddPolicy("AuthLimit", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();