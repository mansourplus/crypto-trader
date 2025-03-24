using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using CryptoTrader.Application.Services;
using CryptoTrader.Application.DTOs;
using FluentValidation;

namespace CryptoTrader.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AssetService _assetService;
        private readonly IValidator<CreateAssetDto> _createValidator;
        private readonly IValidator<UpdateAssetDto> _updateValidator;
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(
            AssetService assetService,
            IValidator<CreateAssetDto> createValidator,
            IValidator<UpdateAssetDto> updateValidator,
            ILogger<AssetsController> logger)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère tous les actifs
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AssetDto>), 200)]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetAllAssets()
        {
            try
            {
                var assets = await _assetService.GetAllAssetsAsync();
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des actifs");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des actifs");
            }
        }

        /// <summary>
        /// Récupère un actif par son identifiant
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AssetDto>> GetAssetById(Guid id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return NotFound($"Actif avec l'ID {id} non trouvé");
                }
                return Ok(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'actif {AssetId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la récupération de l'actif");
            }
        }

        /// <summary>
        /// Récupère un actif par son symbole
        /// </summary>
        [HttpGet("symbol/{symbol}")]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AssetDto>> GetAssetBySymbol(string symbol)
        {
            try
            {
                var asset = await _assetService.GetAssetBySymbolAsync(symbol);
                if (asset == null)
                {
                    return NotFound($"Actif avec le symbole {symbol} non trouvé");
                }
                return Ok(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'actif avec le symbole {Symbol}", symbol);
                return StatusCode(500, "Une erreur est survenue lors de la récupération de l'actif");
            }
        }

        /// <summary>
        /// Récupère les actifs les plus performants
        /// </summary>
        [HttpGet("top/{count}")]
        [ProducesResponseType(typeof(IEnumerable<AssetDto>), 200)]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetTopPerformingAssets(int count, [FromQuery] string criteria = "marketCap")
        {
            try
            {
                var assets = await _assetService.GetTopPerformingAssetsAsync(count, criteria);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des actifs les plus performants");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des actifs les plus performants");
            }
        }

        /// <summary>
        /// Ajoute un nouvel actif
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AssetDto>> AddAsset([FromBody] CreateAssetDto createAssetDto)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(createAssetDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var asset = await _assetService.AddAssetAsync(createAssetDto);
                return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout d'un actif");
                return StatusCode(500, "Une erreur est survenue lors de l'ajout de l'actif");
            }
        }

        /// <summary>
        /// Met à jour un actif existant
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetDto updateAssetDto)
        {
            try
            {
                if (id != updateAssetDto.Id)
                {
                    return BadRequest("L'ID de l'actif ne correspond pas à l'ID dans l'URL");
                }

                var validationResult = await _updateValidator.ValidateAsync(updateAssetDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var success = await _assetService.UpdateAssetAsync(updateAssetDto);
                if (!success)
                {
                    return NotFound($"Actif avec l'ID {id} non trouvé");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'actif {AssetId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour de l'actif");
            }
        }

        /// <summary>
        /// Supprime un actif
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            try
            {
                var success = await _assetService.DeleteAssetAsync(id);
                if (!success)
                {
                    return NotFound($"Actif avec l'ID {id} non trouvé");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'actif {AssetId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la suppression de l'actif");
            }
        }

        /// <summary>
        /// Met à jour les données de marché pour tous les actifs
        /// </summary>
        [HttpPost("update-market-data")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> UpdateMarketData()
        {
            try
            {
                await _assetService.UpdateMarketDataAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des données de marché");
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour des données de marché");
            }
        }
    }
}
