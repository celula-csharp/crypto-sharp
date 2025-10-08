using Microsoft.AspNetCore.Mvc;
using SeguimientoCriptomonedas.Services;
using SeguimientoCriptomonedas.Models;

namespace SeguimientoCriptomonedas.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoinGeckoService _coinService;

        public HomeController(CoinGeckoService coinService)
        {
            _coinService = coinService;
        }
        
        public async Task<IActionResult> Index()
        {
            List<Coin> topCoins;

            try
            {
                topCoins = await _coinService.GetTopCoinsAsync(10);
            }
            catch
            {
                topCoins = new List<Coin>();
            }

            return View(topCoins);
        }
    }
}