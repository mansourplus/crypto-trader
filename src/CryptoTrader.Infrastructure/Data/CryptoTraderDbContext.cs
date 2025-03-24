using System;
using Microsoft.EntityFrameworkCore;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Infrastructure.Data
{
    /// <summary>
    /// Contexte Entity Framework Core pour l'application CryptoTrader
    /// </summary>
    public class CryptoTraderDbContext : DbContext
    {
        public CryptoTraderDbContext(DbContextOptions<CryptoTraderDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet pour les actifs cryptographiques
        /// </summary>
        public DbSet<Asset> Assets { get; set; }

        /// <summary>
        /// DbSet pour les transactions
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// DbSet pour les stratégies de trading
        /// </summary>
        public DbSet<Strategy> Strategies { get; set; }

        /// <summary>
        /// Configuration du modèle de données
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'entité Asset
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Assets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18,8)");
                entity.Property(e => e.PriceChangePercentage24h).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MarketCap).HasColumnType("decimal(24,2)");
                entity.Property(e => e.Volume24h).HasColumnType("decimal(24,2)");
                entity.Property(e => e.CirculatingSupply).HasColumnType("decimal(24,8)");
                entity.Property(e => e.MaxSupply).HasColumnType("decimal(24,8)");
                entity.Property(e => e.ImageUrl).HasMaxLength(255);
                entity.HasIndex(e => e.Symbol).IsUnique();
            });

            // Configuration de l'entité Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,8)");
                entity.Property(e => e.Price).HasColumnType("decimal(18,8)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Fee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CoinbaseTransactionId).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
                
                // Relation avec Asset
                entity.HasOne(e => e.Asset)
                      .WithMany()
                      .HasForeignKey(e => e.AssetId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration de l'entité Strategy
            modelBuilder.Entity<Strategy>(entity =>
            {
                entity.ToTable("Strategies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Parameters).HasColumnType("jsonb");
                entity.Property(e => e.MaxInvestmentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TakeProfitPercentage).HasColumnType("decimal(10,2)");
                entity.Property(e => e.StopLossPercentage).HasColumnType("decimal(10,2)");
                
                // Conversion de la liste des AssetIds en JSON
                entity.Property(e => e.AssetIds)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => new List<Guid>(v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse))
                      );
            });
        }
    }
}
