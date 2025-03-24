using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface pour le service de recommandations de trading
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Récupère les meilleures cryptomonnaies à acheter selon différents critères
        /// </summary>
        Task<IEnumerable<AssetRecommendation>> GetTopCryptosAsync(int count, RecommendationCriteria criteria);
        
        /// <summary>
        /// Analyse un actif spécifique et fournit des recommandations
        /// </summary>
        Task<AssetRecommendation> AnalyzeAssetAsync(string symbol);
        
        /// <summary>
        /// Détermine le meilleur moment pour acheter un actif spécifique
        /// </summary>
        Task<TimingRecommendation> GetBestBuyTimingAsync(string symbol);
        
        /// <summary>
        /// Détermine le meilleur moment pour vendre un actif spécifique
        /// </summary>
        Task<TimingRecommendation> GetBestSellTimingAsync(string symbol);
        
        /// <summary>
        /// Génère des recommandations personnalisées pour un utilisateur spécifique
        /// </summary>
        Task<IEnumerable<AssetRecommendation>> GetPersonalizedRecommendationsAsync(string userId, int count);
        
        /// <summary>
        /// Récupère les signaux techniques pour un actif spécifique
        /// </summary>
        Task<TechnicalSignals> GetTechnicalSignalsAsync(string symbol);
    }
    
    /// <summary>
    /// Critères de recommandation pour les actifs
    /// </summary>
    public enum RecommendationCriteria
    {
        MarketCap,          // Capitalisation boursière
        Performance24h,     // Performance sur 24h
        PerformanceWeek,    // Performance sur une semaine
        PerformanceMonth,   // Performance sur un mois
        PerformanceYTD,     // Performance depuis le début de l'année
        Volume,             // Volume d'échanges
        Volatility,         // Volatilité
        TechnicalAnalysis   // Analyse technique
    }
    
    /// <summary>
    /// Représente une recommandation pour un actif
    /// </summary>
    public class AssetRecommendation
    {
        public Asset Asset { get; set; }
        public RecommendationType Type { get; set; }
        public string Reasoning { get; set; }
        public decimal ConfidenceScore { get; set; }
        public decimal PotentialReturn { get; set; }
        public decimal RiskLevel { get; set; }
        public TimeSpan SuggestedHoldingPeriod { get; set; }
        public decimal SuggestedEntryPrice { get; set; }
        public decimal? SuggestedExitPrice { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
    
    /// <summary>
    /// Types de recommandations possibles
    /// </summary>
    public enum RecommendationType
    {
        StrongBuy,
        Buy,
        Hold,
        Sell,
        StrongSell
    }
    
    /// <summary>
    /// Représente une recommandation de timing pour l'achat ou la vente
    /// </summary>
    public class TimingRecommendation
    {
        public string Symbol { get; set; }
        public DateTime OptimalTime { get; set; }
        public string Reasoning { get; set; }
        public decimal ExpectedPrice { get; set; }
        public decimal ConfidenceScore { get; set; }
        public List<string> SupportingFactors { get; set; }
    }
    
    /// <summary>
    /// Représente les signaux techniques pour un actif
    /// </summary>
    public class TechnicalSignals
    {
        public string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Indicateurs de tendance
        public decimal SMA50 { get; set; }
        public decimal SMA200 { get; set; }
        public decimal EMA20 { get; set; }
        public bool GoldenCross { get; set; }
        public bool DeathCross { get; set; }
        
        // Oscillateurs
        public decimal RSI { get; set; }
        public decimal MACD { get; set; }
        public decimal MACDSignal { get; set; }
        public decimal MACDHistogram { get; set; }
        
        // Niveaux de support et résistance
        public decimal SupportLevel { get; set; }
        public decimal ResistanceLevel { get; set; }
        
        // Signaux globaux
        public RecommendationType OverallSignal { get; set; }
        public string SignalDescription { get; set; }
    }
}
