using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface pour le repository des stratégies de trading
    /// </summary>
    public interface IStrategyRepository
    {
        /// <summary>
        /// Récupère toutes les stratégies
        /// </summary>
        Task<IEnumerable<Strategy>> GetAllAsync();
        
        /// <summary>
        /// Récupère une stratégie par son identifiant
        /// </summary>
        Task<Strategy> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Récupère les stratégies d'un utilisateur
        /// </summary>
        Task<IEnumerable<Strategy>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Récupère les stratégies par type
        /// </summary>
        Task<IEnumerable<Strategy>> GetByTypeAsync(StrategyType type);
        
        /// <summary>
        /// Récupère les stratégies par statut
        /// </summary>
        Task<IEnumerable<Strategy>> GetByStatusAsync(StrategyStatus status);
        
        /// <summary>
        /// Récupère les stratégies associées à un actif spécifique
        /// </summary>
        Task<IEnumerable<Strategy>> GetByAssetIdAsync(Guid assetId);
        
        /// <summary>
        /// Ajoute une nouvelle stratégie
        /// </summary>
        Task<Strategy> AddAsync(Strategy strategy);
        
        /// <summary>
        /// Met à jour une stratégie existante
        /// </summary>
        Task<bool> UpdateAsync(Strategy strategy);
        
        /// <summary>
        /// Supprime une stratégie
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Change le statut d'une stratégie
        /// </summary>
        Task<bool> ChangeStatusAsync(Guid id, StrategyStatus newStatus);
        
        /// <summary>
        /// Met à jour la date de dernière exécution d'une stratégie
        /// </summary>
        Task<bool> UpdateLastExecutionTimeAsync(Guid id, DateTime executionTime);
        
        /// <summary>
        /// Récupère les stratégies qui doivent être exécutées (basé sur leur fréquence d'exécution)
        /// </summary>
        Task<IEnumerable<Strategy>> GetStrategiesToExecuteAsync();
    }
}
