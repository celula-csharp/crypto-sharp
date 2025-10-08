using System.ComponentModel.DataAnnotations;

namespace SeguimientoCriptomonedas.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public ICollection<FavoriteCoin>? FavoriteCoins { get; set; }
    }
}