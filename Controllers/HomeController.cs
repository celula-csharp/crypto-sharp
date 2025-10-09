using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Data;
using SeguimientoCriptomonedas.Services;
using SeguimientoCriptomonedas.Models;

namespace SeguimientoCriptomonedas.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoinGeckoService _coinService;
        private readonly AppDbContext _context;

        public HomeController(CoinGeckoService coinService, AppDbContext context)
        {
            _coinService = coinService;
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            List<Coin> topCoins;
            List<int> favoriteCoinIds = new();

            try
            {
                topCoins = await _coinService.GetTopCoinsAsync(10);

                foreach (var coin in topCoins)
                {
                    var existing = await _context.Coins.FirstOrDefaultAsync(c => c.CoinGeckoId == coin.CoinGeckoId);

                    if (existing == null)
                    {
                        _context.Coins.Add(coin);
                    }
                    else
                    {
                        existing.Name = coin.Name;
                        existing.Symbol = coin.Symbol;
                        existing.CurrentPrice = coin.CurrentPrice;
                        existing.MarketCapRank = coin.MarketCapRank;
                        existing.PriceChange24h = coin.PriceChange24h;
                    }
                }

                await _context.SaveChangesAsync();

                var loggedUser = HttpContext.Session.GetString("LoggedUser");
                if (!string.IsNullOrEmpty(loggedUser))
                {
                    var user = await _context.Users.Include(u => u.FavoriteCoins)
                        .FirstOrDefaultAsync(u => u.Username == loggedUser);

                    if (user.FavoriteCoins != null)
                    {
                        favoriteCoinIds = user.FavoriteCoins.Select(f => f.CoinId).ToList();
                    }
                }
            }
            catch
            {
                topCoins = new List<Coin>();
            }

            ViewBag.FavoriteIds = favoriteCoinIds;

            Console.WriteLine(favoriteCoinIds.Count);
            return View(topCoins);
        }

    }
}