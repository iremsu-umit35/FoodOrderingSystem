using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FoodOrderingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                             //REGİSTER KISMI
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        
        [HttpPost]
        //spregister user ile dataabase gelen verileri kaydettik 
        //buraya gelen veriler rehisterhtlmden geldi
        public IActionResult Register(User model)
        {
            Console.WriteLine(">>> POST Register tetiklendi!");
            Console.WriteLine($"FullName: {model.FullName}");
            Console.WriteLine($"Email: {model.Email}");
            // Console.WriteLine($"Password: {model.Password}"); 
            Console.WriteLine($"Phone: {model.Phone}");
            Console.WriteLine($"Address: {model.Address}");
            Console.WriteLine($"Role (gelen): {model.Role}");

            // 1. İstemci Tarafındaki Model Doğrulaması (Validation)
            if (!ModelState.IsValid)
            {
                Console.WriteLine(">>> ModelState HATALI! Hatalar:");

                foreach (var item in ModelState)
                {
                    if (item.Value.Errors.Count > 0)
                    {
                        Console.WriteLine(">>> MODEL ERROR: " + item.Key + " => " + item.Value.Errors[0].ErrorMessage);
                    }
                }

                // Hata varsa, kullanıcıya hatalı modeli geri gönder (View'da hatalar görünür)
                return View(model);
            }

            try
            {
                // 2. Özel Rol Kontrolü
                if (string.IsNullOrEmpty(model.Role))
                {
                    Console.WriteLine(">>> HATA: Rol seçilmemiş!");
                    // ModelState'e genel bir hata ekle
                    ModelState.AddModelError("", "Lütfen hesap türü seçiniz.");
                    return View(model);
                }

                // 3. Veritabanı İşlemi
                Console.WriteLine(">>> SP çağırılıyor...");
                Console.WriteLine($"SP Parametreleri => {model.FullName}, {model.Email}, {model.Password}, {model.Role}, {model.Phone}, {model.Address}");

                // Veritabanında Kayıt İşlemini Gerçekleştir (Strored Procedure çağrısı)
            
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_RegisterUser @FullName={0}, @Email={1}, @Password={2}, @Role={3}, @Phone={4}, @Address={5}",
                    model.FullName, model.Email, model.Password, model.Role, model.Phone, model.Address
                );

                Console.WriteLine(">>> SP başarıyla çalıştı!");

                // 4. Başarılı Mesajı ve Yönlendirme
                TempData["Message"] = model.Role == "RestaurantOwner"
                    ? "Registration successful! If you are a restaurant owner, admin approval is pending.."
                    : "Registration successful! You can log in..";

                Console.WriteLine(">>> Login sayfasına yönlendiriliyor...");
                // Kullanıcıyı Giriş (Login) sayfasına yönlendir
                return RedirectToAction("Login"); //loginhtlmle yönlendirildi
            }
            catch (Exception ex)
            {
                // 5. Hata Yakalama
                Console.WriteLine(">>> EXCEPTION YAKALANDI:");
                Console.WriteLine(ex.ToString());

                // Kullanıcıya bir hata mesajı göster
                ModelState.AddModelError("", "Kayıt sırasında hata oluştu: " + ex.Message);
                // Hatalı modeli View'a geri gönder
                return View(model);
            }
        }       
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
              //GİRİŞ KISMI 
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /*tarayıcıdan (Login formundan) login htlmden POST isteği geldiğinde çalışır ve kullanıcının
         * Email ve Password bilgilerini içeren LoginViewModel nesnesini girdi olarak alır.*/

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Email and password cannot be blank.!";
                return View();
            }

            /*(_context.Users) kullanılarak, veritabanında hem girilen
           * Email'e hem de girilen Password'a uyan ilk kullanıcı kaydı alınır*/
            // Kullanıcıyı email ve password ile bul

            var user = _context.Users
                .FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user == null)
            {
                ViewBag.Error = "Email or password is incorrect!";
                return View();
            }

            /*Eğer IsApproved False ise, kullanıcının sisteme giriş yapması
          * engellenir ve Admin onayı beklediği mesajı gösterilir.*/

            if (!user.IsApproved)
            {
                ViewBag.Error = "Your account has not yet been confirm.!";
                return View();
            }

            // Session kaydet  //Kullanıcı kimliğini doğrulamayı geçtikten sonra, durumu Oturum (Session) 
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            //test amaçlı verilerı konsola yazdırma
            var id = HttpContext.Session.GetInt32("UserId");
            var name = HttpContext.Session.GetString("UserFullName");
            var role = HttpContext.Session.GetString("UserRole");

            Console.WriteLine($"ID: {id}, Name: {name}, Role: {role}");


            //Başarıyla giriş yapan kullanıcıyı, sahip olduğu role uygun olan varsayılan anasayfasına yönlendirir.
            // Role göre yönlendirme
            switch (user.Role)
            {
                case "Customer":
                    return RedirectToAction("Home", "User"); // User/Home.cshtml
                case "RestaurantOwner":
                    return RedirectToAction("Dashboard", "RestaurantOwner"); // Restaurant/Home.cshtml
                case "Admin":
                    return RedirectToAction("Home", "Admin"); // Admin/Home.cshtml
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Oturumu temizle
            return RedirectToAction("Login");
        }

       

    }
}
