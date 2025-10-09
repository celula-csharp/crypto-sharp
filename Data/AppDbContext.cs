using Microsoft.EntityFrameworkCore;
using SeguimientoCriptomonedas.Models;

namespace SeguimientoCriptomonedas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Coin> Coins { get; set; }
        public DbSet<FavoriteCoin> FavoriteCoins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FavoriteCoin>()
                .HasOne(fc => fc.User)
                .WithMany(u => u.FavoriteCoins)
                .HasForeignKey(fc => fc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteCoin>()
                .HasOne(fc => fc.Coin)
                .WithMany(c => c.FavoriteCoins)
                .HasForeignKey(fc => fc.CoinId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteCoin>()
                .HasIndex(fc => new { fc.UserId, fc.CoinId })
                .IsUnique();
        }
    }
}