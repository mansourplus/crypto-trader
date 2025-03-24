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
    /// Implémentation du repository pour les transactions
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly CryptoTraderDbContext _context;

        public TransactionRepository(CryptoTraderDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Récupère toutes les transactions
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère une transaction par son identifiant
        /// </summary>
        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Récupère les transactions d'un utilisateur
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId)
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les transactions pour un actif spécifique
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetByAssetIdAsync(Guid assetId)
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les transactions générées par une stratégie spécifique
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetByStrategyIdAsync(Guid strategyId)
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .Where(t => t.StrategyId == strategyId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les transactions dans une plage de dates
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Ajoute une nouvelle transaction
        /// </summary>
        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        public async Task<bool> UpdateAsync(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TransactionExistsAsync(transaction.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Supprime une transaction
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Récupère le solde total d'un utilisateur pour un actif spécifique
        /// </summary>
        public async Task<decimal> GetUserBalanceForAssetAsync(string userId, Guid assetId)
        {
            // Calculer le solde en additionnant les achats et en soustrayant les ventes
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.AssetId == assetId && t.Status == TransactionStatus.Completed)
                .ToListAsync();

            decimal balance = 0;
            foreach (var transaction in transactions)
            {
                if (transaction.Type == TransactionType.Buy)
                {
                    balance += transaction.Quantity;
                }
                else if (transaction.Type == TransactionType.Sell)
                {
                    balance -= transaction.Quantity;
                }
            }

            return balance;
        }

        /// <summary>
        /// Récupère le solde total d'un utilisateur pour tous les actifs
        /// </summary>
        public async Task<Dictionary<Guid, decimal>> GetUserBalancesAsync(string userId)
        {
            // Récupérer toutes les transactions complétées de l'utilisateur
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Status == TransactionStatus.Completed)
                .ToListAsync();

            // Calculer le solde pour chaque actif
            var balances = new Dictionary<Guid, decimal>();
            foreach (var transaction in transactions)
            {
                if (!balances.ContainsKey(transaction.AssetId))
                {
                    balances[transaction.AssetId] = 0;
                }

                if (transaction.Type == TransactionType.Buy)
                {
                    balances[transaction.AssetId] += transaction.Quantity;
                }
                else if (transaction.Type == TransactionType.Sell)
                {
                    balances[transaction.AssetId] -= transaction.Quantity;
                }
            }

            return balances;
        }

        /// <summary>
        /// Vérifie si une transaction existe
        /// </summary>
        private async Task<bool> TransactionExistsAsync(Guid id)
        {
            return await _context.Transactions.AnyAsync(t => t.Id == id);
        }
    }
}
