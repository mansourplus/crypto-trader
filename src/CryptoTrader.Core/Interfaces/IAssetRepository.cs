using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface pour le repository des actifs cryptographiques
    /// </summary>
    public interface IAssetRepository
    {
        /// <summary>
        /// Récupère tous les actifs
        /// </summary>
        Task<IEnumerable<Asset>> GetAllAsync();
        
        /// <summary>
        /// Récupère un actif par son identifiant
        /// </summary>
        Task<Asset> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Récupère un actif par son symbole
        /// </summary>
        Task<Asset> GetBySymbolAsync(string symbol);
        
        /// <summary>
        /// Récupère les actifs les plus performants selon un critère donné
        /// </summary>
        Task<IEnumerable<Asset>> GetTopPerformingAsync(int count, string criteria = "marketCap");
        
        /// <summary>
        /// Ajoute un nouvel actif
        /// </summary>
        Task<Asset> AddAsync(Asset asset);
        
        /// <summary>
        /// Met à jour un actif existant
        /// </summary>
        Task<bool> UpdateAsync(Asset asset);
        
        /// <summary>
        /// Supprime un actif
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Met à jour les prix et autres données de marché pour tous les actifs
        /// </summary>
        Task UpdateMarketDataAsync();
    }
}
