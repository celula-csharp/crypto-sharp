using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeguimientoCriptomonedas.Models
{
    public class FavoriteCoin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Coin")]
        public int CoinId { get; set; }

        public User? User { get; set; }
        public Coin? Coin { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}