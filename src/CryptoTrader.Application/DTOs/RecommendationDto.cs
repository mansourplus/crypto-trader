using System;
using System.Collections.Generic;

namespace CryptoTrader.Application.DTOs
{
    /// <summary>
    /// DTO pour représenter une recommandation d'actif
    /// </summary>
    public class RecommendationDto
    {
        /// <summary>
        /// Informations sur l'actif
        /// </summary>
        public AssetDto Asset { get; set; }
        
        /// <summary>
        /// Type de recommandation (StrongBuy, Buy, Hold, Sell, StrongSell)
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Raisonnement derrière la recommandation
        /// </summary>
        public string Reasoning { get; set; }
        
        /// <summary>
        /// Score de confiance (0-1)
        /// </summary>
        public decimal ConfidenceScore { get; set; }
        
        /// <summary>
        /// Retour potentiel en pourcentage
        /// </summary>
        public decimal PotentialReturn { get; set; }
        
        /// <summary>
        /// Niveau de risque (0-1)
        /// </summary>
        public decimal RiskLevel { get; set; }
        
        /// <summary>
        /// Période de détention suggérée en jours
        /// </summary>
        public double SuggestedHoldingPeriodDays { get; set; }
        
        /// <summary>
        /// Prix d'entrée suggéré
        /// </summary>
        public decimal SuggestedEntryPrice { get; set; }
        
        /// <summary>
        /// Prix de sortie suggéré
        /// </summary>
        public decimal? SuggestedExitPrice { get; set; }
        
        /// <summary>
        /// Date de génération de la recommandation
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }
    
    /// <summary>
    /// DTO pour représenter une recommandation de timing
    /// </summary>
    public class TimingRecommendationDto
    {
        /// <summary>
        /// Symbole de l'actif
        /// </summary>
        public string Symbol { get; set; }
        
        /// <summary>
        /// Moment optimal pour l'action
        /// </summary>
        public DateTime OptimalTime { get; set; }
        
        /// <summary>
        /// Raisonnement derrière la recommandation
        /// </summary>
        public string Reasoning { get; set; }
        
        /// <summary>
        /// Prix attendu
        /// </summary>
        public decimal ExpectedPrice { get; set; }
        
        /// <summary>
        /// Score de confiance (0-1)
        /// </summary>
        public decimal ConfidenceScore { get; set; }
        
        /// <summary>
        /// Facteurs de support pour la recommandation
        /// </summary>
        public List<string> SupportingFactors { get; set; }
    }
    
    /// <summary>
    /// DTO pour représenter les signaux techniques d'un actif
    /// </summary>
    public class TechnicalSignalsDto
    {
        /// <summary>
        /// Symbole de l'actif
        /// </summary>
        public string Symbol { get; set; }
        
        /// <summary>
        /// Date et heure de l'analyse
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Moyenne mobile simple sur 50 périodes
        /// </summary>
        public decimal SMA50 { get; set; }
        
        /// <summary>
        /// Moyenne mobile simple sur 200 périodes
        /// </summary>
        public decimal SMA200 { get; set; }
        
        /// <summary>
        /// Moyenne mobile exponentielle sur 20 périodes
        /// </summary>
        public decimal EMA20 { get; set; }
        
        /// <summary>
        /// Indique si un Golden Cross est détecté
        /// </summary>
        public bool GoldenCross { get; set; }
        
        /// <summary>
        /// Indique si un Death Cross est détecté
        /// </summary>
        public bool DeathCross { get; set; }
        
        /// <summary>
        /// Indice de force relative
        /// </summary>
        public decimal RSI { get; set; }
        
        /// <summary>
        /// MACD (Moving Average Convergence Divergence)
        /// </summary>
        public decimal MACD { get; set; }
        
        /// <summary>
        /// Ligne de signal MACD
        /// </summary>
        public decimal MACDSignal { get; set; }
        
        /// <summary>
        /// Histogramme MACD
        /// </summary>
        public decimal MACDHistogram { get; set; }
        
        /// <summary>
        /// Niveau de support
        /// </summary>
        public decimal SupportLevel { get; set; }
        
        /// <summary>
        /// Niveau de résistance
        /// </summary>
        public decimal ResistanceLevel { get; set; }
        
        /// <summary>
        /// Signal global (StrongBuy, Buy, Hold, Sell, StrongSell)
        /// </summary>
        public string OverallSignal { get; set; }
        
        /// <summary>
        /// Description du signal
        /// </summary>
        public string SignalDescription { get; set; }
    }
}
