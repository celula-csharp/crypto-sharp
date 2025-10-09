using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Data;
using SeguimientoCriptomonedas.Models;

namespace SeguimientoCriptomonedas.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly AppDbContext _dbContext;

        public FavoritesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int coinId)
        {
            var username = HttpContext.Session.GetString("LoggedUser");
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();

            bool exists = await _dbContext.FavoriteCoins.AnyAsync(f => f.UserId == user.Id && f.CoinId == coinId);
            if (exists)
            {
                TempData["Message"] = "Esta moneda ya está en tus favoritos.";
                return RedirectToAction("Market", "Home");
            }

            var coin = await _dbContext.Coins.FirstOrDefaultAsync(c => c.Id == coinId);
            if (coin == null) return NotFound();

            var favorite = new FavoriteCoin
            {
                UserId = user.Id,
                CoinId = coin.Id,
            };

            _dbContext.FavoriteCoins.Add(favorite);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = $"{coin.Name} fue añadida a tus favoritos.";
            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(int coinId)
        {
            var username = HttpContext.Session.GetString("LoggedUser");
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();

            var favorite = await _dbContext.FavoriteCoins
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.CoinId == coinId);

            if (favorite == null)
            {
                TempData["Message"] = "Esta moneda no está en tus favoritos.";
                return RedirectToAction("Index", "Home");
            }

            _dbContext.FavoriteCoins.Remove(favorite);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = "Moneda eliminada de tus favoritos.";
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("LoggedUser");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();

            var favorites = await _dbContext.FavoriteCoins
                .Where(f => f.UserId == user.Id)
                .ToListAsync();

            return View(favorites);
        }
    }
}
