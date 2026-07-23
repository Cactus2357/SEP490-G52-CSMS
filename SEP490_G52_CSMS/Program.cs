using Microsoft.EntityFrameworkCore;
using SEP490_G52_CSMS.Models;
using SEP490_G52_CSMS.Repositories;
using SEP490_G52_CSMS.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CSMSAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<ICashHandoverRepository, CashHandoverRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();

// Đăng ký cho Xin đổi ca
builder.Services.AddScoped<IShiftChangeRepository, ShiftChangeRepository>();
builder.Services.AddScoped<IShiftChangeService, ShiftChangeService>();

// Đăng ký cho Quản lý Order
builder.Services.AddScoped<IOrderManagementRepository, OrderManagementRepository>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICashHandoverService, CashHandoverService>();
builder.Services.AddScoped<IWorkScheduleRepository, WorkScheduleRepository>();
builder.Services.AddScoped<IWorkScheduleService, WorkScheduleService>();

var app = builder.Build();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
