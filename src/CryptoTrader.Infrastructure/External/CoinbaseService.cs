using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;
using CryptoTrader.Core.Exceptions;
using CryptoTrader.Core.Interfaces;

namespace CryptoTrader.Infrastructure.External
{
    /// <summary>
    /// Implémentation du service d'intégration avec l'API Coinbase
    /// </summary>
    public class CoinbaseService : ICoinbaseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _baseUrl = "https://api.coinbase.com/v2/";

        public CoinbaseService(HttpClient httpClient, string apiKey, string apiSecret)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Récupère le solde du portefeuille de l'utilisateur
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetWalletBalancesAsync(string userId)
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var requestPath = "accounts";
                
                var signature = GenerateSignature("GET", requestPath, "", timestamp);
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
                
                var response = await _httpClient.GetAsync(requestPath);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var accountsResponse = JsonSerializer.Deserialize<CoinbaseAccountsResponse>(content);
                
                var balances = new Dictionary<string, decimal>();
                
                if (accountsResponse?.Data != null)
                {
                    foreach (var account in accountsResponse.Data)
                    {
                        if (decimal.TryParse(account.Balance.Amount, out var amount) && amount > 0)
                        {
                            balances[account.Currency.Code] = amount;
                        }
                    }
                }
                
                return balances;
            }
            catch (HttpRequestException ex)
            {
                throw new CoinbaseApiException($"Erreur lors de la récupération des soldes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Récupère l'historique des transactions de l'utilisateur
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Implémentation simplifiée pour l'exemple
            // Dans une implémentation réelle, il faudrait paginer les résultats et gérer les différents types de transactions
            try
            {
                var transactions = new List<Transaction>();
                var accounts = await GetAccountsAsync();
                
                foreach (var account in accounts)
                {
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    var requestPath = $"accounts/{account.Id}/transactions";
                    
                    var signature = GenerateSignature("GET", requestPath, "", timestamp);
                    
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _apiKey);
                    _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
                    _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
                    
                    var response = await _httpClient.GetAsync(requestPath);
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var transactionsResponse = JsonSerializer.Deserialize<CoinbaseTransactionsResponse>(content);
                    
                    if (transactionsResponse?.Data != null)
                    {
                        foreach (var tx in transactionsResponse.Data)
                        {
                            // Convertir les transactions Coinbase en transactions de notre domaine
                            var transaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                CoinbaseTransactionId = tx.Id,
                                Type = tx.Type == "buy" ? TransactionType.Buy : TransactionType.Sell,
                                Quantity = decimal.Parse(tx.Amount.Amount),
                                Price = tx.NativeAmount != null ? decimal.Parse(tx.NativeAmount.Amount) / decimal.Parse(tx.Amount.Amount) : 0,
                                TotalAmount = tx.NativeAmount != null ? decimal.Parse(tx.NativeAmount.Amount) : 0,
                                Timestamp = DateTime.Parse(tx.CreatedAt),
                                Status = TransactionStatus.Completed,
                                Notes = tx.Description
                            };
                            
                            transactions.Add(transaction);
                        }
                    }
                }
                
                // Filtrer par date si nécessaire
                if (startDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Timestamp >= startDate.Value).ToList();
                }
                
                if (endDate.HasValue)
                {
                    transactions = transactions.Where(t => t.Timestamp <= endDate.Value).ToList();
                }
                
                return transactions;
            }
            catch (HttpRequestException ex)
            {
                throw new CoinbaseApiException($"Erreur lors de la récupération de l'historique des transactions: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exécute un ordre d'achat
        /// </summary>
        public async Task<Transaction> ExecuteBuyOrderAsync(string userId, string symbol, decimal quantity, decimal? limitPrice = null)
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var requestPath = "buys";
                
                var requestBody = new
                {
                    amount = quantity.ToString("0.########"),
                    currency = symbol,
                    payment_method = "bank_account" // À remplacer par l'ID de la méthode de paiement réelle
                };
                
                var jsonBody = JsonSerializer.Serialize(requestBody);
                var signature = GenerateSignature("POST", requestPath, jsonBody, timestamp);
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
                
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestPath, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var buyResponse = JsonSerializer.Deserialize<CoinbaseBuyResponse>(responseContent);
                
                if (buyResponse?.Data != null)
                {
                    return new Transaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CoinbaseTransactionId = buyResponse.Data.Id,
                        Type = TransactionType.Buy,
                        Quantity = decimal.Parse(buyResponse.Data.Amount.Amount),
                        Price = decimal.Parse(buyResponse.Data.Subtotal.Amount) / decimal.Parse(buyResponse.Data.Amount.Amount),
                        TotalAmount = decimal.Parse(buyResponse.Data.Total.Amount),
                        Fee = decimal.Parse(buyResponse.Data.Fee.Amount),
                        Timestamp = DateTime.Parse(buyResponse.Data.CreatedAt),
                        Status = ConvertCoinbaseStatus(buyResponse.Data.Status)
                    };
                }
                
                throw new CoinbaseApiException("Réponse invalide de l'API Coinbase lors de l'achat");
            }
            catch (HttpRequestException ex)
            {
                throw new CoinbaseApiException($"Erreur lors de l'exécution de l'ordre d'achat: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exécute un ordre de vente
        /// </summary>
        public async Task<Transaction> ExecuteSellOrderAsync(string userId, string symbol, decimal quantity, decimal? limitPrice = null)
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var requestPath = "sells";
                
                var requestBody = new
                {
                    amount = quantity.ToString("0.########"),
                    currency = symbol
                };
                
                var jsonBody = JsonSerializer.Serialize(requestBody);
                var signature = GenerateSignature("POST", requestPath, jsonBody, timestamp);
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
                
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestPath, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var sellResponse = JsonSerializer.Deserialize<CoinbaseSellResponse>(responseContent);
                
                if (sellResponse?.Data != null)
                {
                    return new Transaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CoinbaseTransactionId = sellResponse.Data.Id,
                        Type = TransactionType.Sell,
                        Quantity = decimal.Parse(sellResponse.Data.Amount.Amount),
                        Price = decimal.Parse(sellResponse.Data.Subtotal.Amount) / decimal.Parse(sellResponse.Data.Amount.Amount),
                        TotalAmount = decimal.Parse(sellResponse.Data.Total.Amount),
                        Fee = decimal.Parse(sellResponse.Data.Fee.Amount),
                        Timestamp = DateTime.Parse(sellResponse.Data.CreatedAt),
                        Status = ConvertCoinbaseStatus(sellResponse.Data.Status)
                    };
                }
                
                throw new CoinbaseApiException("Réponse invalide de l'API Coinbase lors de la vente");
            }
            catch (HttpRequestException ex)
            {
                throw new CoinbaseApiException($"Erreur lors de l'exécution de l'ordre de vente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Récupère les données de marché en temps réel pour un actif
        /// </summary>
        public async Task<Asset> GetMarketDataAsync(string symbol)
        {
            try
            {
                var response = await _httpClient.GetAsync($"prices/{symbol}-USD/spot");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var priceResponse = JsonSerializer.Deserialize<CoinbasePriceResponse>(content);
                
                if (priceResponse?.Data != null)
                {
                    // Récupérer des informations supplémentaires sur l'actif
                    var assetResponse = await _httpClient.GetAsync($"assets/search?query={symbol}");
                    assetResponse.EnsureSuccessStatusCode();
                    
                    var assetContent = await assetResponse.Content.ReadAsStringAsync();
                    var assetDataResponse = JsonSerializer.Deserialize<CoinbaseAssetResponse>(assetContent);
                    
                    var asset = new Asset
                    {
                        Id = Guid.NewGuid(),
                        Symbol = symbol,
                        CurrentPrice = decimal.Parse(priceResponse.Data.Amount),
                        LastUpdated = DateTime.UtcNow
                    };
                    
                    if (assetDataResponse?.Data != null && assetDataResponse.Data.Length > 0)
                    {
                        var assetData = assetDataResponse.Data[0];
                        asset.Name = assetData.Name;
                        asset.ImageUrl = assetData.Image;
                    }
                    
                    return asset;
                }
                
                throw new CoinbaseApiException($"Données de marché non disponibles pour {symbol}");
            }
            catch (HttpRequestException ex)
            {
                throw new CoinbaseApiException($"Erreur lors de la récupération des données de marché: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Récupère les données de marché en temps réel pour plusieurs actifs
        /// </summary>
        public async Task<IEnumerable<Asset>> GetMarketDataBatchAsync(IEnumerable<string> symbols)
        {
            var assets = new List<Asset>();
            
            foreach (var symbol in symbols)
            {
                try
                {
                    var asset = await GetMarketDataAsync(symbol);
                    assets.Add(asset);
                }
                catch (Exception)
                {
                    // Continuer avec le symbole suivant en cas d'erreur
                    continue;
                }
            }
            
            return assets;
        }

        /// <summary>
        /// Récupère l'historique des prix pour un actif
        /// </summary>
        public async Task<IEnumerable<PricePoint>> GetPriceHistoryAsync(string symbol, string timeframe, int limit)
        {
            // Note: L'API Coinbase Pro serait plus adaptée pour cette fonctionnalité
            // Ceci est une implémentation simplifiée
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = timeframe.ToLower() switch
                {
                    "day" => endDate.AddDays(-1),
                    "week" => endDate.AddDays(-7),
                    "month" => endDate.AddMonths(-1),
                    "year" => endDate.AddYears(-1),
                    _ => endDate.AddDays(-1)
                };
                
                var response = await _httpClient.GetAsync($"prices/{symbol}-USD/historic?start={startDate:yyyy-MM-dd}&end={endDate:yyyy-MM-dd}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var priceHistoryResponse = JsonSerializer.Deserialize<CoinbasePriceHistoryResponse>(content);
                
                var pricePoints = new List<PricePoint>();
                
                if (priceHistoryResponse?.Data?.Prices != null)
                {
                    foreach (var price in priceHistoryResponse.Data.Prices)
                    {
                        pricePoints.Add(new PricePoint
                        {
                            Timestamp = DateTime.Parse(price.Time),
                            Close = decimal.Parse(price.Price),
                            Open = decimal.Parse(price.Price), // Simplifié
                            High = decimal.Parse(price.Price), // Simplifié
                            Low = decimal.Parse(price.Price),  // Simplifié
                            Volume = 0 // Non disponible dans cette API
                        });
                    }
                }
                
                return pricePoints.Take(limit).ToList();
            }
            catch (HttpRequestException ex)
            {
                throw new CoinbaseApiException($"Erreur lors de la récupération de l'historique des prix: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Récupère le carnet d'ordres pour un actif
        /// </summary>
        public async Task<OrderBook> GetOrderBookAsync(string symbol)
        {
            // Note: L'API Coinbase Pro serait plus adaptée pour cette fonctionnalité
            // Ceci est une implémentation simulée
            return new OrderBook
            {
                Symbol = symbol,
                Timestamp = DateTime.UtcNow,
                Bids = GenerateSimulatedOrderBookEntries(10, true),
                Asks = GenerateSimulatedOrderBookEntries(10, false)
            };
        }

        /// <summary>
        /// Vérifie si l'API Coinbase est disponible
        /// </summary>
        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("time");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie la validité des identifiants API de l'utilisateur
        /// </summary>
        public async Task<bool> ValidateApiCredentialsAsync(string apiKey, string apiSecret)
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var requestPath = "user";
                
                var signature = GenerateSignature("GET", requestPath, "", timestamp, apiSecret);
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", apiKey);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
                _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
                
                var response = await _httpClient.GetAsync(requestPath);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #region Méthodes privées

        /// <summary>
        /// Génère une signature HMAC pour l'authentification à l'API Coinbase
        /// </summary>
        private string GenerateSignature(string method, string requestPath, string body, string timestamp, string secret = null)
        {
            var message = timestamp + method + "/" + requestPath + body;
            var secretBytes = Encoding.UTF8.GetBytes(secret ?? _apiSecret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            
            using var hmac = new HMACSHA256(secretBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(hash).ToLower();
        }

        /// <summary>
        /// Récupère les comptes de l'utilisateur
        /// </summary>
        private async Task<List<CoinbaseAccount>> GetAccountsAsync()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var requestPath = "accounts";
            
            var signature = GenerateSignature("GET", requestPath, "", timestamp);
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
            
            var response = await _httpClient.GetAsync(requestPath);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var accountsResponse = JsonSerializer.Deserialize<CoinbaseAccountsResponse>(content);
            
            return accountsResponse?.Data ?? new List<CoinbaseAccount>();
        }

        /// <summary>
        /// Convertit le statut Coinbase en statut de notre domaine
        /// </summary>
        private TransactionStatus ConvertCoinbaseStatus(string status)
        {
            return status.ToLower() switch
            {
                "created" => TransactionStatus.Pending,
                "completed" => TransactionStatus.Completed,
                "canceled" => TransactionStatus.Cancelled,
                _ => TransactionStatus.Pending
            };
        }

        /// <summary>
        /// Génère des entrées simulées pour le carnet d'ordres
        /// </summary>
        private List<OrderBookEntry> GenerateSimulatedOrderBookEntries(int count, bool isBid)
        {
            var random = new Random();
            var entries = new List<OrderBookEntry>();
            var basePrice = 50000m; // Prix de base simulé
            
            for (int i = 0; i < count; i++)
            {
                var priceOffset = (decimal)random.NextDouble() * 1000;
                var price = isBid ? basePrice - priceOffset : basePrice + priceOffset;
                var quantity = (decimal)random.NextDouble() * 10;
                
                entries.Add(new OrderBookEntry
                {
                    Price = price,
                    Quantity = quantity
                });
            }
            
            return entries.OrderByDescending(e => isBid ? e.Price : -e.Price).ToList();
        }

        #endregion

        #region Classes de réponse Coinbase

        private class CoinbaseAccountsResponse
        {
            public List<CoinbaseAccount> Data { get; set; }
        }

        private class CoinbaseAccount
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public CoinbaseAmount Balance { get; set; }
            public CoinbaseCurrency Currency { get; set; }
        }

        private class CoinbaseAmount
        {
            public string Amount { get; set; }
            public string Currency { get; set; }
        }

        private class CoinbaseCurrency
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        private class CoinbaseTransactionsResponse
        {
            public List<CoinbaseTransaction> Data { get; set; }
        }

        private class CoinbaseTransaction
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
            public CoinbaseAmount Amount { get; set; }
            public CoinbaseAmount NativeAmount { get; set; }
            public string Description { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
        }

        private class CoinbaseBuyResponse
        {
            public CoinbaseBuy Data { get; set; }
        }

        private class CoinbaseBuy
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public CoinbaseAmount Amount { get; set; }
            public CoinbaseAmount Subtotal { get; set; }
            public CoinbaseAmount Fee { get; set; }
            public CoinbaseAmount Total { get; set; }
            public string CreatedAt { get; set; }
        }

        private class CoinbaseSellResponse
        {
            public CoinbaseSell Data { get; set; }
        }

        private class CoinbaseSell
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public CoinbaseAmount Amount { get; set; }
            public CoinbaseAmount Subtotal { get; set; }
            public CoinbaseAmount Fee { get; set; }
            public CoinbaseAmount Total { get; set; }
            public string CreatedAt { get; set; }
        }

        private class CoinbasePriceResponse
        {
            public CoinbasePrice Data { get; set; }
        }

        private class CoinbasePrice
        {
            public string Base { get; set; }
            public string Currency { get; set; }
            public string Amount { get; set; }
        }

        private class CoinbaseAssetResponse
        {
            public CoinbaseAssetData[] Data { get; set; }
        }

        private class CoinbaseAssetData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string Image { get; set; }
        }

        private class CoinbasePriceHistoryResponse
        {
            public CoinbasePriceHistoryData Data { get; set; }
        }

        private class CoinbasePriceHistoryData
        {
            public CoinbasePricePoint[] Prices { get; set; }
        }

        private class CoinbasePricePoint
        {
            public string Price { get; set; }
            public string Time { get; set; }
        }

        #endregion
    }
}
