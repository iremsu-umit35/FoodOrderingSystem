using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Models.DTOs;
using FoodOrderingSystem.Models.Entities;
using FoodOrderingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;




namespace FoodOrderingSystem.Controllers
{
    public class AdminController : Controller
    {


        private readonly AppDbContext _context;



        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Basit role kontrolü
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }


        //bu fonksiyon sayesinde temiz kod yazıldı adimn olmayınca direk giriş ekranına yönlendirir
        private IActionResult RedirectIfNotAdmin()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");
            return null;
        }

        // DASHBOARD GENEL RESOTANT SAYISSI KULLANICI SAYISI ŞİPARİŞ SAYISI GİBİ VERİLERİ ÇEKER
        public async Task<IActionResult> Home()
        {
            var redirect = RedirectIfNotAdmin(); // admin mi değil mi diye yukarıda yazılan kod için 
            if (redirect != null) return redirect;

            //model ile veirleri eşliyoruz
            var model = new AdminDashboardViewModel
            {
                //_contex ile veirtabanına ulaş ordan tabloya ulaş gibi devam ediyo
                TotalUsers = await _context.Users.CountAsync(),
                TotalRestaurants = await _context.Restaurants.CountAsync(),
                PendingRestaurantOwners = await _context.Users //restorant sahibi ve onay bekleyen kişi sayısı
                    .CountAsync(u => u.Role == "RestaurantOwner" && !u.IsApproved),
                TotalOrders = await _context.Orders.CountAsync(),
                TodayOrders = await _context.Orders
                    .CountAsync(o => o.OrderDate.Date == DateTime.Today)
            };

            ViewData["Title"] = "Dashboard";
            return View(model); //modeli döndür 
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  ONAY BEKLEYEN RESTORAN SAHİPLERİ İÇİN CONFİRM BUTONUNA TIKLAYINCA ÇALIŞIR VE ONAYLAR -----------------
        public async Task<IActionResult> RestaurantOwners()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            // Onay bekleyen restoran sahipleri
            var pending = await _context.Users
                .Where(u => u.Role == "RestaurantOwner" && !u.IsApproved)
                .ToListAsync();

            ViewData["Title"] = "Waiting restaurants";
            return View(pending);
        }

        // SP ile RESTORAN SAHİBİ ONAYLAMA
        [HttpPost]
        public async Task<IActionResult> ApproveOwner(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_ApproveRestaurantOwner @p0", id); ///burda sp kullanıldı onaylamayı bekleyen restoran sahiplerini onayladı 

            TempData["Message"] = "Restaurant owner account confirm.";
            return RedirectToAction("RestaurantOwners");
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //KULLANICI DETAYLARI YÖNETİMİ 
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Kullanıcılar";

            var data = await _context.Users.ToListAsync();  //kullanıcıları listeledi

            return View(data);
        }

        //kullanıcı detaylarını gösterir kaç yorumu var kimdir vs 
        public async Task<IActionResult> UserDetails(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            //BURDA USERDETAİLVİEEW KULLANARAK BİLGİLERİ DATA DEĞİŞKENİNE ATATIK
            var data = await _context.UserDetailView
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (data == null)
                return NotFound();

            ViewData["Title"] = "Kullanıcı Detayları";
            return View(data);
        }

        //KULLANICI SİLME detay sayfasından silme işlemi admin silinemez
        /// KULLANICI SİLME – admin silinemez
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            //  ADMIN KORUNUR adimni silmemize karşı çıkar
            if (user.Role == "Admin")
            {
                TempData["Message"] = "Admin account cannot be deleted!";
                return RedirectToAction("Users");
            }

            //restorant sahibini ise, sadece pasif yaparız
            if (user.Role == "RestaurantOwner")
            {
                user.IsApproved = false;
                var restaurants = await _context.Restaurants
                    .Where(r => r.UserId == id)
                    .ToListAsync();

                foreach (var r in restaurants)
                    r.IsOpen = false;

                await _context.SaveChangesAsync();

                TempData["Message"] = "Restaurant owner has been deactivated.";
                return RedirectToAction("Users");
            }

            // müsteriryi silme işlemi
            using var transaction = await _context.Database.BeginTransactionAsync();
             //usera bağlı her şeyi silerek hatayı ordadan kaldırıyoruz
            try
            {
                //  CART
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Cart WHERE UserId = @id",
                    new SqlParameter("@id", id)
                );

                // PAYMENT
                await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM Payment
            WHERE OrderId IN (SELECT OrderId FROM [Order] WHERE UserId = @id)",
                    new SqlParameter("@id", id)
                );
                // ORDER TRACKING 
                await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM OrderTracking
            WHERE OrderId IN (SELECT OrderId FROM [Order] WHERE UserId = @id)",
                    new SqlParameter("@id", id)
                );
                // 3 ORDER DETAIL
                await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM OrderDetail
            WHERE OrderId IN (SELECT OrderId FROM [Order] WHERE UserId = @id)",
                    new SqlParameter("@id", id)
                );

                //  ORDER
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [Order] WHERE UserId = @id",
                    new SqlParameter("@id", id)
                );

                // 5 USER (TRIGGER ÇALIŞACAK) bu çalışınca trigger devreye girip null yapacak
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [User] WHERE UserId = @id",
                    new SqlParameter("@id", id)
                );

                await transaction.CommitAsync();

                TempData["Message"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                //  ASIL DEBUG BURASI
                TempData["Message"] =
                    "DELETE ERROR 👉 " + ex.GetType().Name + " | " + ex.Message;

                // loglara da düşsün
                Console.WriteLine("DELETE USER ERROR:");
                Console.WriteLine(ex.ToString());
            }

            return RedirectToAction("Users");
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //RESTORANT YÖNETİMİ KISMI
        //restoranları listelleme kısmı
        public async Task<IActionResult> Restaurants()
        {
            var restaurants = await _context.Restaurants
                .Include(r => r.User)        // restoran sahibini getir
                .OrderBy(r => r.RestaurantName) //restorant adına göre sırala
                .ToListAsync();

            return View(restaurants);
        }

        // yemeklerin detaylarını gösterme kısmı
        public async Task<IActionResult> RestaurantFoods(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Foods.Where(f => f.IsAvailable)) // 🔥 BURASI
                .FirstOrDefaultAsync(r => r.RestaurantId == id);

            if (restaurant == null)
                return NotFound();

            var model = new AdminRestaurantDetailsViewModel
            {
                Restaurant = restaurant,
                Foods = restaurant.Foods.ToList()
            };

            return View(model);
        }
        //yemek silme kısmı
        [HttpPost]
        public async Task<IActionResult> DeleteFood(int foodId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Food SET IsAvailable = 0 WHERE FoodId = @id",
                new SqlParameter("@id", foodId)
            );

            TempData["success"] = "Food removed from menu.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //yorumları listeleme kısmı
        public async Task<IActionResult> Reviews()
        {
            //view modeli kullanarak yorumları çekme 
            var list = await _context.ReviewAdminView
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(list);
        }
        // yorum silme kısmı
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM ReviewReply WHERE ReviewId = @id;" +
                "DELETE FROM Review WHERE ReviewId = @id;",
                new SqlParameter("@id", id)
            );

            return RedirectToAction("Reviews");
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  BURASI DA SİPARİŞ DURUMLARINI GÖSTEREN KISIN
        public async Task<IActionResult> Orders()
        {
            //order tablosndan verileri çektik
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            //  dört gruba ayırarak daha kullanışılı bir yapı yapmak istedim
            var model = new OrderGroupedViewModel
            {
                Preparing = orders.Where(o => o.OrderStatus.StatusName == "Preparing").ToList(),
                OnTheWay = orders.Where(o => o.OrderStatus.StatusName == "On The Way").ToList(),
                Delivered = orders.Where(o => o.OrderStatus.StatusName == "Delivered").ToList(),
                Cancelled = orders.Where(o => o.OrderStatus.StatusName == "Cancelled").ToList(),
            };

            return View(model); // modele göndererek order.htlm sayfasında gösterdik 
        }

        // siparişlerin deyalarını görme şeysi
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderDetails)      // 
                    .ThenInclude(od => od.Food)    // her satırdaki yemeği de dahil et
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }


    }
}

