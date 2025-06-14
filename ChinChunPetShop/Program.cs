using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using ChinChunPetShop.Models.Services;
using ChinChunPetShop.Models.Sevices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChinChunPetShopContext>(options =>
    options.UseMySql(
        "server=103.178.235.9;database=ntu195_ChinChunPetShop;user=ntu195_chinchun_user;password=chinchun_user!",
        ServerVersion.AutoDetect("server=103.178.235.9;database=ntu195_ChinChunPetShop;user=ntu195_chinchun_user;password=chinchun_user!")
    ));
builder.Services
  .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.LoginPath = "/nhanvien/dangnhap";
      options.AccessDeniedPath = "/nhanvien/chucnang";
  });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("VaiTro", "VT000001"));
    options.AddPolicy("NVDuyetDon", policy =>
        policy.RequireClaim("VaiTro", "VT000002", "VT000001"));
    options.AddPolicy("NVBanHang", policy =>
        policy.RequireClaim("VaiTro", "VT000003", "VT000001"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.Configure<SMTPSettings>(builder.Configuration.GetSection("SMTP"));
builder.Services.AddTransient<SecurityService>();
builder.Services.AddScoped<SessionConfig>();
builder.Services.AddScoped<DBConfig>();
builder.Services.AddScoped<NhanVienService>();
builder.Services.AddScoped<SanPhamService>();
builder.Services.AddScoped<KhachHangService>();
builder.Services.AddScoped<SoKhoService>();
builder.Services.AddScoped<GioHangService>();
builder.Services.AddScoped<DonHangService>();
builder.Services.AddScoped<ThamSoService>();

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
app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
//app.MapControllerRoute(
//    name: "admin_sd",
//    pattern: "admin_sd/{action=Index}/{id?}",
//    defaults: new { area = "Admin", controller = "HomeAdmin" }
//);


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Logger.LogInformation("Routing started...");
app.Run();
