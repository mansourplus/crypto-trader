using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CryptoTrader.Application.DTOs;
using CryptoTrader.Core.Interfaces;

namespace CryptoTrader.Application.Services
{
    /// <summary>
    /// Service d'application pour les recommandations de trading
    /// </summary>
    public class RecommendationService
    {
        private readonly IRecommendationService _recommendationService;
        private readonly IAssetRepository _assetRepository;
        private readonly IMapper _mapper;

        public RecommendationService(
            IRecommendationService recommendationService,
            IAssetRepository assetRepository,
            IMapper mapper)
        {
            _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Récupère les meilleures cryptomonnaies à acheter selon différents critères
        /// </summary>
        public async Task<IEnumerable<RecommendationDto>> GetTopCryptosAsync(int count, string criteria)
        {
            if (!Enum.TryParse<RecommendationCriteria>(criteria, true, out var recommendationCriteria))
            {
                throw new ArgumentException($"Critère de recommandation invalide: {criteria}");
            }

            var recommendations = await _recommendationService.GetTopCryptosAsync(count, recommendationCriteria);
            return _mapper.Map<IEnumerable<RecommendationDto>>(recommendations);
        }

        /// <summary>
        /// Analyse un actif spécifique et fournit des recommandations
        /// </summary>
        public async Task<RecommendationDto> AnalyzeAssetAsync(string symbol)
        {
            var recommendation = await _recommendationService.AnalyzeAssetAsync(symbol);
            return _mapper.Map<RecommendationDto>(recommendation);
        }

        /// <summary>
        /// Détermine le meilleur moment pour acheter un actif spécifique
        /// </summary>
        public async Task<TimingRecommendationDto> GetBestBuyTimingAsync(string symbol)
        {
            var timing = await _recommendationService.GetBestBuyTimingAsync(symbol);
            return _mapper.Map<TimingRecommendationDto>(timing);
        }

        /// <summary>
        /// Détermine le meilleur moment pour vendre un actif spécifique
        /// </summary>
        public async Task<TimingRecommendationDto> GetBestSellTimingAsync(string symbol)
        {
            var timing = await _recommendationService.GetBestSellTimingAsync(symbol);
            return _mapper.Map<TimingRecommendationDto>(timing);
        }

        /// <summary>
        /// Génère des recommandations personnalisées pour un utilisateur spécifique
        /// </summary>
        public async Task<IEnumerable<RecommendationDto>> GetPersonalizedRecommendationsAsync(string userId, int count)
        {
            var recommendations = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, count);
            return _mapper.Map<IEnumerable<RecommendationDto>>(recommendations);
        }

        /// <summary>
        /// Récupère les signaux techniques pour un actif spécifique
        /// </summary>
        public async Task<TechnicalSignalsDto> GetTechnicalSignalsAsync(string symbol)
        {
            var signals = await _recommendationService.GetTechnicalSignalsAsync(symbol);
            return _mapper.Map<TechnicalSignalsDto>(signals);
        }

        /// <summary>
        /// Récupère les recommandations du jour
        /// </summary>
        public async Task<IEnumerable<RecommendationDto>> GetDailyRecommendationsAsync(int count = 3)
        {
            // Par défaut, nous utilisons le critère de capitalisation boursière
            var recommendations = await _recommendationService.GetTopCryptosAsync(count, RecommendationCriteria.MarketCap);
            
            // Enrichir les recommandations avec des informations de timing
            var result = new List<RecommendationDto>();
            foreach (var recommendation in recommendations)
            {
                var dto = _mapper.Map<RecommendationDto>(recommendation);
                
                // Ajouter des informations de timing si possible
                try
                {
                    var timing = await _recommendationService.GetBestBuyTimingAsync(recommendation.Asset.Symbol);
                    dto.Reasoning += $" Meilleur moment pour acheter: {timing.OptimalTime.ToString("dddd HH:mm")}. {timing.Reasoning}";
                }
                catch (Exception)
                {
                    // Ignorer les erreurs de timing
                }
                
                result.Add(dto);
            }
            
            return result;
        }

        /// <summary>
        /// Récupère les recommandations pour le lundi matin (période de faible activité)
        /// </summary>
        public async Task<IEnumerable<RecommendationDto>> GetMondayMorningRecommendationsAsync(int count = 5)
        {
            // Récupérer les meilleures cryptos selon la performance
            var recommendations = await _recommendationService.GetTopCryptosAsync(count, RecommendationCriteria.Performance24h);
            
            var result = new List<RecommendationDto>();
            foreach (var recommendation in recommendations)
            {
                var dto = _mapper.Map<RecommendationDto>(recommendation);
                dto.Reasoning += " Cette recommandation est particulièrement pertinente pour le lundi matin, période traditionnellement de faible activité sur les marchés crypto.";
                result.Add(dto);
            }
            
            return result;
        }

        /// <summary>
        /// Génère des recommandations pour une stratégie DCA (Dollar-Cost Averaging)
        /// </summary>
        public async Task<IEnumerable<RecommendationDto>> GetDCARecommendationsAsync(int count = 3)
        {
            // Pour les stratégies DCA, nous privilégions les actifs avec une forte capitalisation et une volatilité modérée
            var recommendations = await _recommendationService.GetTopCryptosAsync(count, RecommendationCriteria.MarketCap);
            
            var result = new List<RecommendationDto>();
            foreach (var recommendation in recommendations)
            {
                var dto = _mapper.Map<RecommendationDto>(recommendation);
                dto.Reasoning += " Cet actif est recommandé pour une stratégie DCA (Dollar-Cost Averaging) en raison de sa capitalisation importante et de son potentiel de croissance à long terme.";
                result.Add(dto);
            }
            
            return result;
        }
    }
}
