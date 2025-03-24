using System;
using System.Collections.Generic;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Application.DTOs
{
    /// <summary>
    /// DTO pour représenter une stratégie de trading
    /// </summary>
    public class StrategyDto
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
        public string Type { get; set; }

        /// <summary>
        /// Statut actuel de la stratégie
        /// </summary>
        public string Status { get; set; }

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
        /// Liste des actifs concernés par cette stratégie
        /// </summary>
        public List<AssetDto> Assets { get; set; }

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
    /// DTO pour la création d'une nouvelle stratégie
    /// </summary>
    public class CreateStrategyDto
    {
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
        /// Type de stratégie (DCA, TakeProfitStopLoss, MarketSignal, Custom)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Paramètres de la stratégie au format JSON
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Liste des symboles des actifs concernés par cette stratégie
        /// </summary>
        public List<string> AssetSymbols { get; set; }

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
    /// DTO pour la mise à jour d'une stratégie existante
    /// </summary>
    public class UpdateStrategyDto
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
        /// Statut de la stratégie (Active, Paused, Completed, Failed, Draft)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Paramètres de la stratégie au format JSON
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Liste des symboles des actifs concernés par cette stratégie
        /// </summary>
        public List<string> AssetSymbols { get; set; }

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
    /// DTO pour le résultat d'exécution d'une stratégie
    /// </summary>
    public class StrategyExecutionResultDto
    {
        /// <summary>
        /// Identifiant de la stratégie
        /// </summary>
        public Guid StrategyId { get; set; }

        /// <summary>
        /// Date et heure d'exécution
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// Succès de l'exécution
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message de résultat
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Transactions générées par l'exécution
        /// </summary>
        public List<TransactionDto> Transactions { get; set; }
    }
}
