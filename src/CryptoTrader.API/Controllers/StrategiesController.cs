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
    public class StrategiesController : ControllerBase
    {
        private readonly StrategyService _strategyService;
        private readonly IValidator<CreateStrategyDto> _createValidator;
        private readonly IValidator<UpdateStrategyDto> _updateValidator;
        private readonly ILogger<StrategiesController> _logger;

        public StrategiesController(
            StrategyService strategyService,
            IValidator<CreateStrategyDto> createValidator,
            IValidator<UpdateStrategyDto> updateValidator,
            ILogger<StrategiesController> logger)
        {
            _strategyService = strategyService ?? throw new ArgumentNullException(nameof(strategyService));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère toutes les stratégies
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<StrategyDto>), 200)]
        public async Task<ActionResult<IEnumerable<StrategyDto>>> GetAllStrategies()
        {
            try
            {
                var strategies = await _strategyService.GetAllStrategiesAsync();
                return Ok(strategies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des stratégies");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des stratégies");
            }
        }

        /// <summary>
        /// Récupère une stratégie par son identifiant
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(StrategyDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<StrategyDto>> GetStrategyById(Guid id)
        {
            try
            {
                var strategy = await _strategyService.GetStrategyByIdAsync(id);
                if (strategy == null)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                // Vérifier que l'utilisateur a accès à cette stratégie
                var userId = User.FindFirst("sub")?.Value;
                if (strategy.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(strategy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la stratégie {StrategyId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la récupération de la stratégie");
            }
        }

        /// <summary>
        /// Récupère les stratégies d'un utilisateur
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<StrategyDto>), 200)]
        public async Task<ActionResult<IEnumerable<StrategyDto>>> GetStrategiesByUserId(string userId)
        {
            try
            {
                // Vérifier que l'utilisateur a accès à ces stratégies
                var currentUserId = User.FindFirst("sub")?.Value;
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var strategies = await _strategyService.GetStrategiesByUserIdAsync(userId);
                return Ok(strategies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des stratégies de l'utilisateur {UserId}", userId);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des stratégies");
            }
        }

        /// <summary>
        /// Récupère les stratégies par type
        /// </summary>
        [HttpGet("type/{type}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<StrategyDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<StrategyDto>>> GetStrategiesByType(string type)
        {
            try
            {
                var strategies = await _strategyService.GetStrategiesByTypeAsync(type);
                
                // Filtrer les stratégies pour n'inclure que celles de l'utilisateur actuel (sauf pour les admins)
                if (!User.IsInRole("Admin"))
                {
                    var userId = User.FindFirst("sub")?.Value;
                    strategies = strategies.Where(s => s.UserId == userId).ToList();
                }
                
                return Ok(strategies);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Type de stratégie invalide: {Type}", type);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des stratégies de type {Type}", type);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des stratégies");
            }
        }

        /// <summary>
        /// Récupère les stratégies par statut
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<StrategyDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<StrategyDto>>> GetStrategiesByStatus(string status)
        {
            try
            {
                var strategies = await _strategyService.GetStrategiesByStatusAsync(status);
                
                // Filtrer les stratégies pour n'inclure que celles de l'utilisateur actuel (sauf pour les admins)
                if (!User.IsInRole("Admin"))
                {
                    var userId = User.FindFirst("sub")?.Value;
                    strategies = strategies.Where(s => s.UserId == userId).ToList();
                }
                
                return Ok(strategies);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Statut de stratégie invalide: {Status}", status);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des stratégies avec le statut {Status}", status);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des stratégies");
            }
        }

        /// <summary>
        /// Crée une nouvelle stratégie
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(StrategyDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<StrategyDto>> CreateStrategy([FromBody] CreateStrategyDto createStrategyDto)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à créer cette stratégie
                var userId = User.FindFirst("sub")?.Value;
                if (createStrategyDto.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var validationResult = await _createValidator.ValidateAsync(createStrategyDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var strategy = await _strategyService.CreateStrategyAsync(createStrategyDto);
                return CreatedAtAction(nameof(GetStrategyById), new { id = strategy.Id }, strategy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument invalide lors de la création d'une stratégie");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création d'une stratégie");
                return StatusCode(500, "Une erreur est survenue lors de la création de la stratégie");
            }
        }

        /// <summary>
        /// Met à jour une stratégie existante
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(StrategyDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<StrategyDto>> UpdateStrategy(Guid id, [FromBody] UpdateStrategyDto updateStrategyDto)
        {
            try
            {
                if (id != updateStrategyDto.Id)
                {
                    return BadRequest("L'ID de la stratégie ne correspond pas à l'ID dans l'URL");
                }

                // Vérifier que l'utilisateur est autorisé à mettre à jour cette stratégie
                var strategy = await _strategyService.GetStrategyByIdAsync(id);
                if (strategy == null)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                var userId = User.FindFirst("sub")?.Value;
                if (strategy.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var validationResult = await _updateValidator.ValidateAsync(updateStrategyDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var updatedStrategy = await _strategyService.UpdateStrategyAsync(updateStrategyDto);
                if (updatedStrategy == null)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                return Ok(updatedStrategy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument invalide lors de la mise à jour de la stratégie {StrategyId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la stratégie {StrategyId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour de la stratégie");
            }
        }

        /// <summary>
        /// Change le statut d'une stratégie
        /// </summary>
        [HttpPatch("{id}/status/{status}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeStrategyStatus(Guid id, string status)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à modifier cette stratégie
                var strategy = await _strategyService.GetStrategyByIdAsync(id);
                if (strategy == null)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                var userId = User.FindFirst("sub")?.Value;
                if (strategy.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var success = await _strategyService.ChangeStrategyStatusAsync(id, status);
                if (!success)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Statut invalide lors du changement de statut de la stratégie {StrategyId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut de la stratégie {StrategyId}", id);
                return StatusCode(500, "Une erreur est survenue lors du changement de statut de la stratégie");
            }
        }

        /// <summary>
        /// Supprime une stratégie
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteStrategy(Guid id)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à supprimer cette stratégie
                var strategy = await _strategyService.GetStrategyByIdAsync(id);
                if (strategy == null)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                var userId = User.FindFirst("sub")?.Value;
                if (strategy.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var success = await _strategyService.DeleteStrategyAsync(id);
                if (!success)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la stratégie {StrategyId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la suppression de la stratégie");
            }
        }

        /// <summary>
        /// Exécute une stratégie spécifique
        /// </summary>
        [HttpPost("{id}/execute")]
        [Authorize]
        [ProducesResponseType(typeof(StrategyExecutionResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<StrategyExecutionResultDto>> ExecuteStrategy(Guid id)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à exécuter cette stratégie
                var strategy = await _strategyService.GetStrategyByIdAsync(id);
                if (strategy == null)
                {
                    return NotFound($"Stratégie avec l'ID {id} non trouvée");
                }

                var userId = User.FindFirst("sub")?.Value;
                if (strategy.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var result = await _strategyService.ExecuteStrategyAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument invalide lors de l'exécution de la stratégie {StrategyId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution de la stratégie {StrategyId}", id);
                return StatusCode(500, "Une erreur est survenue lors de l'exécution de la stratégie");
            }
        }

        /// <summary>
        /// Exécute toutes les stratégies actives qui doivent être exécutées
        /// </summary>
        [HttpPost("execute-active")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<StrategyExecutionResultDto>), 200)]
        public async Task<ActionResult<List<StrategyExecutionResultDto>>> ExecuteActiveStrategies()
        {
            try
            {
                var results = await _strategyService.ExecuteActiveStrategiesAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution des stratégies actives");
                return StatusCode(500, "Une erreur est survenue lors de l'exécution des stratégies actives");
            }
        }
    }
}
