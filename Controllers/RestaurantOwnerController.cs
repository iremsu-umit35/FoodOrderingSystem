using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models.DTOs;
using FoodOrderingSystem.Models.Entities;
using FoodOrderingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FoodOrderingSystem.Controllers
{
    public class RestaurantOwnerController : Controller
    {
        private readonly AppDbContext _context;

        public RestaurantOwnerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ---------------------------------------------------------------------------------------
        // restautant OWNER DASHBOARD

        public async Task<IActionResult> Dashboard()
        {
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (ownerId == 0)
                return RedirectToAction("Login", "Account");

            //  Owner’a ait restoranı bul
            // Owner’a ait restoranı bul
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.UserId == ownerId);

            if (restaurant == null)
                return RedirectToAction("CreateRestaurant");  // ilk defa giriş yapıoyrsa create restaurant a yönlendir



            int restId = restaurant.RestaurantId; // Restoran ID'si restId değişkenine atanır

            // Günlük Sipariş Sayısı
            // Mutlak AS takma adı kullanıldı
            int todayOrders = await _context.Database
                .SqlQuery<int>($"SELECT dbo.fn_GetRestaurantDailyOrderCount({restId}, GETDATE()) AS TotalOrders")
                .ToListAsync() // Listeye çevirip ilk öğeyi alıyoruz
                .ContinueWith(t => t.Result.FirstOrDefault());

            //  Günlük Toplam Gelir
            // Mutlak AS takma adı kullanıldı
            decimal todayRevenue = await _context.Database
                .SqlQuery<decimal>($"SELECT dbo.fn_GetRestaurantDailyRevenue({restId}, GETDATE()) AS TotalRevenue")
                .ToListAsync() // Listeye çevirip ilk öğeyi alıyoruz
                .ContinueWith(t => t.Result.FirstOrDefault());
            // ...

            //  Toplam sipariş sayısı
            int totalOrders = await _context.Orders
                .CountAsync(o => o.RestaurantId == restId);

            // Ortalama Puan
            decimal avgRating = restaurant.AverageRating;

            //  Son 5 sipariş
            var latestOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .Where(o => o.RestaurantId == restId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            //  VIEWMODEle değişkenleri aktar
            var vm = new OwnerDashboardViewModel
            {
                TodayOrders = todayOrders,
                TodayRevenue = todayRevenue,
                TotalOrders = totalOrders,
                AverageRating = avgRating,
                LatestOrders = latestOrders
            };

            return View(vm); //vies modeli gönder html e
        }

       
        // RESTORANT SAHİBİ iLK DEFA GİRŞ YAPIyorsa
            public IActionResult CreateRestaurant()
        {
            return View();
        }

        // restornt sahibi ilk girişte bu sayfaya yönlendirilir bu  sayede restoranını oluşturur ve direkt restorant arayüzüne yönlendirilir
        [HttpPost]
        public async Task<IActionResult> CreateRestaurant(Restaurant model)
        {
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            model.UserId = ownerId;

            _context.Restaurants.Add(model); // formdan gelen verileri veri tabanına ekledi
            await _context.SaveChangesAsync();

            TempData["success"] = "Restaurant created!";

            return RedirectToAction("Dashboard");//arayüze yönlendirildi
        }


        //RETORANT OWNER SETTINGS KISMI BURASI 
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0; // Oturumdan UserId al ownerId değişkenine ata

            var restaurant = await _context.Restaurants // Owner'ın restoranını bul 
                .FirstOrDefaultAsync(r => r.UserId == ownerId);

            if (restaurant == null) // Restoran bulunamazsa restoran bulunamadı mesajı döndür
                return Content("Restoran bulunamadı!");

            var vm = new OwnerRestaurantSettingsViewModel  // ViewModel oluştur ve restoran bilgilerini ata
            {
                RestaurantId = restaurant.RestaurantId,
                RestaurantName = restaurant.RestaurantName,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                Description = restaurant.Description,
                MinOrderAmount = restaurant.MinOrderAmount,
                IsOpen = restaurant.IsOpen??false
            };

            return View(vm); // ViewModel'i View'a gönder
        }


        // ayarlar post kısmı burda değişiklikleri kaydetmek için
        [HttpPost]
        public async Task<IActionResult> Settings(OwnerRestaurantSettingsViewModel model)
        {
            if (!ModelState.IsValid) // Model doğrulaması başarısızsa aynı ViewModel ile View'a geri dön
                return View(model);

            var restaurant = await _context.Restaurants.FindAsync(model.RestaurantId); // Restoranı veritabanında bul restaurantId ile restaurant değişkenine ata

            if (restaurant == null)
                return Content("Restoran bulunamadı!");

            // SADECE DB'DEKİ VERİLERİ GÜNCELLE modelden gelen verilerle tablo verilerini güncelle
            restaurant.RestaurantName = model.RestaurantName;
            restaurant.Address = model.Address;
            restaurant.Phone = model.Phone;
            restaurant.Description = model.Description;
            restaurant.MinOrderAmount = model.MinOrderAmount;
            restaurant.IsOpen = model.IsOpen;

            await _context.SaveChangesAsync(); // Değişiklikleri veritabanına kaydet

            TempData["success"] = "Success changes updates!";

            return RedirectToAction("Settings"); // Ayarlar sayfasına yönlendir
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //RESTORANT YEMEKLERİ GÜNCELLEME EKLEME SİLME KISMI BURASI
        //YEMEKLERİ LİSTELEME KISMI
        public async Task<IActionResult> Foods()
        {
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.UserId == ownerId);

            var foods = await _context.Foods
            .Where(f => f.RestaurantId == restaurant.RestaurantId
             && f.IsAvailable)
             .ToListAsync();


            return View(foods);
        }

        // FOOD EKLEME KISMI
        [HttpGet]
        public IActionResult AddFood()
        {
            return View();
        }
        // FOOD EKLEME POST KISMI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFood(AddFoodDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Model validation failed";
                return View(model);
            }

            //  TR formatlı fiyatı decimale çevir
            if (!decimal.TryParse(
                    model.Price,
                    NumberStyles.Any,
                    new CultureInfo("tr-TR"),
                    out decimal price))
            {
                ModelState.AddModelError("Price", "Geçerli bir fiyat giriniz (örn: 200,00)");
                return View(model);
            }

            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.UserId == ownerId);

            if (restaurant == null)
            {
                TempData["error"] = "Restaurant not found";
                return RedirectToAction("Foods");
            }

            //  Görsel istemiyoruz default
            string imageUrl = "/images/default-food.jpg";

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_AddFood @p0, @p1, @p2, @p3, @p4", //sp kullanıldı gene
                restaurant.RestaurantId,
                model.FoodName,
                model.Description ?? "",
                price,
                imageUrl
            );

            TempData["success"] = "Food added!";
            return RedirectToAction("Foods");
        }


        // food güncelleme kısmı
        /*
        public async Task<IActionResult> EditFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            return View(food);
        }*/

        public async Task<IActionResult> EditFood(int id)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.FoodId == id);

            if (food == null)
                return NotFound();

            var history = await _context.FoodPriceHistory
                .Where(x => x.FoodId == id)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync();

            var model = new FoodEditViewModel
            {
                Food = food,
                PriceHistory = history
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditFood(FoodEditViewModel model, IFormFile imageFile)
        {
            // Model doğrulama ve resim yükleme kodları burada kalır

            string imageUrl = model.Food.ImageUrl;

            if (imageFile != null)
            {
                // Resim yükleme ve imageUrl'i ayarlama kodları
                imageUrl = "/images/" + imageFile.FileName;
            }

            //  PARAMETRELER KULLANILARAK GÜVENLİ ÇAĞRI
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"EXEC sp_UpdateFood @FoodId, @FoodName, @Description, @Price, @ImageUrl",
                  new SqlParameter("FoodId", model.Food.FoodId),
            new SqlParameter("FoodName", model.Food.FoodName),
            new SqlParameter("Description", model.Food.Description),
            new SqlParameter("Price", model.Food.Price),
            new SqlParameter("ImageUrl", imageUrl)
                );

                TempData["success"] = "Food updated!";
                return RedirectToAction("Foods");
            }
            catch (Exception ex)
            {
                // Hata durumunda ne olduğunu konsola yazdırın
                ModelState.AddModelError("", "Güncelleme başarısız oldu. Hata detayı: " + ex.Message);

                // fiyat geçmişini tekrar doldur
                model.PriceHistory = await _context.FoodPriceHistory
                    .Where(x => x.FoodId == model.Food.FoodId)
                    .OrderByDescending(x => x.ChangedAt)
                    .ToListAsync();
                return View(model);
            }
        }

        // food silme kısmı
        [HttpPost]
        public async Task<IActionResult> DeleteFood(int id)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Food SET IsAvailable = 0 WHERE FoodId = @id",
                new SqlParameter("@id", id)
            );

            TempData["success"] = "Food removed from menu.";
            return RedirectToAction("Foods");
        }

        ///////////////////////////////////////////////////////////////////////////////////

        /// Yorumlar kımmı gelen yorumları cevaplıyacak
        public async Task<IActionResult> Reviews()
        {
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.UserId == ownerId);

            if (restaurant == null)
                return Content("Restoran bulunamadı!");

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.RestaurantId == restaurant.RestaurantId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    UserFullName = r.User != null ? r.User.FullName : "Anonim", //anonim kullanıcıyı
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,

                    // Reply bilgisi:
                    ReplyText = _context.ReviewReply
                                 .Where(x => x.ReviewId == r.ReviewId)
                                 .Select(x => x.ReplyText)
                                 .FirstOrDefault(),

                    RepliedAt = _context.ReviewReply
                                 .Where(x => x.ReviewId == r.ReviewId)
                                 .Select(x => x.RepliedAt)
                                 .FirstOrDefault()
                })
                .ToListAsync();

            return View(reviews);
        }

        //yorum yazma kısmı
        [HttpPost]
        public async Task<IActionResult> AddReply(int ReviewId, string ReplyText)
        {
            await _context.ReviewReply.AddAsync(new ReviewReply
            {
                ReviewId = ReviewId,
                ReplyText = ReplyText,
                RepliedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Reviews");
        }



  
        // ////////////////////////////////////////////////////////////////////////////////////////////
        // restorant OWNER ORDERS kısmı siparişleri 
        /*Restoran sahibinin restoranını bulur
         YALNIZCA o restorana ait siparişleri getirir
         Kullanıcı bilgisi + sipariş durumu + tarih gelir*/
        public async Task<IActionResult> Orders()
        {
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Owner’ın restoranını al
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.UserId == ownerId);

            if (restaurant == null)
                return Content("Restoran bulunamadı!");

            // Restorana ait siparişleri çek
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .Where(o => o.RestaurantId == restaurant.RestaurantId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        /*RESTORAN SAHİBİ TEK TIKLA:
        Preparing → On The Way
        On The Way → Delivered
        Delivered → LOCKED (hiçbir şey yapılmaz)*/
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest req)
        {
            //  Bu satır Output penceresine yazdık hata vardı kontrol etmek için
            Console.WriteLine($"[DEBUG] UpdateOrderStatus() -> orderId = {req.orderId}, newStatusId = {req.newStatusId}");

            if (req.orderId <= 0 || req.newStatusId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Gönderilen orderId veya newStatusId geçersiz (0 geliyor)."
                });
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UpdateOrderStatus @OrderId, @NewStatusId", //sp kullanıldı siparis durumunu güncelleme spsi önemlidir
                    new SqlParameter("@OrderId", req.orderId),
                    new SqlParameter("@NewStatusId", req.newStatusId)
                );

                Console.WriteLine("[DEBUG] Order status successfully updated!");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] SQL Error: " + ex.Message);

                return Json(new
                {
                    success = false,
                    message = "SQL Hatası: " + ex.Message
                });
            }
        }
        //sipariş detayların ıgösterme kısmı
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            var items = await _context.OrderDetails
                .Include(od => od.Food)
                .Where(od => od.OrderId == id)
                .ToListAsync();

            var tracking = await _context.OrderTracking
                .Where(t => t.OrderId == id)
                .OrderBy(t => t.TrackingTime)
                .ToListAsync();

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == id);

            var vm = new OwnerOrderDetailViewModel
            {
                Order = order,
                Items = items,
                Tracking = tracking,
                Payment = payment
            };
            return View(vm);
        }
    }
}
