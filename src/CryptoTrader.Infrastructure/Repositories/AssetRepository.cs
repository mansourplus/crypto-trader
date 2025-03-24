using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CryptoTrader.Core.Entities;
using CryptoTrader.Core.Interfaces;
using CryptoTrader.Infrastructure.Data;

namespace CryptoTrader.Infrastructure.Repositories
{
    /// <summary>
    /// Implémentation du repository pour les actifs cryptographiques
    /// </summary>
    public class AssetRepository : IAssetRepository
    {
        private readonly CryptoTraderDbContext _context;

        public AssetRepository(CryptoTraderDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Récupère tous les actifs
        /// </summary>
        public async Task<IEnumerable<Asset>> GetAllAsync()
        {
            return await _context.Assets.ToListAsync();
        }

        /// <summary>
        /// Récupère un actif par son identifiant
        /// </summary>
        public async Task<Asset> GetByIdAsync(Guid id)
        {
            return await _context.Assets.FindAsync(id);
        }

        /// <summary>
        /// Récupère un actif par son symbole
        /// </summary>
        public async Task<Asset> GetBySymbolAsync(string symbol)
        {
            return await _context.Assets
                .FirstOrDefaultAsync(a => a.Symbol.ToUpper() == symbol.ToUpper());
        }

        /// <summary>
        /// Récupère les actifs les plus performants selon un critère donné
        /// </summary>
        public async Task<IEnumerable<Asset>> GetTopPerformingAsync(int count, string criteria = "marketCap")
        {
            // Filtrage selon le critère spécifié
            IQueryable<Asset> query = _context.Assets;
            
            switch (criteria.ToLower())
            {
                case "marketcap":
                    query = query.OrderByDescending(a => a.MarketCap);
                    break;
                case "performance24h":
                    query = query.OrderByDescending(a => a.PriceChangePercentage24h);
                    break;
                case "volume":
                    query = query.OrderByDescending(a => a.Volume24h);
                    break;
                default:
                    query = query.OrderByDescending(a => a.MarketCap);
                    break;
            }
            
            return await query.Take(count).ToListAsync();
        }

        /// <summary>
        /// Ajoute un nouvel actif
        /// </summary>
        public async Task<Asset> AddAsync(Asset asset)
        {
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return asset;
        }

        /// <summary>
        /// Met à jour un actif existant
        /// </summary>
        public async Task<bool> UpdateAsync(Asset asset)
        {
            _context.Entry(asset).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AssetExistsAsync(asset.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Supprime un actif
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return false;
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Met à jour les prix et autres données de marché pour tous les actifs
        /// </summary>
        public async Task UpdateMarketDataAsync()
        {
            // Cette méthode sera implémentée avec l'intégration de l'API Coinbase
            // pour mettre à jour les données de marché en temps réel
            // Pour l'instant, c'est juste un placeholder
            await Task.CompletedTask;
        }

        /// <summary>
        /// Vérifie si un actif existe
        /// </summary>
        private async Task<bool> AssetExistsAsync(Guid id)
        {
            return await _context.Assets.AnyAsync(a => a.Id == id);
        }
    }
}
