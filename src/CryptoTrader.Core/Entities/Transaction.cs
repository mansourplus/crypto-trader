using System;
using System.Collections.Generic;

namespace CryptoTrader.Core.Entities
{
    /// <summary>
    /// Représente une transaction d'achat ou de vente d'un actif cryptographique
    /// </summary>
    public class Transaction
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
        /// Référence à l'actif concerné par la transaction
        /// </summary>
        public Asset Asset { get; set; }

        /// <summary>
        /// Type de transaction (Achat ou Vente)
        /// </summary>
        public TransactionType Type { get; set; }

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
        public TransactionStatus Status { get; set; }

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
    /// Types de transaction possibles
    /// </summary>
    public enum TransactionType
    {
        Buy,
        Sell
    }

    /// <summary>
    /// Statuts possibles d'une transaction
    /// </summary>
    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }
}
