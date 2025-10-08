using System.ComponentModel.DataAnnotations;

namespace SeguimientoCriptomonedas.Models
{
    public class Coin
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Symbol { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public decimal? CurrentPrice { get; set; }
        public decimal? PriceChange24h { get; set; }
        public int? MarketCapRank { get; set; }
        
        public DateTime? LastUpdated { get; set; }
        
        public ICollection<FavoriteCoin>? FavoriteCoins { get; set; }
    }
}