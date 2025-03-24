using System;
using System.Collections.Generic;

namespace CryptoTrader.Core.Entities
{
    /// <summary>
    /// Représente une stratégie de trading automatisée
    /// </summary>
    public class Strategy
    {
        /// <summary>
        /// Identifiant unique de la stratégie
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nom de la stratégie
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description détaillée de la stratégie
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur propriétaire de la stratégie
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Type de stratégie
        /// </summary>
        public StrategyType Type { get; set; }

        /// <summary>
        /// Statut actuel de la stratégie
        /// </summary>
        public StrategyStatus Status { get; set; }

        /// <summary>
        /// Date de création de la stratégie
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date de dernière mise à jour de la stratégie
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Date de dernière exécution de la stratégie
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// Paramètres de la stratégie au format JSON
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Liste des actifs concernés par cette stratégie
        /// </summary>
        public List<Guid> AssetIds { get; set; }

        /// <summary>
        /// Montant maximum à investir par transaction
        /// </summary>
        public decimal? MaxInvestmentAmount { get; set; }

        /// <summary>
        /// Pourcentage de take-profit (si applicable)
        /// </summary>
        public decimal? TakeProfitPercentage { get; set; }

        /// <summary>
        /// Pourcentage de stop-loss (si applicable)
        /// </summary>
        public decimal? StopLossPercentage { get; set; }

        /// <summary>
        /// Fréquence d'exécution pour les stratégies périodiques (en minutes)
        /// </summary>
        public int? ExecutionFrequencyMinutes { get; set; }
    }

    /// <summary>
    /// Types de stratégies disponibles
    /// </summary>
    public enum StrategyType
    {
        DCA,                // Dollar-Cost Averaging
        TakeProfitStopLoss, // Take-profit/Stop-loss
        MarketSignal,       // Basée sur des signaux de marché
        Custom              // Stratégie personnalisée
    }

    /// <summary>
    /// Statuts possibles d'une stratégie
    /// </summary>
    public enum StrategyStatus
    {
        Active,     // Stratégie active et en cours d'exécution
        Paused,     // Stratégie temporairement suspendue
        Completed,  // Stratégie terminée (objectif atteint)
        Failed,     // Stratégie en échec
        Draft       // Stratégie en cours d'édition, non activée
    }
}
