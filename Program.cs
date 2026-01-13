using Microsoft.EntityFrameworkCore;
using QLBDN.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

/* 🔥 Bật SESSION để lưu Login */
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

/* 🔥 Kết nối SQL Server */
builder.Services.AddDbContext<QlbdnContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLBDNConnection"))
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

/* 🔥 SESSION phải đặt TRƯỚC Authorization */
app.UseSession();

app.UseAuthorization();

/* Cho phép Attribute Routing */
app.MapControllers();

/* ROUTE MẶC ĐỊNH */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}"
);

app.Run();
