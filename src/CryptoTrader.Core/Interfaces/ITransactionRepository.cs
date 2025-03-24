using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface pour le repository des transactions
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Récupère toutes les transactions
        /// </summary>
        Task<IEnumerable<Transaction>> GetAllAsync();
        
        /// <summary>
        /// Récupère une transaction par son identifiant
        /// </summary>
        Task<Transaction> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Récupère les transactions d'un utilisateur
        /// </summary>
        Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Récupère les transactions pour un actif spécifique
        /// </summary>
        Task<IEnumerable<Transaction>> GetByAssetIdAsync(Guid assetId);
        
        /// <summary>
        /// Récupère les transactions générées par une stratégie spécifique
        /// </summary>
        Task<IEnumerable<Transaction>> GetByStrategyIdAsync(Guid strategyId);
        
        /// <summary>
        /// Récupère les transactions dans une plage de dates
        /// </summary>
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Ajoute une nouvelle transaction
        /// </summary>
        Task<Transaction> AddAsync(Transaction transaction);
        
        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        Task<bool> UpdateAsync(Transaction transaction);
        
        /// <summary>
        /// Supprime une transaction
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Récupère le solde total d'un utilisateur pour un actif spécifique
        /// </summary>
        Task<decimal> GetUserBalanceForAssetAsync(string userId, Guid assetId);
        
        /// <summary>
        /// Récupère le solde total d'un utilisateur pour tous les actifs
        /// </summary>
        Task<Dictionary<Guid, decimal>> GetUserBalancesAsync(string userId);
    }
}
