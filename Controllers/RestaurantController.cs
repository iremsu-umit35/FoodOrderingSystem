using FoodOrderingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly AppDbContext _context;

        public RestaurantController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult List(string city)
        {
            // SQL fonksiyonuna parametre
            var cityParam = new SqlParameter("@City", city ?? (object)DBNull.Value);

            // Fonksiyon çağrılıyor
            var restaurants = _context.Restaurants
                .FromSqlRaw("SELECT * FROM dbo.fn_SearchRestaurantsByCity(@City)", cityParam)
                .ToList();

            ViewBag.City = city;
            return View(restaurants);
        }
    }

}
