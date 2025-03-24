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
    /// Implémentation du repository pour les stratégies de trading
    /// </summary>
    public class StrategyRepository : IStrategyRepository
    {
        private readonly CryptoTraderDbContext _context;

        public StrategyRepository(CryptoTraderDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Récupère toutes les stratégies
        /// </summary>
        public async Task<IEnumerable<Strategy>> GetAllAsync()
        {
            return await _context.Strategies.ToListAsync();
        }

        /// <summary>
        /// Récupère une stratégie par son identifiant
        /// </summary>
        public async Task<Strategy> GetByIdAsync(Guid id)
        {
            return await _context.Strategies.FindAsync(id);
        }

        /// <summary>
        /// Récupère les stratégies d'un utilisateur
        /// </summary>
        public async Task<IEnumerable<Strategy>> GetByUserIdAsync(string userId)
        {
            return await _context.Strategies
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les stratégies par type
        /// </summary>
        public async Task<IEnumerable<Strategy>> GetByTypeAsync(StrategyType type)
        {
            return await _context.Strategies
                .Where(s => s.Type == type)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les stratégies par statut
        /// </summary>
        public async Task<IEnumerable<Strategy>> GetByStatusAsync(StrategyStatus status)
        {
            return await _context.Strategies
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les stratégies associées à un actif spécifique
        /// </summary>
        public async Task<IEnumerable<Strategy>> GetByAssetIdAsync(Guid assetId)
        {
            return await _context.Strategies
                .Where(s => s.AssetIds.Contains(assetId))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Ajoute une nouvelle stratégie
        /// </summary>
        public async Task<Strategy> AddAsync(Strategy strategy)
        {
            strategy.CreatedAt = DateTime.UtcNow;
            strategy.UpdatedAt = DateTime.UtcNow;
            
            _context.Strategies.Add(strategy);
            await _context.SaveChangesAsync();
            return strategy;
        }

        /// <summary>
        /// Met à jour une stratégie existante
        /// </summary>
        public async Task<bool> UpdateAsync(Strategy strategy)
        {
            strategy.UpdatedAt = DateTime.UtcNow;
            _context.Entry(strategy).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await StrategyExistsAsync(strategy.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Supprime une stratégie
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var strategy = await _context.Strategies.FindAsync(id);
            if (strategy == null)
            {
                return false;
            }

            _context.Strategies.Remove(strategy);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Change le statut d'une stratégie
        /// </summary>
        public async Task<bool> ChangeStatusAsync(Guid id, StrategyStatus newStatus)
        {
            var strategy = await _context.Strategies.FindAsync(id);
            if (strategy == null)
            {
                return false;
            }

            strategy.Status = newStatus;
            strategy.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Met à jour la date de dernière exécution d'une stratégie
        /// </summary>
        public async Task<bool> UpdateLastExecutionTimeAsync(Guid id, DateTime executionTime)
        {
            var strategy = await _context.Strategies.FindAsync(id);
            if (strategy == null)
            {
                return false;
            }

            strategy.LastExecutionTime = executionTime;
            strategy.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Récupère les stratégies qui doivent être exécutées (basé sur leur fréquence d'exécution)
        /// </summary>
        public async Task<IEnumerable<Strategy>> GetStrategiesToExecuteAsync()
        {
            var now = DateTime.UtcNow;
            
            return await _context.Strategies
                .Where(s => s.Status == StrategyStatus.Active &&
                           (s.LastExecutionTime == null ||
                            (s.ExecutionFrequencyMinutes != null &&
                             s.LastExecutionTime.Value.AddMinutes(s.ExecutionFrequencyMinutes.Value) <= now)))
                .ToListAsync();
        }

        /// <summary>
        /// Vérifie si une stratégie existe
        /// </summary>
        private async Task<bool> StrategyExistsAsync(Guid id)
        {
            return await _context.Strategies.AnyAsync(s => s.Id == id);
        }
    }
}
