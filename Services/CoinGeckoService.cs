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

        // Obtiene las top N monedas
        public async Task<List<Coin>> GetTopCoinsAsync(int count = 10)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={count}&page=1&sparkline=false";

            try
            {
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"Status code: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("JSON recibido: " + json.Substring(0, Math.Min(200, json.Length)));

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var coins = JsonSerializer.Deserialize<List<CoinGeckoDto>>(json, options);

                if (coins == null || !coins.Any())
                {
                    Console.WriteLine("No se obtuvieron monedas de CoinGecko");
                    return new List<Coin>();
                }

                return coins.Select(c => new Coin
                {
                    Symbol = c.symbol.ToUpper(),
                    Name = c.name,
                    Image = c.image,
                    CurrentPrice = c.current_price,
                    PriceChange24h = c.price_change_percentage_24h,
                    MarketCapRank = c.market_cap_rank,
                    LastUpdated = DateTime.UtcNow
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en GetTopCoinsAsync: " + ex.Message);
                return new List<Coin>();
            }
        }

        // Obtiene los datos de una moneda específica
        public async Task<Coin?> GetCoinDataAsync(string coinGeckoId)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={coinGeckoId}&order=market_cap_desc&per_page=1&page=1&sparkline=false";

            try
            {
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"Status code (GetCoinDataAsync): {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("JSON recibido (GetCoinDataAsync): " + json.Substring(0, Math.Min(200, json.Length)));

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var coins = JsonSerializer.Deserialize<List<CoinGeckoDto>>(json, options);

                var c = coins?.FirstOrDefault();
                if (c == null)
                {
                    Console.WriteLine($"No se encontró la moneda con id {coinGeckoId}");
                    return null;
                }

                return new Coin
                {
                    Symbol = c.symbol.ToUpper(),
                    Name = c.name,
                    CurrentPrice = c.current_price,
                    PriceChange24h = c.price_change_percentage_24h,
                    MarketCapRank = c.market_cap_rank,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en GetCoinDataAsync: " + ex.Message);
                return null;
            }
        }

        // DTO interno para deserializar JSON de CoinGecko
        private class CoinGeckoDto
        {
            public string id { get; set; } = string.Empty;
            public string symbol { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public string image { get; set; } = string.Empty;
            public decimal current_price { get; set; }
            public decimal? price_change_percentage_24h { get; set; }
            public int? market_cap_rank { get; set; }
        }
    }
}
