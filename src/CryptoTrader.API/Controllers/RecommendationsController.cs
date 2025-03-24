using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using CryptoTrader.Application.Services;
using CryptoTrader.Application.DTOs;

namespace CryptoTrader.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;
        private readonly ILogger<RecommendationsController> _logger;

        public RecommendationsController(
            RecommendationService recommendationService,
            ILogger<RecommendationsController> logger)
        {
            _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère les meilleures cryptomonnaies à acheter selon différents critères
        /// </summary>
        [HttpGet("top/{count}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<RecommendationDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetTopCryptos(int count, [FromQuery] string criteria = "MarketCap")
        {
            try
            {
                var recommendations = await _recommendationService.GetTopCryptosAsync(count, criteria);
                return Ok(recommendations);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Critère de recommandation invalide: {Criteria}", criteria);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des meilleures cryptomonnaies");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des recommandations");
            }
        }

        /// <summary>
        /// Analyse un actif spécifique et fournit des recommandations
        /// </summary>
        [HttpGet("analyze/{symbol}")]
        [Authorize]
        [ProducesResponseType(typeof(RecommendationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RecommendationDto>> AnalyzeAsset(string symbol)
        {
            try
            {
                var recommendation = await _recommendationService.AnalyzeAssetAsync(symbol);
                if (recommendation == null)
                {
                    return NotFound($"Actif avec le symbole {symbol} non trouvé");
                }
                return Ok(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'analyse de l'actif {Symbol}", symbol);
                return StatusCode(500, "Une erreur est survenue lors de l'analyse de l'actif");
            }
        }

        /// <summary>
        /// Détermine le meilleur moment pour acheter un actif spécifique
        /// </summary>
        [HttpGet("timing/buy/{symbol}")]
        [Authorize]
        [ProducesResponseType(typeof(TimingRecommendationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TimingRecommendationDto>> GetBestBuyTiming(string symbol)
        {
            try
            {
                var timing = await _recommendationService.GetBestBuyTimingAsync(symbol);
                if (timing == null)
                {
                    return NotFound($"Impossible de déterminer le meilleur moment d'achat pour {symbol}");
                }
                return Ok(timing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la détermination du meilleur moment d'achat pour {Symbol}", symbol);
                return StatusCode(500, "Une erreur est survenue lors de la détermination du meilleur moment d'achat");
            }
        }

        /// <summary>
        /// Détermine le meilleur moment pour vendre un actif spécifique
        /// </summary>
        [HttpGet("timing/sell/{symbol}")]
        [Authorize]
        [ProducesResponseType(typeof(TimingRecommendationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TimingRecommendationDto>> GetBestSellTiming(string symbol)
        {
            try
            {
                var timing = await _recommendationService.GetBestSellTimingAsync(symbol);
                if (timing == null)
                {
                    return NotFound($"Impossible de déterminer le meilleur moment de vente pour {symbol}");
                }
                return Ok(timing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la détermination du meilleur moment de vente pour {Symbol}", symbol);
                return StatusCode(500, "Une erreur est survenue lors de la détermination du meilleur moment de vente");
            }
        }

        /// <summary>
        /// Génère des recommandations personnalisées pour un utilisateur spécifique
        /// </summary>
        [HttpGet("personalized/{userId}/{count}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<RecommendationDto>), 200)]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetPersonalizedRecommendations(string userId, int count)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à accéder à ces recommandations
                var currentUserId = User.FindFirst("sub")?.Value;
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var recommendations = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération de recommandations personnalisées pour {UserId}", userId);
                return StatusCode(500, "Une erreur est survenue lors de la génération de recommandations personnalisées");
            }
        }

        /// <summary>
        /// Récupère les signaux techniques pour un actif spécifique
        /// </summary>
        [HttpGet("technical/{symbol}")]
        [Authorize]
        [ProducesResponseType(typeof(TechnicalSignalsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TechnicalSignalsDto>> GetTechnicalSignals(string symbol)
        {
            try
            {
                var signals = await _recommendationService.GetTechnicalSignalsAsync(symbol);
                if (signals == null)
                {
                    return NotFound($"Impossible de récupérer les signaux techniques pour {symbol}");
                }
                return Ok(signals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des signaux techniques pour {Symbol}", symbol);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des signaux techniques");
            }
        }

        /// <summary>
        /// Récupère les recommandations du jour
        /// </summary>
        [HttpGet("daily/{count}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<RecommendationDto>), 200)]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetDailyRecommendations(int count = 3)
        {
            try
            {
                var recommendations = await _recommendationService.GetDailyRecommendationsAsync(count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des recommandations du jour");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des recommandations du jour");
            }
        }

        /// <summary>
        /// Récupère les recommandations pour le lundi matin (période de faible activité)
        /// </summary>
        [HttpGet("monday-morning/{count}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<RecommendationDto>), 200)]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetMondayMorningRecommendations(int count = 5)
        {
            try
            {
                var recommendations = await _recommendationService.GetMondayMorningRecommendationsAsync(count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des recommandations du lundi matin");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des recommandations du lundi matin");
            }
        }

        /// <summary>
        /// Génère des recommandations pour une stratégie DCA (Dollar-Cost Averaging)
        /// </summary>
        [HttpGet("dca/{count}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<RecommendationDto>), 200)]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetDCARecommendations(int count = 3)
        {
            try
            {
                var recommendations = await _recommendationService.GetDCARecommendationsAsync(count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération de recommandations DCA");
                return StatusCode(500, "Une erreur est survenue lors de la génération de recommandations DCA");
            }
        }
    }
}
