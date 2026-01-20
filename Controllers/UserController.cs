using FoodOrderingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Execution;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FoodOrderingSystem.Models.Entities;
using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models.ViewModels;


namespace FoodOrderingSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        //database bağlan
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Customer") // sadece Customer girebilir
                return RedirectToAction("Login", "Account");

            return View(); // /User/Home.cshtml

        }
        // kullanıcı çıkış işlemi için yazılan fonksiyon
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // kullanıcı sipariş geçmişi için yazılan bir fonksiyon navbarda orders kısmına tıklayınca çalışır
        public async Task<IActionResult> Orders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Kullanıcının sipariş geçmişini getirir ve ilgili restoran ve sipariş durumu bilgilerini dahil eder
            var orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        // Şehir bazlı restoran arama işlemi user sayfasındaki şehirlerin
        // üzerine tıklayınca o şehire özel restoranrtlar fonksiyon ile sıralanır
        public IActionResult List(string city)
        {
            //  Role kontrolü (sadece Customer erişebilir)
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Customer")
                return RedirectToAction("Login", "Account");

            //  SQL fonksiyonunu kullanarak restoranları getir
            var restaurants = _context.Restaurants
                .FromSqlRaw("SELECT * FROM dbo.fn_SearchRestaurantsByCity({0})", city)
                .ToList();

            //  View'a şehir bilgisini gönder
            ViewData["City"] = city;

            //  Listeyi view'a gönder
            return View(restaurants); // /User/List.cshtml
        }



        // details ile tıklanan restoranın detayları gösterilir burda  sql fonksiyon calculatocartotal kullanılır 
        public async Task<IActionResult> Details(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Restoranı getir
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return NotFound();

            //  Restoranın menüsünü getir
            var foods = await _context.Foods
                .Where(f => f.RestaurantId == id && f.IsAvailable)
                .ToListAsync();

            //  Kullanıcının sepetini getir
            var cartItems = await (from c in _context.Carts
                                   join f in _context.Foods on c.FoodId equals f.FoodId
                                   where c.UserId == userId
                                   select new CartItemDTO
                                   {
                                       FoodId = c.FoodId,
                                       FoodName = f.FoodName,
                                       Price = f.Price,
                                       Quantity = c.Quantity
                                   }).ToListAsync();

            //  Sepet toplamını hesapla calculate cart total sp sile 
            decimal cartTotal = 0;
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT dbo.fn_CalculateCartTotal(@UserId)";
                var param = command.CreateParameter();
                param.ParameterName = "@UserId";
                param.Value = userId;
                command.Parameters.Add(param);

                await _context.Database.OpenConnectionAsync();
                var result = await command.ExecuteScalarAsync();

                if (result != DBNull.Value)
                    cartTotal = Convert.ToDecimal(result);

                await _context.Database.CloseConnectionAsync();
            }

            //  Restoranın yorumlarını getir manuel olarak çekiyor hata veiryodu çünkü 
                    var reviews = await _context.Reviews
             .Include(r => r.User)
             .Where(r => r.RestaurantId == id)
             .OrderByDescending(r => r.CreatedAt)
             .Select(r => new ReviewDTO
             {
                 ReviewId = r.ReviewId,
                 UserId = r.UserId,
                 UserFullName = r.User != null
                                ? r.User.FullName //trigger ile anonim yap
                                : "Anonim User", //burda kullanıcı silindiyse anonim kullanıcı olarak gözüküyor
                 RestaurantId = r.RestaurantId,
                 Rating = r.Rating,
                 Comment = r.Comment,
                 CreatedAt = r.CreatedAt,

                 //  RESTORAN CEVABI kullanıcıya verdiği cevap için eklenen kısım
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

                   // model.Reviews = reviews;
                    //  ViewModel oluştur
                    var model = new RestaurantDetailsViewModel
                    {
                        Restaurant = restaurant,
                        Foods = foods,
                        CartItems = cartItems,
                        CartTotal = cartTotal,
                        Reviews = reviews  
                    };

                    return View(model); // /User/Details.cshtml kullanılır
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            // Checkout GET’te ne yapıyorsan aynısı
            return await Checkout();
        }

        // ----------------------------------------------------------
        //burda + isaretine tıklayınca seçilen yemeğin sepete eklenemsi işlemi yapılır 
        //spaddtocar stored procedure kullanılır
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0; //burda kullanıcının ıdsini alırız sessiondan eğer yoksa 0 atarız
            if (userId == 0)
            {
                return Json(new { success = false, message = "Please, You must be sig in." }); //kullanıcı girişi yoksa hata mesajı döner
            }

            try
            {
                // addto cart işlemi için stored procedure çağrılır burda formdan gelen veriler kullanılır
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_AddToCart @p0, @p1, @p2", // sp kullanıldı
                    userId, dto.FoodId, 1
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //sepeti temizleme işlemi yapılır sp_clearcart stored procedure kullanılır
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            // Session'dan kullanıcı ID'sini doğrudan INT? olarak çek
            var userIdNullable = HttpContext.Session.GetInt32("UserId");

            // Eğer değer NULL ise veya geçerli değilse hata ver
            if (!userIdNullable.HasValue)
            {
                return Json(new { success = false, message = "The session expired or there was a user ID error.." });
            }
            // Değeri INT olarak al
            int userId = userIdNullable.Value;

            try
            {
                // sp_ClearCart prosedürünü çağırma burda sepet temizlenir 
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ClearCart @UserId", // sp ile sepeti temizle
                    new SqlParameter("UserId", userId)
                );

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // ----------------------------------------------------------
        // /User/Checkout  kullanıcı sepeti onaylayınca formdan gelen veriler burda işlenir
      //  KULLANICI ADRESİNİ VERİTABANINDAN GETİREN YARDIMCI METOT
            private async Task<string> GetUserAddress(int userId)
            {
                return await _context.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => u.Address)
                    .FirstOrDefaultAsync();
            }
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            // 1) Session üzerinden kullanıcı ID'sini al
            var userIdNullable = HttpContext.Session.GetInt32("UserId");

            if (!userIdNullable.HasValue) // kullamcı kontrolleri
                return RedirectToAction("Login", "Account");

            int userId = userIdNullable.Value;


            // 2) Sepet toplamını SQL fonksiyonundan doğru şekilde çek
            decimal total = 0; // toplam değişkeni

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                // Bağlantıyı aç
                if (command.Connection.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                // SQL fonksiyon çağrısı
                command.CommandText = "SELECT dbo.fn_CalculateCartTotal(@UserId)";
                command.CommandType = System.Data.CommandType.Text;

                // Parametre ekle
                var param = command.CreateParameter();
                param.ParameterName = "@UserId";
                param.Value = userId;
                command.Parameters.Add(param);

                // Sonucu al
                var result = await command.ExecuteScalarAsync();

                // NULL gelirse 0 atar
                total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }


            // 3) Sepet içeriklerini yiyecek ve restoran bilgisiyle birlikte al
            var cartItemsQuery = _context.Carts
                .Include(c => c.Food)
                .ThenInclude(f => f.Restaurant)
                .Where(c => c.UserId == userId);

            var firstCartItem = await cartItemsQuery.FirstOrDefaultAsync();

            if (firstCartItem == null)
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Home", "User");
            }


            // 4) CartItemDTO listesi oluştur
            var cartItemsList = await cartItemsQuery
                .Select(c => new CartItemDTO
                {
                    FoodId = c.FoodId,
                    FoodName = c.Food.FoodName,
                    Price = c.Food.Price,
                    Quantity = c.Quantity,
                   
                })
                .ToListAsync();


            // 5) Kullanıcının kayıtlı adresini çek (kullanıcı tablon üzerinden)
            var userAddress = await GetUserAddress(userId); // fomksiyon aşağıda bir yerde 

            // 6) ViewModel oluştur ve doldur ekranda gözükmesi için veriler 
            var model = new CheckoutViewModel
            {
                CartItems = cartItemsList,
                CartTotal = total,
                RestaurantId = firstCartItem.Food.RestaurantId,
                RestaurantName = firstCartItem.Food.Restaurant.RestaurantName,
                DeliveryAddress = userAddress ?? ""   // boş gelirse null hatasını önlüyor
            };

            return View(model);
        }


        //  POST: /User/Checkout post ile databese atılır burda sepetteki ürünü siparişe çevirme spsi kullanılır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ReloadCartDetailsForModel(model); // hata gelirse ViewModel'i tekrar doldur
                return View(model);
            }
            //kullancı girş yapmadı ise logine yolla
            var userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (!userIdNullable.HasValue)
                return RedirectToAction("Login", "Account");

            int userId = userIdNullable.Value;

            try
            {
                int createdOrderId = 0; // oluşturulan siparişin ID'si

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    // kullanıcı sepeti onaylayıp sipaiş oluşturma ve order tablosuna kayıtları eklme işlemi 
                    command.CommandText =
                        "EXEC sp_CreateOrderFromCart @UserId, @RestaurantId, @CourierId, @PaymentMethod";

                    command.CommandType = System.Data.CommandType.Text;

                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                    command.Parameters.Add(new SqlParameter("@RestaurantId", model.RestaurantId));
                    command.Parameters.Add(new SqlParameter("@CourierId", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PaymentMethod", model.PaymentMethod));

                    await _context.Database.OpenConnectionAsync();

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                        createdOrderId = Convert.ToInt32(result);

                    await _context.Database.CloseConnectionAsync();
                }

                // ⭐ Sipariş başarıyla oluştu order sayfasına yönlendir
                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu!";

                return RedirectToAction("Orders", "User");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + ex.Message);
                await ReloadCartDetailsForModel(model);
                return View(model);
            }
        }


        //  HATA OLURSA: ViewModel'i tekrar doldur bu sayede tekrardan başlar
        private async Task ReloadCartDetailsForModel(CheckoutViewModel model)
        {
            var userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (!userIdNullable.HasValue) return;

            int userId = userIdNullable.Value;


            var cartItemsQuery = _context.Carts
                .Include(c => c.Food)
                .ThenInclude(f => f.Restaurant)
                .Where(c => c.UserId == userId);

            model.CartItems = await cartItemsQuery
                .Select(c => new CartItemDTO
                {
                    FoodId = c.FoodId,
                    FoodName = c.Food.FoodName,
                    Price = c.Food.Price,
                    Quantity = c.Quantity,
                    
                })
                .ToListAsync();

            model.CartTotal = model.CartItems.Sum(i => i.Price * i.Quantity);

            var firstItem = await cartItemsQuery.FirstOrDefaultAsync();

            if (firstItem != null)
            {
                model.RestaurantId = firstItem.Food.RestaurantId;
                model.RestaurantName = firstItem.Food.Restaurant.RestaurantName;
            }

            // Kullanıcı adresini tekrar yükle
            model.DeliveryAddress = await GetUserAddress(userId) ?? "";
        }

        // ----------------------------------------------------------    
        ///KULLANIVI YORUM YAPMA KISMI burda da sp çalışıyor 
        ///
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewViewModel model)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return Json(new { success = false, message = "you must be login" });

            try
            {
                // kullancının yorum ekleme işlemi sp_addreview stored procedure kullanılır
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_AddReview @UserId, @RestaurantId, @Rating, @Comment",
                    new SqlParameter("UserId", userId),
                    new SqlParameter("RestaurantId", model.RestaurantId),
                    new SqlParameter("Rating", model.Rating),
                    new SqlParameter("Comment", model.Comment ?? "")
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}