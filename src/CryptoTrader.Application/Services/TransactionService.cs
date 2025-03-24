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
    /// Service d'application pour la gestion des transactions
    /// </summary>
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly ICoinbaseService _coinbaseService;
        private readonly IMapper _mapper;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IAssetRepository assetRepository,
            ICoinbaseService coinbaseService,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _coinbaseService = coinbaseService ?? throw new ArgumentNullException(nameof(coinbaseService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Récupère toutes les transactions
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        /// <summary>
        /// Récupère une transaction par son identifiant
        /// </summary>
        public async Task<TransactionDto> GetTransactionByIdAsync(Guid id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            return _mapper.Map<TransactionDto>(transaction);
        }

        /// <summary>
        /// Récupère les transactions d'un utilisateur
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(string userId)
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        /// <summary>
        /// Récupère les transactions pour un actif spécifique
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByAssetIdAsync(Guid assetId)
        {
            var transactions = await _transactionRepository.GetByAssetIdAsync(assetId);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        /// <summary>
        /// Récupère les transactions générées par une stratégie spécifique
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByStrategyIdAsync(Guid strategyId)
        {
            var transactions = await _transactionRepository.GetByStrategyIdAsync(strategyId);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        /// <summary>
        /// Récupère les transactions dans une plage de dates
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        /// <summary>
        /// Exécute un ordre d'achat
        /// </summary>
        public async Task<TransactionDto> ExecuteBuyOrderAsync(CreateTransactionDto createTransactionDto)
        {
            // Vérifier que le type de transaction est bien un achat
            if (createTransactionDto.Type.ToUpper() != "BUY")
            {
                throw new ArgumentException("Le type de transaction doit être 'Buy' pour un ordre d'achat");
            }

            // Récupérer l'actif par son symbole
            var asset = await _assetRepository.GetBySymbolAsync(createTransactionDto.Symbol);
            if (asset == null)
            {
                // Si l'actif n'existe pas dans notre base de données, essayer de le récupérer via Coinbase
                try
                {
                    asset = await _coinbaseService.GetMarketDataAsync(createTransactionDto.Symbol);
                    if (asset != null)
                    {
                        asset = await _assetRepository.AddAsync(asset);
                    }
                    else
                    {
                        throw new ArgumentException($"L'actif avec le symbole {createTransactionDto.Symbol} n'a pas été trouvé");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur lors de la récupération de l'actif: {ex.Message}", ex);
                }
            }

            // Exécuter l'ordre d'achat via Coinbase
            var transaction = await _coinbaseService.ExecuteBuyOrderAsync(
                createTransactionDto.UserId,
                createTransactionDto.Symbol,
                createTransactionDto.Quantity,
                createTransactionDto.LimitPrice);

            // Compléter les informations de la transaction
            transaction.AssetId = asset.Id;
            transaction.Asset = asset;
            transaction.StrategyId = createTransactionDto.StrategyId;
            transaction.Notes = createTransactionDto.Notes;

            // Enregistrer la transaction dans notre base de données
            var savedTransaction = await _transactionRepository.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(savedTransaction);
        }

        /// <summary>
        /// Exécute un ordre de vente
        /// </summary>
        public async Task<TransactionDto> ExecuteSellOrderAsync(CreateTransactionDto createTransactionDto)
        {
            // Vérifier que le type de transaction est bien une vente
            if (createTransactionDto.Type.ToUpper() != "SELL")
            {
                throw new ArgumentException("Le type de transaction doit être 'Sell' pour un ordre de vente");
            }

            // Récupérer l'actif par son symbole
            var asset = await _assetRepository.GetBySymbolAsync(createTransactionDto.Symbol);
            if (asset == null)
            {
                throw new ArgumentException($"L'actif avec le symbole {createTransactionDto.Symbol} n'a pas été trouvé");
            }

            // Vérifier que l'utilisateur possède suffisamment de l'actif pour vendre
            var balance = await _transactionRepository.GetUserBalanceForAssetAsync(createTransactionDto.UserId, asset.Id);
            if (balance < createTransactionDto.Quantity)
            {
                throw new InvalidOperationException($"Solde insuffisant. Vous possédez {balance} {createTransactionDto.Symbol} mais essayez d'en vendre {createTransactionDto.Quantity}");
            }

            // Exécuter l'ordre de vente via Coinbase
            var transaction = await _coinbaseService.ExecuteSellOrderAsync(
                createTransactionDto.UserId,
                createTransactionDto.Symbol,
                createTransactionDto.Quantity,
                createTransactionDto.LimitPrice);

            // Compléter les informations de la transaction
            transaction.AssetId = asset.Id;
            transaction.Asset = asset;
            transaction.StrategyId = createTransactionDto.StrategyId;
            transaction.Notes = createTransactionDto.Notes;

            // Enregistrer la transaction dans notre base de données
            var savedTransaction = await _transactionRepository.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(savedTransaction);
        }

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        public async Task<bool> UpdateTransactionAsync(UpdateTransactionDto updateTransactionDto)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(updateTransactionDto.Id);
            if (existingTransaction == null)
            {
                return false;
            }

            _mapper.Map(updateTransactionDto, existingTransaction);
            return await _transactionRepository.UpdateAsync(existingTransaction);
        }

        /// <summary>
        /// Récupère le solde du portefeuille d'un utilisateur
        /// </summary>
        public async Task<PortfolioSummaryDto> GetUserPortfolioAsync(string userId)
        {
            // Récupérer les soldes pour chaque actif
            var balances = await _transactionRepository.GetUserBalancesAsync(userId);
            var portfolioAssets = new List<PortfolioAssetDto>();
            decimal totalValue = 0;

            foreach (var balance in balances)
            {
                if (balance.Value <= 0)
                    continue;

                var asset = await _assetRepository.GetByIdAsync(balance.Key);
                if (asset == null)
                    continue;

                var valueUsd = balance.Value * asset.CurrentPrice;
                totalValue += valueUsd;

                portfolioAssets.Add(new PortfolioAssetDto
                {
                    Asset = _mapper.Map<AssetDto>(asset),
                    Quantity = balance.Value,
                    ValueUsd = valueUsd,
                    Change24h = asset.PriceChangePercentage24h,
                    // Ces valeurs nécessiteraient des calculs plus complexes basés sur l'historique des transactions
                    AverageBuyPrice = 0, // À calculer
                    UnrealizedPnl = 0,   // À calculer
                    PortfolioPercentage = 0 // Sera calculé après avoir obtenu la valeur totale
                });
            }

            // Calculer le pourcentage du portefeuille pour chaque actif
            if (totalValue > 0)
            {
                foreach (var asset in portfolioAssets)
                {
                    asset.PortfolioPercentage = (asset.ValueUsd / totalValue) * 100;
                }
            }

            // Calculer la variation sur 24h du portefeuille total
            decimal change24h = 0;
            if (totalValue > 0)
            {
                decimal weightedChange = 0;
                foreach (var asset in portfolioAssets)
                {
                    weightedChange += (asset.ValueUsd / totalValue) * asset.Change24h;
                }
                change24h = weightedChange;
            }

            return new PortfolioSummaryDto
            {
                UserId = userId,
                TotalValue = totalValue,
                Change24h = change24h,
                Assets = portfolioAssets
            };
        }

        /// <summary>
        /// Synchronise les transactions depuis Coinbase
        /// </summary>
        public async Task<int> SyncTransactionsFromCoinbaseAsync(string userId, DateTime? startDate = null)
        {
            try
            {
                // Récupérer les transactions depuis Coinbase
                var coinbaseTransactions = await _coinbaseService.GetTransactionHistoryAsync(userId, startDate);
                int syncCount = 0;

                foreach (var tx in coinbaseTransactions)
                {
                    // Vérifier si la transaction existe déjà dans notre base de données
                    var existingTransactions = await _transactionRepository.GetAllAsync();
                    bool exists = existingTransactions.Any(t => t.CoinbaseTransactionId == tx.CoinbaseTransactionId);

                    if (!exists)
                    {
                        // Récupérer ou créer l'actif associé
                        var asset = await _assetRepository.GetBySymbolAsync(tx.Asset?.Symbol);
                        if (asset == null && tx.Asset != null)
                        {
                            asset = await _assetRepository.AddAsync(tx.Asset);
                        }

                        if (asset != null)
                        {
                            tx.AssetId = asset.Id;
                            await _transactionRepository.AddAsync(tx);
                            syncCount++;
                        }
                    }
                }

                return syncCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la synchronisation des transactions: {ex.Message}", ex);
            }
        }
    }
}
