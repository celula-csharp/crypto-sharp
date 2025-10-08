using Microsoft.AspNetCore.Mvc;
using SeguimientoCriptomonedas.Services;

namespace SeguimientoCriptomonedas.Controllers
{
    public class DashboardController : Controller
    {
        private readonly CoinGeckoService _coinService;

        public DashboardController(CoinGeckoService coinService)
        {
            _coinService = coinService;
        }
        
        public async Task<IActionResult> Index()
        {
            var topCoins = await _coinService.GetTopCoinsAsync(10);

            return View(topCoins);
        }
    }
}