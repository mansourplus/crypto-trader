using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CryptoTrader.Application.DTOs;
using CryptoTrader.Core.Entities;
using CryptoTrader.Core.Interfaces;

namespace CryptoTrader.Application.Services
{
    /// <summary>
    /// Service d'application pour la gestion des stratégies de trading
    /// </summary>
    public class StrategyService
    {
        private readonly IStrategyRepository _strategyRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICoinbaseService _coinbaseService;
        private readonly IMapper _mapper;

        public StrategyService(
            IStrategyRepository strategyRepository,
            IAssetRepository assetRepository,
            ITransactionRepository transactionRepository,
            ICoinbaseService coinbaseService,
            IMapper mapper)
        {
            _strategyRepository = strategyRepository ?? throw new ArgumentNullException(nameof(strategyRepository));
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _coinbaseService = coinbaseService ?? throw new ArgumentNullException(nameof(coinbaseService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Récupère toutes les stratégies
        /// </summary>
        public async Task<IEnumerable<StrategyDto>> GetAllStrategiesAsync()
        {
            var strategies = await _strategyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<StrategyDto>>(strategies);
        }

        /// <summary>
        /// Récupère une stratégie par son identifiant
        /// </summary>
        public async Task<StrategyDto> GetStrategyByIdAsync(Guid id)
        {
            var strategy = await _strategyRepository.GetByIdAsync(id);
            if (strategy == null)
                return null;

            var result = _mapper.Map<StrategyDto>(strategy);

            // Récupérer les informations sur les actifs associés
            if (strategy.AssetIds != null && strategy.AssetIds.Count > 0)
            {
                result.Assets = new List<AssetDto>();
                foreach (var assetId in strategy.AssetIds)
                {
                    var asset = await _assetRepository.GetByIdAsync(assetId);
                    if (asset != null)
                    {
                        result.Assets.Add(_mapper.Map<AssetDto>(asset));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Récupère les stratégies d'un utilisateur
        /// </summary>
        public async Task<IEnumerable<StrategyDto>> GetStrategiesByUserIdAsync(string userId)
        {
            var strategies = await _strategyRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<StrategyDto>>(strategies);
        }

        /// <summary>
        /// Récupère les stratégies par type
        /// </summary>
        public async Task<IEnumerable<StrategyDto>> GetStrategiesByTypeAsync(string type)
        {
            if (!Enum.TryParse<StrategyType>(type, true, out var strategyType))
            {
                throw new ArgumentException($"Type de stratégie invalide: {type}");
            }

            var strategies = await _strategyRepository.GetByTypeAsync(strategyType);
            return _mapper.Map<IEnumerable<StrategyDto>>(strategies);
        }

        /// <summary>
        /// Récupère les stratégies par statut
        /// </summary>
        public async Task<IEnumerable<StrategyDto>> GetStrategiesByStatusAsync(string status)
        {
            if (!Enum.TryParse<StrategyStatus>(status, true, out var strategyStatus))
            {
                throw new ArgumentException($"Statut de stratégie invalide: {status}");
            }

            var strategies = await _strategyRepository.GetByStatusAsync(strategyStatus);
            return _mapper.Map<IEnumerable<StrategyDto>>(strategies);
        }

        /// <summary>
        /// Crée une nouvelle stratégie
        /// </summary>
        public async Task<StrategyDto> CreateStrategyAsync(CreateStrategyDto createStrategyDto)
        {
            var strategy = _mapper.Map<Strategy>(createStrategyDto);

            // Convertir les symboles d'actifs en identifiants
            strategy.AssetIds = new List<Guid>();
            foreach (var symbol in createStrategyDto.AssetSymbols)
            {
                var asset = await _assetRepository.GetBySymbolAsync(symbol);
                if (asset == null)
                {
                    // Si l'actif n'existe pas, essayer de le récupérer via Coinbase
                    try
                    {
                        asset = await _coinbaseService.GetMarketDataAsync(symbol);
                        if (asset != null)
                        {
                            asset = await _assetRepository.AddAsync(asset);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignorer les actifs qui ne peuvent pas être trouvés
                        continue;
                    }
                }

                if (asset != null)
                {
                    strategy.AssetIds.Add(asset.Id);
                }
            }

            var result = await _strategyRepository.AddAsync(strategy);
            return await GetStrategyByIdAsync(result.Id);
        }

        /// <summary>
        /// Met à jour une stratégie existante
        /// </summary>
        public async Task<StrategyDto> UpdateStrategyAsync(UpdateStrategyDto updateStrategyDto)
        {
            var existingStrategy = await _strategyRepository.GetByIdAsync(updateStrategyDto.Id);
            if (existingStrategy == null)
            {
                return null;
            }

            _mapper.Map(updateStrategyDto, existingStrategy);

            // Mettre à jour les identifiants d'actifs si nécessaire
            if (updateStrategyDto.AssetSymbols != null && updateStrategyDto.AssetSymbols.Count > 0)
            {
                existingStrategy.AssetIds = new List<Guid>();
                foreach (var symbol in updateStrategyDto.AssetSymbols)
                {
                    var asset = await _assetRepository.GetBySymbolAsync(symbol);
                    if (asset != null)
                    {
                        existingStrategy.AssetIds.Add(asset.Id);
                    }
                }
            }

            var success = await _strategyRepository.UpdateAsync(existingStrategy);
            if (success)
            {
                return await GetStrategyByIdAsync(existingStrategy.Id);
            }

            return null;
        }

        /// <summary>
        /// Change le statut d'une stratégie
        /// </summary>
        public async Task<bool> ChangeStrategyStatusAsync(Guid id, string status)
        {
            if (!Enum.TryParse<StrategyStatus>(status, true, out var strategyStatus))
            {
                throw new ArgumentException($"Statut de stratégie invalide: {status}");
            }

            return await _strategyRepository.ChangeStatusAsync(id, strategyStatus);
        }

        /// <summary>
        /// Supprime une stratégie
        /// </summary>
        public async Task<bool> DeleteStrategyAsync(Guid id)
        {
            return await _strategyRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Exécute une stratégie spécifique
        /// </summary>
        public async Task<StrategyExecutionResultDto> ExecuteStrategyAsync(Guid id)
        {
            var strategy = await _strategyRepository.GetByIdAsync(id);
            if (strategy == null)
            {
                throw new ArgumentException($"Stratégie avec l'ID {id} non trouvée");
            }

            var result = new StrategyExecutionResultDto
            {
                StrategyId = id,
                ExecutionTime = DateTime.UtcNow,
                Transactions = new List<TransactionDto>()
            };

            try
            {
                switch (strategy.Type)
                {
                    case StrategyType.DCA:
                        await ExecuteDCAStrategyAsync(strategy, result);
                        break;
                    case StrategyType.TakeProfitStopLoss:
                        await ExecuteTakeProfitStopLossStrategyAsync(strategy, result);
                        break;
                    case StrategyType.MarketSignal:
                        await ExecuteMarketSignalStrategyAsync(strategy, result);
                        break;
                    case StrategyType.Custom:
                        // Logique personnalisée à implémenter selon les besoins
                        result.Success = false;
                        result.Message = "Les stratégies personnalisées ne sont pas encore implémentées";
                        break;
                    default:
                        result.Success = false;
                        result.Message = $"Type de stratégie non pris en charge: {strategy.Type}";
                        break;
                }

                // Mettre à jour la date de dernière exécution
                await _strategyRepository.UpdateLastExecutionTimeAsync(id, result.ExecutionTime);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Erreur lors de l'exécution de la stratégie: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Exécute toutes les stratégies actives qui doivent être exécutées
        /// </summary>
        public async Task<List<StrategyExecutionResultDto>> ExecuteActiveStrategiesAsync()
        {
            var strategiesToExecute = await _strategyRepository.GetStrategiesToExecuteAsync();
            var results = new List<StrategyExecutionResultDto>();

            foreach (var strategy in strategiesToExecute)
            {
                try
                {
                    var result = await ExecuteStrategyAsync(strategy.Id);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new StrategyExecutionResultDto
                    {
                        StrategyId = strategy.Id,
                        ExecutionTime = DateTime.UtcNow,
                        Success = false,
                        Message = $"Erreur lors de l'exécution de la stratégie: {ex.Message}",
                        Transactions = new List<TransactionDto>()
                    });
                }
            }

            return results;
        }

        #region Méthodes privées pour l'exécution des stratégies

        /// <summary>
        /// Exécute une stratégie DCA (Dollar-Cost Averaging)
        /// </summary>
        private async Task ExecuteDCAStrategyAsync(Strategy strategy, StrategyExecutionResultDto result)
        {
            // Vérifier que la stratégie a des actifs associés
            if (strategy.AssetIds == null || strategy.AssetIds.Count == 0)
            {
                result.Success = false;
                result.Message = "La stratégie DCA n'a pas d'actifs associés";
                return;
            }

            // Vérifier que le montant d'investissement est défini
            if (!strategy.MaxInvestmentAmount.HasValue || strategy.MaxInvestmentAmount.Value <= 0)
            {
                result.Success = false;
                result.Message = "Le montant d'investissement n'est pas défini pour la stratégie DCA";
                return;
            }

            // Calculer le montant à investir par actif
            decimal amountPerAsset = strategy.MaxInvestmentAmount.Value / strategy.AssetIds.Count;

            foreach (var assetId in strategy.AssetIds)
            {
                var asset = await _assetRepository.GetByIdAsync(assetId);
                if (asset == null)
                    continue;

                try
                {
                    // Calculer la quantité à acheter en fonction du prix actuel
                    decimal quantity = amountPerAsset / asset.CurrentPrice;

                    // Exécuter l'ordre d'achat via Coinbase
                    var transaction = await _coinbaseService.ExecuteBuyOrderAsync(
                        strategy.UserId,
                        asset.Symbol,
                        quantity);

                    // Compléter les informations de la transaction
                    transaction.AssetId = asset.Id;
                    transaction.Asset = asset;
                    transaction.StrategyId = strategy.Id;
                    transaction.Notes = $"Transaction générée par la stratégie DCA {strategy.Name}";

                    // Enregistrer la transaction dans notre base de données
                    var savedTransaction = await _transactionRepository.AddAsync(transaction);
                    result.Transactions.Add(_mapper.Map<TransactionDto>(savedTransaction));
                }
                catch (Exception ex)
                {
                    // Continuer avec les autres actifs même si une transaction échoue
                    continue;
                }
            }

            result.Success = result.Transactions.Count > 0;
            result.Message = result.Success
                ? $"Stratégie DCA exécutée avec succès. {result.Transactions.Count} transactions générées."
                : "Aucune transaction n'a pu être générée lors de l'exécution de la stratégie DCA.";
        }

        /// <summary>
        /// Exécute une stratégie Take-Profit/Stop-Loss
        /// </summary>
        private async Task ExecuteTakeProfitStopLossStrategyAsync(Strategy strategy, StrategyExecutionResultDto result)
        {
            // Vérifier que la stratégie a des actifs associés
            if (strategy.AssetIds == null || strategy.AssetIds.Count == 0)
            {
                result.Success = false;
                result.Message = "La stratégie Take-Profit/Stop-Loss n'a pas d'actifs associés";
                return;
            }

            // Vérifier que les pourcentages sont définis
            if (!strategy.TakeProfitPercentage.HasValue || !strategy.StopLossPercentage.HasValue)
            {
                result.Success = false;
                result.Message = "Les pourcentages de Take-Profit et/ou Stop-Loss ne sont pas définis";
                return;
            }

            foreach (var assetId in strategy.AssetIds)
            {
                var asset = await _assetRepository.GetByIdAsync(assetId);
                if (asset == null)
                    continue;

                // Récupérer le solde de l'utilisateur pour cet actif
                var balance = await _transactionRepository.GetUserBalanceForAssetAsync(strategy.UserId, assetId);
                if (balance <= 0)
                    continue;

                // Récupérer le prix moyen d'achat (simplifié)
                var transactions = await _transactionRepository.GetByUserIdAsync(strategy.UserId);
                var buyTransactions = transactions
                    .Where(t => t.AssetId == assetId && t.Type == TransactionType.Buy && t.Status == TransactionStatus.Completed)
                    .ToList();

                if (buyTransactions.Count == 0)
                    continue;

                decimal totalQuantity = buyTransactions.Sum(t => t.Quantity);
                decimal totalCost = buyTransactions.Sum(t => t.TotalAmount);
                decimal averageBuyPrice = totalCost / totalQuantity;

                // Calculer les seuils de Take-Profit et Stop-Loss
                decimal takeProfitPrice = averageBuyPrice * (1 + strategy.TakeProfitPercentage.Value / 100);
                decimal stopLossPrice = averageBuyPrice * (1 - strategy.StopLossPercentage.Value / 100);

                // Vérifier si les conditions sont remplies
                if (asset.CurrentPrice >= takeProfitPrice)
                {
                    // Condition de Take-Profit atteinte, vendre
                    try
                    {
                        var transaction = await _coinbaseService.ExecuteSellOrderAsync(
                            strategy.UserId,
                            asset.Symbol,
                            balance);

                        transaction.AssetId = asset.Id;
                        transaction.Asset = asset;
                        transaction.StrategyId = strategy.Id;
                        transaction.Notes = $"Transaction générée par la stratégie Take-Profit {strategy.Name}. Prix cible: {takeProfitPrice}";

                        var savedTransaction = await _transactionRepository.AddAsync(transaction);
                        result.Transactions.Add(_mapper.Map<TransactionDto>(savedTransaction));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                else if (asset.CurrentPrice <= stopLossPrice)
                {
                    // Condition de Stop-Loss atteinte, vendre
                    try
                    {
                        var transaction = await _coinbaseService.ExecuteSellOrderAsync(
                            strategy.UserId,
                            asset.Symbol,
                            balance);

                        transaction.AssetId = asset.Id;
                        transaction.Asset = asset;
                        transaction.StrategyId = strategy.Id;
                        transaction.Notes = $"Transaction générée par la stratégie Stop-Loss {strategy.Name}. Prix cible: {stopLossPrice}";

                        var savedTransaction = await _transactionRepository.AddAsync(transaction);
                        result.Transactions.Add(_mapper.Map<TransactionDto>(savedTransaction));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            result.Success = true;
            result.Message = result.Transactions.Count > 0
                ? $"Stratégie Take-Profit/Stop-Loss exécutée avec succès. {result.Transactions.Count} transactions générées."
                : "Aucune condition de Take-Profit ou Stop-Loss n'a été déclenchée.";
        }

        /// <summary>
        /// Exécute une stratégie basée sur les signaux de marché
        /// </summary>
        private async Task ExecuteMarketSignalStrategyAsync(Strategy strategy, StrategyExecutionResultDto result)
        {
            // Cette implémentation est simplifiée et devrait être enrichie avec une logique d'analyse technique plus avancée
            result.Success = true;
            result.Message = "La stratégie basée sur les signaux de marché n'est pas encore complètement implémentée.";
        }

        #endregion
    }
}
