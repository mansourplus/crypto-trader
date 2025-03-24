using System;
using System.Collections.Generic;
using CryptoTrader.Core.Entities;

namespace CryptoTrader.Application.DTOs
{
    /// <summary>
    /// DTO pour représenter une transaction
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// Identifiant unique de la transaction
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur qui a effectué la transaction
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Identifiant de l'actif concerné par la transaction
        /// </summary>
        public Guid AssetId { get; set; }

        /// <summary>
        /// Informations sur l'actif concerné par la transaction
        /// </summary>
        public AssetDto Asset { get; set; }

        /// <summary>
        /// Type de transaction (Achat ou Vente)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Quantité d'actif échangée
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Prix unitaire au moment de la transaction
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Montant total de la transaction (Quantity * Price)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Frais associés à la transaction
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Date et heure de la transaction
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Identifiant de la transaction sur la plateforme Coinbase
        /// </summary>
        public string CoinbaseTransactionId { get; set; }

        /// <summary>
        /// Statut de la transaction
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Identifiant de la stratégie qui a initié cette transaction (si applicable)
        /// </summary>
        public Guid? StrategyId { get; set; }

        /// <summary>
        /// Notes ou commentaires sur la transaction
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO pour la création d'une nouvelle transaction
    /// </summary>
    public class CreateTransactionDto
    {
        /// <summary>
        /// Identifiant de l'utilisateur qui effectue la transaction
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Symbole de l'actif concerné par la transaction
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Type de transaction (Buy ou Sell)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Quantité d'actif à échanger
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Prix limite (optionnel pour les ordres limites)
        /// </summary>
        public decimal? LimitPrice { get; set; }

        /// <summary>
        /// Identifiant de la stratégie qui initie cette transaction (si applicable)
        /// </summary>
        public Guid? StrategyId { get; set; }

        /// <summary>
        /// Notes ou commentaires sur la transaction
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO pour la mise à jour d'une transaction existante
    /// </summary>
    public class UpdateTransactionDto
    {
        /// <summary>
        /// Identifiant unique de la transaction
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Statut de la transaction
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Notes ou commentaires sur la transaction
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO pour le résumé du portefeuille d'un utilisateur
    /// </summary>
    public class PortfolioSummaryDto
    {
        /// <summary>
        /// Identifiant de l'utilisateur
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Valeur totale du portefeuille en USD
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Variation de la valeur sur 24h en pourcentage
        /// </summary>
        public decimal Change24h { get; set; }

        /// <summary>
        /// Liste des actifs détenus
        /// </summary>
        public List<PortfolioAssetDto> Assets { get; set; }
    }

    /// <summary>
    /// DTO pour un actif dans le portefeuille
    /// </summary>
    public class PortfolioAssetDto
    {
        /// <summary>
        /// Informations sur l'actif
        /// </summary>
        public AssetDto Asset { get; set; }

        /// <summary>
        /// Quantité détenue
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Valeur en USD
        /// </summary>
        public decimal ValueUsd { get; set; }

        /// <summary>
        /// Pourcentage du portefeuille
        /// </summary>
        public decimal PortfolioPercentage { get; set; }

        /// <summary>
        /// Variation de la valeur sur 24h en pourcentage
        /// </summary>
        public decimal Change24h { get; set; }

        /// <summary>
        /// Prix moyen d'achat
        /// </summary>
        public decimal AverageBuyPrice { get; set; }

        /// <summary>
        /// Profit/perte non réalisé
        /// </summary>
        public decimal UnrealizedPnl { get; set; }
    }
}
