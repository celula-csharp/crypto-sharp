using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeguimientoCriptomonedas.Models
{
    public class FavoriteCoin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CoinId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        [ForeignKey(nameof(CoinId))]
        public Coin? Coin { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}