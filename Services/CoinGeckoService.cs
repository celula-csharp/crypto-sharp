using System.Text.Json;
using SeguimientoCriptomonedas.Models;

namespace SeguimientoCriptomonedas.Services
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient;

        public CoinGeckoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<List<Coin>> GetTopCoinsAsync(int count = 10)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={count}&page=1&sparkline=false";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var coins = JsonSerializer.Deserialize<List<CoinGeckoDto>>(json);

                return coins!.Select(c => new Coin
                {
                    Symbol = c.symbol,
                    Name = c.name,
                    CurrentPrice = c.current_price,
                    PriceChange24h = c.price_change_percentage_24h,
                    MarketCapRank = c.market_cap_rank,
                    LastUpdated = DateTime.UtcNow
                }).ToList();
            }
            catch
            {
                return new List<Coin>();
            }
        }
        
        public async Task<Coin?> GetCoinDataAsync(string symbol)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={symbol}&order=market_cap_desc&per_page=1&page=1&sparkline=false";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var coins = JsonSerializer.Deserialize<List<CoinGeckoDto>>(json);

                var c = coins!.FirstOrDefault();
                if (c == null) return null;

                return new Coin
                {
                    Symbol = c.symbol,
                    Name = c.name,
                    CurrentPrice = c.current_price,
                    PriceChange24h = c.price_change_percentage_24h,
                    MarketCapRank = c.market_cap_rank,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch
            {
                return null; // fallback si falla la API
            }
        }

        //  DTO interno para deserializar JSON de CoinGecko
        private class CoinGeckoDto
        {
            public string id { get; set; } = string.Empty;
            public string symbol { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public decimal current_price { get; set; }
            public decimal? price_change_percentage_24h { get; set; }
            public int? market_cap_rank { get; set; }
        }
    }
}
