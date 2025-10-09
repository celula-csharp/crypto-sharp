using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Data;
using SeguimientoCriptomonedas.Services;

namespace SeguimientoCriptomonedas.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CoinGeckoService _coinGeckoService;

        public DashboardController(AppDbContext context,  CoinGeckoService coinGeckoService)
        {
            _context = context;
            _coinGeckoService = coinGeckoService;
        }
        
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("LoggedUser");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.FavoriteCoins)
                .ThenInclude(f => f.Coin)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return Unauthorized();

            var favoriteCoins = user.FavoriteCoins.ToList(); // lista para la vista

            // IDs de CoinGecko (máximo 5 segun tu requisito)
            var ids = favoriteCoins
                .Select(f => f.Coin.CoinGeckoId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Take(5)
                .ToList();

            // Obtener precios seguros (API o fallback local)
            var realTimePrices = await _coinGeckoService.GetSafePricesAsync(ids);

            // Pasamos el diccionario a la vista
            ViewBag.RealTimePrices = realTimePrices;

            return View(favoriteCoins);
        }
    }
}