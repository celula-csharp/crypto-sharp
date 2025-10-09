using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Data;
using SeguimientoCriptomonedas.Models;

namespace SeguimientoCriptomonedas.Services
{
    public class CoinGeckoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public CoinGeckoService(HttpClient httpClient, AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClient;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // Obtiene las top N monedas
        public async Task<List<Coin>> GetTopCoinsAsync(int count = 10)
        {
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={count}&page=1&sparkline=false";

            try
            {
                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"Status code: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine("Api limit reached. Error 429. Too Many Requests.");
                    return await _context.Coins
                        .OrderBy(c => c.MarketCapRank)
                        .Take(count)
                        .ToListAsync();
                }
                
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
                    Console.WriteLine("No se obtuvieron monedas de CoinGecko, devolviendo base de datos local.");
                    return await _context.Coins
                        .OrderBy(c => c.MarketCapRank)
                        .Take(count)
                        .ToListAsync();
                }

                return coins.Select(c => new Coin
                {
                    Symbol = c.symbol.ToUpper(),
                    Name = c.name,
                    Image = c.image,
                    CurrentPrice = c.current_price,
                    PriceChange24h = c.price_change_percentage_24h,
                    MarketCapRank = c.market_cap_rank,
                    LastUpdated = DateTime.UtcNow,
                    CoinGeckoId = c.id
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

        public async Task<List<CoinPriceResult>> GetFavoritePricesAsync(List<string> coinGekoIds)
        {
            var tasks = coinGekoIds.Select(async id =>
            {
                var url = $"https://api.coingecko.com/api/v3/simple/price?ids={id}&vs_currencies=usd";
                var response = await _httpClient.GetStringAsync(url);

                return new CoinPriceResult
                {
                    CoinId = id,
                    PriceJson = response
                };
            });

            return (await Task.WhenAll(tasks)).ToList();
        }
        
    private async Task<decimal?> FetchPriceFromApiAsync(string coinGeckoId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.coingecko.com/api/v3/simple/price?ids={coinGeckoId}&vs_currencies=usd";

            using var resp = await client.GetAsync(url);

            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return null;
            }

            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty(coinGeckoId, out var coinElement)
                && coinElement.TryGetProperty("usd", out var usdElement))
            {
                if (usdElement.TryGetDecimal(out var price))
                    return price;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
    public async Task<Dictionary<string, decimal?>> GetSafePricesAsync(List<string> coinGeckoIds)
    {
        var fetchTasks = coinGeckoIds
            .Select(async id =>
            {
                var price = await FetchPriceFromApiAsync(id);
                return (Id: id, Price: price);
            })
            .ToList();

        var fetchResults = await Task.WhenAll(fetchTasks);

        var result = fetchResults.ToDictionary(r => r.Id, r => r.Price);

        var failedIds = result.Where(kv => kv.Value == null).Select(kv => kv.Key).ToList();
        if (failedIds.Any())
        {
            var localCoins = await _context.Coins
                .Where(c => failedIds.Contains(c.CoinGeckoId))
                .ToListAsync();

            foreach (var id in failedIds)
            {
                var local = localCoins.FirstOrDefault(c => c.CoinGeckoId == id);
                if (local != null)
                {
                    result[id] = local.CurrentPrice;
                }
                else
                {
                    result[id] = null;
                }
            }
        }

        var onlineResults = result.Where(kv => kv.Value != null).ToList();

        if (onlineResults.Any())
        {
            foreach (var kv in onlineResults)
            {
                var id = kv.Key;
                var price = kv.Value.Value;

                var coin = await _context.Coins.FirstOrDefaultAsync(c => c.CoinGeckoId == id);
                if (coin != null)
                {
                    coin.CurrentPrice = price;
                }
                else
                {
                    _context.Coins.Add(new Coin
                    {
                        CoinGeckoId = id,
                        Name = id,
                        Symbol = id,
                        CurrentPrice = price
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        return result;
    }


        public Dictionary<string, decimal> ParseFavoritePrices(List<CoinPriceResult> prices)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var p in prices)
            {
                try
                {
                    using var doc = JsonDocument.Parse(p.PriceJson);
                    var price = doc.RootElement.GetProperty(p.CoinId).GetProperty("usd").GetDecimal();
                    result[p.CoinId] = price;
                }
                catch
                {
                    result[p.CoinId] = 0;
                }
            }

            return result;
        }

        public class CoinPriceResult
        {
            public string CoinId { get; set; }
            public string PriceJson { get; set; }
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
