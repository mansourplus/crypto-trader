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
    /// Service d'application pour la gestion des actifs
    /// </summary>
    public class AssetService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ICoinbaseService _coinbaseService;
        private readonly IMapper _mapper;

        public AssetService(IAssetRepository assetRepository, ICoinbaseService coinbaseService, IMapper mapper)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _coinbaseService = coinbaseService ?? throw new ArgumentNullException(nameof(coinbaseService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Récupère tous les actifs
        /// </summary>
        public async Task<IEnumerable<AssetDto>> GetAllAssetsAsync()
        {
            var assets = await _assetRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AssetDto>>(assets);
        }

        /// <summary>
        /// Récupère un actif par son identifiant
        /// </summary>
        public async Task<AssetDto> GetAssetByIdAsync(Guid id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            return _mapper.Map<AssetDto>(asset);
        }

        /// <summary>
        /// Récupère un actif par son symbole
        /// </summary>
        public async Task<AssetDto> GetAssetBySymbolAsync(string symbol)
        {
            var asset = await _assetRepository.GetBySymbolAsync(symbol);
            
            // Si l'actif n'existe pas dans notre base de données, essayer de le récupérer via Coinbase
            if (asset == null)
            {
                try
                {
                    var coinbaseAsset = await _coinbaseService.GetMarketDataAsync(symbol);
                    if (coinbaseAsset != null)
                    {
                        asset = await _assetRepository.AddAsync(coinbaseAsset);
                    }
                }
                catch (Exception)
                {
                    // Gérer l'erreur ou la propager selon les besoins
                    return null;
                }
            }
            
            return _mapper.Map<AssetDto>(asset);
        }

        /// <summary>
        /// Récupère les actifs les plus performants selon un critère donné
        /// </summary>
        public async Task<IEnumerable<AssetDto>> GetTopPerformingAssetsAsync(int count, string criteria = "marketCap")
        {
            var assets = await _assetRepository.GetTopPerformingAsync(count, criteria);
            return _mapper.Map<IEnumerable<AssetDto>>(assets);
        }

        /// <summary>
        /// Ajoute un nouvel actif
        /// </summary>
        public async Task<AssetDto> AddAssetAsync(CreateAssetDto createAssetDto)
        {
            var asset = _mapper.Map<Asset>(createAssetDto);
            var result = await _assetRepository.AddAsync(asset);
            return _mapper.Map<AssetDto>(result);
        }

        /// <summary>
        /// Met à jour un actif existant
        /// </summary>
        public async Task<bool> UpdateAssetAsync(UpdateAssetDto updateAssetDto)
        {
            var existingAsset = await _assetRepository.GetByIdAsync(updateAssetDto.Id);
            if (existingAsset == null)
            {
                return false;
            }
            
            _mapper.Map(updateAssetDto, existingAsset);
            return await _assetRepository.UpdateAsync(existingAsset);
        }

        /// <summary>
        /// Supprime un actif
        /// </summary>
        public async Task<bool> DeleteAssetAsync(Guid id)
        {
            return await _assetRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Met à jour les données de marché pour tous les actifs
        /// </summary>
        public async Task UpdateMarketDataAsync()
        {
            // Récupérer tous les actifs
            var assets = await _assetRepository.GetAllAsync();
            var symbols = new List<string>();
            
            foreach (var asset in assets)
            {
                symbols.Add(asset.Symbol);
            }
            
            // Récupérer les données de marché en batch
            var marketData = await _coinbaseService.GetMarketDataBatchAsync(symbols);
            
            // Mettre à jour chaque actif
            foreach (var data in marketData)
            {
                var asset = await _assetRepository.GetBySymbolAsync(data.Symbol);
                if (asset != null)
                {
                    asset.CurrentPrice = data.CurrentPrice;
                    asset.LastUpdated = DateTime.UtcNow;
                    
                    // Mettre à jour d'autres propriétés si disponibles
                    if (data.PriceChangePercentage24h != 0)
                    {
                        asset.PriceChangePercentage24h = data.PriceChangePercentage24h;
                    }
                    
                    if (data.MarketCap != 0)
                    {
                        asset.MarketCap = data.MarketCap;
                    }
                    
                    if (data.Volume24h != 0)
                    {
                        asset.Volume24h = data.Volume24h;
                    }
                    
                    await _assetRepository.UpdateAsync(asset);
                }
            }
        }
    }
}
