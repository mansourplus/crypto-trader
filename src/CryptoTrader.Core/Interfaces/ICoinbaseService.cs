using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface pour les services d'intégration avec l'API Coinbase
    /// </summary>
    public interface ICoinbaseService
    {
        /// <summary>
        /// Récupère le solde du portefeuille de l'utilisateur
        /// </summary>
        Task<Dictionary<string, decimal>> GetWalletBalancesAsync(string userId);
        
        /// <summary>
        /// Récupère l'historique des transactions de l'utilisateur
        /// </summary>
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Exécute un ordre d'achat
        /// </summary>
        Task<Transaction> ExecuteBuyOrderAsync(string userId, string symbol, decimal quantity, decimal? limitPrice = null);
        
        /// <summary>
        /// Exécute un ordre de vente
        /// </summary>
        Task<Transaction> ExecuteSellOrderAsync(string userId, string symbol, decimal quantity, decimal? limitPrice = null);
        
        /// <summary>
        /// Récupère les données de marché en temps réel pour un actif
        /// </summary>
        Task<Asset> GetMarketDataAsync(string symbol);
        
        /// <summary>
        /// Récupère les données de marché en temps réel pour plusieurs actifs
        /// </summary>
        Task<IEnumerable<Asset>> GetMarketDataBatchAsync(IEnumerable<string> symbols);
        
        /// <summary>
        /// Récupère l'historique des prix pour un actif
        /// </summary>
        Task<IEnumerable<PricePoint>> GetPriceHistoryAsync(string symbol, string timeframe, int limit);
        
        /// <summary>
        /// Récupère le carnet d'ordres pour un actif
        /// </summary>
        Task<OrderBook> GetOrderBookAsync(string symbol);
        
        /// <summary>
        /// Vérifie si l'API Coinbase est disponible
        /// </summary>
        Task<bool> IsApiAvailableAsync();
        
        /// <summary>
        /// Vérifie la validité des identifiants API de l'utilisateur
        /// </summary>
        Task<bool> ValidateApiCredentialsAsync(string apiKey, string apiSecret);
    }
    
    /// <summary>
    /// Représente un point de données de prix pour un actif à un moment donné
    /// </summary>
    public class PricePoint
    {
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }
    
    /// <summary>
    /// Représente le carnet d'ordres pour un actif
    /// </summary>
    public class OrderBook
    {
        public string Symbol { get; set; }
        public List<OrderBookEntry> Bids { get; set; } = new List<OrderBookEntry>();
        public List<OrderBookEntry> Asks { get; set; } = new List<OrderBookEntry>();
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Représente une entrée dans le carnet d'ordres
    /// </summary>
    public class OrderBookEntry
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }
}
