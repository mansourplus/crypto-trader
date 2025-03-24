using System;

namespace CryptoTrader.Application.DTOs
{
    /// <summary>
    /// DTO pour représenter un actif cryptographique
    /// </summary>
    public class AssetDto
    {
        /// <summary>
        /// Identifiant unique de l'actif
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Symbole de l'actif (ex: BTC, ETH)
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Nom complet de l'actif (ex: Bitcoin, Ethereum)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Prix actuel de l'actif en USD
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Variation du prix sur 24h en pourcentage
        /// </summary>
        public decimal PriceChangePercentage24h { get; set; }

        /// <summary>
        /// Capitalisation boursière de l'actif
        /// </summary>
        public decimal MarketCap { get; set; }

        /// <summary>
        /// Volume d'échange sur 24h
        /// </summary>
        public decimal Volume24h { get; set; }

        /// <summary>
        /// Offre en circulation
        /// </summary>
        public decimal CirculatingSupply { get; set; }

        /// <summary>
        /// Offre maximale (si applicable)
        /// </summary>
        public decimal? MaxSupply { get; set; }

        /// <summary>
        /// URL de l'image/logo de l'actif
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Date de dernière mise à jour des données
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO pour la création d'un nouvel actif
    /// </summary>
    public class CreateAssetDto
    {
        /// <summary>
        /// Symbole de l'actif (ex: BTC, ETH)
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Nom complet de l'actif (ex: Bitcoin, Ethereum)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Prix actuel de l'actif en USD
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// URL de l'image/logo de l'actif
        /// </summary>
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO pour la mise à jour d'un actif existant
    /// </summary>
    public class UpdateAssetDto
    {
        /// <summary>
        /// Identifiant unique de l'actif
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Prix actuel de l'actif en USD
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Variation du prix sur 24h en pourcentage
        /// </summary>
        public decimal PriceChangePercentage24h { get; set; }

        /// <summary>
        /// Capitalisation boursière de l'actif
        /// </summary>
        public decimal MarketCap { get; set; }

        /// <summary>
        /// Volume d'échange sur 24h
        /// </summary>
        public decimal Volume24h { get; set; }

        /// <summary>
        /// Offre en circulation
        /// </summary>
        public decimal CirculatingSupply { get; set; }

        /// <summary>
        /// Offre maximale (si applicable)
        /// </summary>
        public decimal? MaxSupply { get; set; }
    }
}
