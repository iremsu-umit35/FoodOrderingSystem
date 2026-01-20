using FoodOrderingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // ⭐ Sipariş Takip Sayfası sipaişleirm kısmında kullanıcıların siparişlerinin durumunu
        public async Task<IActionResult> Tracking(int orderId)
        {
            // 1) Kullanıcı giriş kontrolü
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // 2) Bu sipariş gerçekten bu kullanıcıya mı ait kontrol et
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId.Value);

            if (order == null)
                return RedirectToAction("Home", "User");

            // 3) Tracking kayıtlarını getir
            var trackingHistory = await _context.OrderTracking
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.TrackingTime)
                .ToListAsync();

            return View(trackingHistory);
        }
    }
}
