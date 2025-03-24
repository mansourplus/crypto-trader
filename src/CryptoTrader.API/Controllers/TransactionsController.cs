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
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private readonly IValidator<CreateTransactionDto> _createValidator;
        private readonly IValidator<UpdateTransactionDto> _updateValidator;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            TransactionService transactionService,
            IValidator<CreateTransactionDto> createValidator,
            IValidator<UpdateTransactionDto> updateValidator,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Récupère toutes les transactions
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des transactions");
            }
        }

        /// <summary>
        /// Récupère une transaction par son identifiant
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(TransactionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TransactionDto>> GetTransactionById(Guid id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound($"Transaction avec l'ID {id} non trouvée");
                }

                // Vérifier que l'utilisateur a accès à cette transaction
                var userId = User.FindFirst("sub")?.Value;
                if (transaction.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la transaction {TransactionId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la récupération de la transaction");
            }
        }

        /// <summary>
        /// Récupère les transactions d'un utilisateur
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByUserId(string userId)
        {
            try
            {
                // Vérifier que l'utilisateur a accès à ces transactions
                var currentUserId = User.FindFirst("sub")?.Value;
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions de l'utilisateur {UserId}", userId);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des transactions");
            }
        }

        /// <summary>
        /// Récupère les transactions pour un actif spécifique
        /// </summary>
        [HttpGet("asset/{assetId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByAssetId(Guid assetId)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsByAssetIdAsync(assetId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions pour l'actif {AssetId}", assetId);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des transactions");
            }
        }

        /// <summary>
        /// Récupère les transactions générées par une stratégie spécifique
        /// </summary>
        [HttpGet("strategy/{strategyId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByStrategyId(Guid strategyId)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsByStrategyIdAsync(strategyId);
                
                // Vérifier que l'utilisateur a accès à ces transactions
                if (transactions.Any() && !User.IsInRole("Admin"))
                {
                    var userId = User.FindFirst("sub")?.Value;
                    if (transactions.First().UserId != userId)
                    {
                        return Forbid();
                    }
                }
                
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions pour la stratégie {StrategyId}", strategyId);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des transactions");
            }
        }

        /// <summary>
        /// Exécute un ordre d'achat
        /// </summary>
        [HttpPost("buy")]
        [Authorize]
        [ProducesResponseType(typeof(TransactionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TransactionDto>> ExecuteBuyOrder([FromBody] CreateTransactionDto createTransactionDto)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à créer cette transaction
                var userId = User.FindFirst("sub")?.Value;
                if (createTransactionDto.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Forcer le type de transaction à "Buy"
                createTransactionDto.Type = "Buy";

                var validationResult = await _createValidator.ValidateAsync(createTransactionDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var transaction = await _transactionService.ExecuteBuyOrderAsync(createTransactionDto);
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument invalide lors de l'exécution d'un ordre d'achat");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution d'un ordre d'achat");
                return StatusCode(500, "Une erreur est survenue lors de l'exécution de l'ordre d'achat");
            }
        }

        /// <summary>
        /// Exécute un ordre de vente
        /// </summary>
        [HttpPost("sell")]
        [Authorize]
        [ProducesResponseType(typeof(TransactionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TransactionDto>> ExecuteSellOrder([FromBody] CreateTransactionDto createTransactionDto)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à créer cette transaction
                var userId = User.FindFirst("sub")?.Value;
                if (createTransactionDto.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Forcer le type de transaction à "Sell"
                createTransactionDto.Type = "Sell";

                var validationResult = await _createValidator.ValidateAsync(createTransactionDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var transaction = await _transactionService.ExecuteSellOrderAsync(createTransactionDto);
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument invalide lors de l'exécution d'un ordre de vente");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Opération invalide lors de l'exécution d'un ordre de vente");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution d'un ordre de vente");
                return StatusCode(500, "Une erreur est survenue lors de l'exécution de l'ordre de vente");
            }
        }

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionDto updateTransactionDto)
        {
            try
            {
                if (id != updateTransactionDto.Id)
                {
                    return BadRequest("L'ID de la transaction ne correspond pas à l'ID dans l'URL");
                }

                // Vérifier que l'utilisateur est autorisé à mettre à jour cette transaction
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound($"Transaction avec l'ID {id} non trouvée");
                }

                var userId = User.FindFirst("sub")?.Value;
                if (transaction.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var validationResult = await _updateValidator.ValidateAsync(updateTransactionDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var success = await _transactionService.UpdateTransactionAsync(updateTransactionDto);
                if (!success)
                {
                    return NotFound($"Transaction avec l'ID {id} non trouvée");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la transaction {TransactionId}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour de la transaction");
            }
        }

        /// <summary>
        /// Récupère le solde du portefeuille d'un utilisateur
        /// </summary>
        [HttpGet("portfolio/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(PortfolioSummaryDto), 200)]
        public async Task<ActionResult<PortfolioSummaryDto>> GetUserPortfolio(string userId)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à accéder à ce portefeuille
                var currentUserId = User.FindFirst("sub")?.Value;
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var portfolio = await _transactionService.GetUserPortfolioAsync(userId);
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du portefeuille de l'utilisateur {UserId}", userId);
                return StatusCode(500, "Une erreur est survenue lors de la récupération du portefeuille");
            }
        }

        /// <summary>
        /// Synchronise les transactions depuis Coinbase
        /// </summary>
        [HttpPost("sync/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> SyncTransactionsFromCoinbase(string userId, [FromQuery] DateTime? startDate = null)
        {
            try
            {
                // Vérifier que l'utilisateur est autorisé à synchroniser ces transactions
                var currentUserId = User.FindFirst("sub")?.Value;
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var syncCount = await _transactionService.SyncTransactionsFromCoinbaseAsync(userId, startDate);
                return Ok(syncCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation des transactions pour l'utilisateur {UserId}", userId);
                return StatusCode(500, "Une erreur est survenue lors de la synchronisation des transactions");
            }
        }
    }
}
