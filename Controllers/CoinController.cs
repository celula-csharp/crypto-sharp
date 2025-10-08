using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Data;
using SeguimientoCriptomonedas.Models;
using SeguimientoCriptomonedas.Services;

namespace SeguimientoCriptomonedas.Controllers
{
    public class CoinsController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly CoinGeckoService _coinService;

        public CoinsController(AppDbContext dbContext, CoinGeckoService coinService)
        {
            _dbContext = dbContext;
            _coinService = coinService;
        }

        public async Task<IActionResult> Index()
        {
            //  Sección de prueba: crear datos iniciales si la DB está vacía
            //  Esta sección se borrará después, solo para testing
            if (!_dbContext.Users.Any())
            {
                var user = new User { Username = "osita", Email = "osita@email.com", PasswordHash = "1234" };
                _dbContext.Users.Add(user);

                var btc = new Coin 
                { 
                    Symbol = "BTC", 
                    Name = "Bitcoin", 
                    CoinGeckoId = "bitcoin",
                    CurrentPrice = 0, 
                    LastUpdated = DateTime.UtcNow 
                };

                var eth = new Coin 
                { 
                    Symbol = "ETH", 
                    Name = "Ethereum", 
                    CoinGeckoId = "ethereum",
                    CurrentPrice = 0, 
                    LastUpdated = DateTime.UtcNow 
                };

                _dbContext.Coins.AddRange(btc, eth);

                _dbContext.FavoriteCoins.AddRange(
                    new FavoriteCoin { User = user, Coin = btc },
                    new FavoriteCoin { User = user, Coin = eth }
                );

                await _dbContext.SaveChangesAsync();
            }
            
            var favoriteCoins = await _dbContext.FavoriteCoins
                .Include(fc => fc.Coin)
                .Include(fc => fc.User)
                .ToListAsync();

            var coinsToUpdate = favoriteCoins
                .Select(fc => fc.Coin!)
                .Distinct()
                .ToList();
            
            var tasks = coinsToUpdate.Select(c => _coinService.GetCoinDataAsync(c.CoinGeckoId));
            var results = await Task.WhenAll(tasks);

            for (int i = 0; i < coinsToUpdate.Count; i++)
            {
                var coin = coinsToUpdate[i];
                var updatedCoin = results[i];

                if (updatedCoin != null)
                {
                    coin.CurrentPrice = updatedCoin.CurrentPrice ?? 0m;
                    coin.PriceChange24h = updatedCoin.PriceChange24h ?? 0m;
                    coin.MarketCapRank = updatedCoin.MarketCapRank ?? 0;
                    coin.LastUpdated = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();

            return View(coinsToUpdate);
        }
    }
}
