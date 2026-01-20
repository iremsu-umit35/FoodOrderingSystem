using FoodOrderingSystem.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/*
//databaase baðlamk için 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));*/

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
});



//session eklemek için KULLANICI GÝRÝÞ YAPTIÐINDA BÝLGÝLERÝN TUTULMASI ÝÇÝN
// 1. Session servisini ekle (builder.Services.AddControllersWithViews() öncesi)
builder.Services.AddDistributedMemoryCache(); // Session verisini bellekte tutar
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturumun ne kadar süreceði
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Cookie'nin Session için zorunlu olduðunu belirtir
});

//ASP.NET Core'da bir istemciden (tarayýcýdan) gelen tek bir HTTP isteði hakkýnda tüm bilgileri tutan kapsayýcýdýr
builder.Services.AddHttpContextAccessor();//////////////////////////////////////// BAK

// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();//cs ve js dosyalarý 


app.UseRouting();

app.UseSession(); // Session middleware'ini etkinleþtir
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); //router ile program varsayýlan olarak nerden baþlýyor görüyoruz
/// ilki conrolller isim ikinci çalýþxak metos

app.Run();
