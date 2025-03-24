using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Entities;
using CryptoTrader.Core.Interfaces;

namespace CryptoTrader.Infrastructure.External
{
    /// <summary>
    /// Implémentation du service de recommandations de trading
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ICoinbaseService _coinbaseService;

        public RecommendationService(IAssetRepository assetRepository, ICoinbaseService coinbaseService)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _coinbaseService = coinbaseService ?? throw new ArgumentNullException(nameof(coinbaseService));
        }

        /// <summary>
        /// Récupère les meilleures cryptomonnaies à acheter selon différents critères
        /// </summary>
        public async Task<IEnumerable<AssetRecommendation>> GetTopCryptosAsync(int count, RecommendationCriteria criteria)
        {
            // Récupérer les actifs selon le critère spécifié
            var assets = await _assetRepository.GetTopPerformingAsync(count, criteria.ToString().ToLower());
            var recommendations = new List<AssetRecommendation>();

            foreach (var asset in assets)
            {
                // Analyser chaque actif pour générer une recommandation
                var recommendation = await AnalyzeAssetInternalAsync(asset);
                recommendations.Add(recommendation);
            }

            return recommendations;
        }

        /// <summary>
        /// Analyse un actif spécifique et fournit des recommandations
        /// </summary>
        public async Task<AssetRecommendation> AnalyzeAssetAsync(string symbol)
        {
            var asset = await _assetRepository.GetBySymbolAsync(symbol);
            if (asset == null)
            {
                // Si l'actif n'existe pas dans notre base de données, essayer de le récupérer via Coinbase
                asset = await _coinbaseService.GetMarketDataAsync(symbol);
                if (asset != null)
                {
                    await _assetRepository.AddAsync(asset);
                }
                else
                {
                    throw new ArgumentException($"L'actif avec le symbole {symbol} n'a pas été trouvé.");
                }
            }

            return await AnalyzeAssetInternalAsync(asset);
        }

        /// <summary>
        /// Détermine le meilleur moment pour acheter un actif spécifique
        /// </summary>
        public async Task<TimingRecommendation> GetBestBuyTimingAsync(string symbol)
        {
            // Récupérer l'historique des prix pour analyser les tendances
            var priceHistory = await _coinbaseService.GetPriceHistoryAsync(symbol, "week", 168); // 7 jours x 24 heures
            
            // Analyser les données pour déterminer le meilleur moment d'achat
            // Cette implémentation est simplifiée et utilise des heuristiques de base
            
            // 1. Identifier les périodes de faible activité (généralement le week-end ou tôt le matin)
            var now = DateTime.UtcNow;
            var dayOfWeek = now.DayOfWeek;
            
            // Le lundi matin est souvent un bon moment pour acheter (selon les spécifications)
            var optimalTime = dayOfWeek == DayOfWeek.Monday ? now.Date.AddHours(9) : 
                              dayOfWeek == DayOfWeek.Friday ? now.Date.AddDays(3).AddHours(9) : // Prochain lundi
                              now.Date.AddDays((8 - (int)dayOfWeek) % 7).AddHours(9); // Prochain lundi
            
            // 2. Estimer le prix attendu en fonction des tendances récentes
            var recentPrices = priceHistory.OrderByDescending(p => p.Timestamp).Take(24).ToList();
            var avgPrice = recentPrices.Average(p => p.Close);
            var minPrice = recentPrices.Min(p => p.Low);
            var expectedPrice = minPrice + (avgPrice - minPrice) * 0.8m; // Estimation conservatrice
            
            return new TimingRecommendation
            {
                Symbol = symbol,
                OptimalTime = optimalTime,
                Reasoning = "Les lundis matins présentent généralement une activité de marché plus faible, ce qui peut offrir de meilleures opportunités d'achat.",
                ExpectedPrice = expectedPrice,
                ConfidenceScore = 0.7m,
                SupportingFactors = new List<string>
                {
                    "Analyse des tendances historiques de prix",
                    "Patterns d'activité hebdomadaires",
                    "Volatilité réduite pendant les périodes de faible volume"
                }
            };
        }

        /// <summary>
        /// Détermine le meilleur moment pour vendre un actif spécifique
        /// </summary>
        public async Task<TimingRecommendation> GetBestSellTimingAsync(string symbol)
        {
            // Récupérer l'historique des prix pour analyser les tendances
            var priceHistory = await _coinbaseService.GetPriceHistoryAsync(symbol, "week", 168); // 7 jours x 24 heures
            
            // Analyser les données pour déterminer le meilleur moment de vente
            // Cette implémentation est simplifiée et utilise des heuristiques de base
            
            // 1. Identifier les périodes de forte activité (généralement en milieu de semaine)
            var now = DateTime.UtcNow;
            var dayOfWeek = now.DayOfWeek;
            
            // Le mercredi ou jeudi après-midi est souvent un bon moment pour vendre
            var optimalTime = dayOfWeek == DayOfWeek.Wednesday ? now.Date.AddHours(15) : 
                              dayOfWeek == DayOfWeek.Thursday ? now.Date.AddHours(15) :
                              now.Date.AddDays(((10 - (int)dayOfWeek) % 7) + (dayOfWeek >= DayOfWeek.Thursday ? 7 : 0)).AddHours(15); // Prochain mercredi
            
            // 2. Estimer le prix attendu en fonction des tendances récentes
            var recentPrices = priceHistory.OrderByDescending(p => p.Timestamp).Take(24).ToList();
            var avgPrice = recentPrices.Average(p => p.Close);
            var maxPrice = recentPrices.Max(p => p.High);
            var expectedPrice = avgPrice + (maxPrice - avgPrice) * 0.7m; // Estimation optimiste
            
            return new TimingRecommendation
            {
                Symbol = symbol,
                OptimalTime = optimalTime,
                Reasoning = "Les mercredis et jeudis après-midi présentent généralement une activité de marché plus élevée, ce qui peut offrir de meilleures opportunités de vente.",
                ExpectedPrice = expectedPrice,
                ConfidenceScore = 0.65m,
                SupportingFactors = new List<string>
                {
                    "Analyse des tendances historiques de prix",
                    "Patterns d'activité hebdomadaires",
                    "Volatilité accrue pendant les périodes de fort volume"
                }
            };
        }

        /// <summary>
        /// Génère des recommandations personnalisées pour un utilisateur spécifique
        /// </summary>
        public async Task<IEnumerable<AssetRecommendation>> GetPersonalizedRecommendationsAsync(string userId, int count)
        {
            // Dans une implémentation réelle, nous utiliserions l'historique des transactions de l'utilisateur
            // et ses préférences pour personnaliser les recommandations
            // Pour l'instant, nous retournons simplement les meilleures cryptos selon la capitalisation boursière
            
            return await GetTopCryptosAsync(count, RecommendationCriteria.MarketCap);
        }

        /// <summary>
        /// Récupère les signaux techniques pour un actif spécifique
        /// </summary>
        public async Task<TechnicalSignals> GetTechnicalSignalsAsync(string symbol)
        {
            // Récupérer l'historique des prix pour calculer les indicateurs techniques
            var priceHistory = await _coinbaseService.GetPriceHistoryAsync(symbol, "month", 200);
            var prices = priceHistory.OrderBy(p => p.Timestamp).ToList();
            
            // Calculer les moyennes mobiles
            var sma50 = CalculateSMA(prices, 50);
            var sma200 = CalculateSMA(prices, 200);
            var ema20 = CalculateEMA(prices, 20);
            
            // Détecter les croisements
            var goldenCross = sma50 > sma200 && CalculateSMA(prices.Skip(1).ToList(), 50) <= CalculateSMA(prices.Skip(1).ToList(), 200);
            var deathCross = sma50 < sma200 && CalculateSMA(prices.Skip(1).ToList(), 50) >= CalculateSMA(prices.Skip(1).ToList(), 200);
            
            // Calculer le RSI
            var rsi = CalculateRSI(prices, 14);
            
            // Calculer le MACD
            var (macd, signal, histogram) = CalculateMACD(prices);
            
            // Déterminer les niveaux de support et résistance
            var supportLevel = CalculateSupportLevel(prices);
            var resistanceLevel = CalculateResistanceLevel(prices);
            
            // Déterminer le signal global
            var overallSignal = DetermineOverallSignal(rsi, macd, signal, goldenCross, deathCross);
            
            return new TechnicalSignals
            {
                Symbol = symbol,
                Timestamp = DateTime.UtcNow,
                SMA50 = sma50,
                SMA200 = sma200,
                EMA20 = ema20,
                GoldenCross = goldenCross,
                DeathCross = deathCross,
                RSI = rsi,
                MACD = macd,
                MACDSignal = signal,
                MACDHistogram = histogram,
                SupportLevel = supportLevel,
                ResistanceLevel = resistanceLevel,
                OverallSignal = overallSignal,
                SignalDescription = GetSignalDescription(overallSignal, rsi, macd, signal, goldenCross, deathCross)
            };
        }

        #region Méthodes privées

        /// <summary>
        /// Analyse un actif et génère une recommandation
        /// </summary>
        private async Task<AssetRecommendation> AnalyzeAssetInternalAsync(Asset asset)
        {
            // Récupérer les signaux techniques
            var technicalSignals = await GetTechnicalSignalsAsync(asset.Symbol);
            
            // Déterminer le type de recommandation en fonction des signaux techniques
            var recommendationType = technicalSignals.OverallSignal;
            
            // Calculer le score de confiance en fonction de la force des signaux
            var confidenceScore = CalculateConfidenceScore(technicalSignals);
            
            // Estimer le retour potentiel en fonction des tendances récentes
            var potentialReturn = EstimatePotentialReturn(asset, technicalSignals);
            
            // Évaluer le niveau de risque
            var riskLevel = EvaluateRiskLevel(asset, technicalSignals);
            
            // Suggérer une période de détention
            var holdingPeriod = SuggestHoldingPeriod(recommendationType, riskLevel);
            
            // Suggérer un prix d'entrée
            var entryPrice = SuggestEntryPrice(asset, technicalSignals);
            
            // Suggérer un prix de sortie (si applicable)
            decimal? exitPrice = recommendationType == RecommendationType.Buy || 
                           recommendationType == RecommendationType.StrongBuy ? 
                           SuggestExitPrice(asset, entryPrice, potentialReturn) : null;
            
            return new AssetRecommendation
            {
                Asset = asset,
                Type = recommendationType,
                Reasoning = technicalSignals.SignalDescription,
                ConfidenceScore = confidenceScore,
                PotentialReturn = potentialReturn,
                RiskLevel = riskLevel,
                SuggestedHoldingPeriod = holdingPeriod,
                SuggestedEntryPrice = entryPrice,
                SuggestedExitPrice = exitPrice,
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Calcule la moyenne mobile simple (SMA)
        /// </summary>
        private decimal CalculateSMA(List<PricePoint> prices, int period)
        {
            if (prices.Count < period)
                return prices.Average(p => p.Close);
            
            return prices.TakeLast(period).Average(p => p.Close);
        }

        /// <summary>
        /// Calcule la moyenne mobile exponentielle (EMA)
        /// </summary>
        private decimal CalculateEMA(List<PricePoint> prices, int period)
        {
            if (prices.Count < period)
                return prices.Average(p => p.Close);
            
            var ema = prices.Take(period).Average(p => p.Close);
            var multiplier = 2.0m / (period + 1);
            
            foreach (var price in prices.Skip(period))
            {
                ema = (price.Close - ema) * multiplier + ema;
            }
            
            return ema;
        }

        /// <summary>
        /// Calcule l'indice de force relative (RSI)
        /// </summary>
        private decimal CalculateRSI(List<PricePoint> prices, int period)
        {
            if (prices.Count <= period)
                return 50; // Valeur neutre par défaut
            
            var gains = new List<decimal>();
            var losses = new List<decimal>();
            
            for (int i = 1; i < prices.Count; i++)
            {
                var change = prices[i].Close - prices[i - 1].Close;
                if (change >= 0)
                {
                    gains.Add(change);
                    losses.Add(0);
                }
                else
                {
                    gains.Add(0);
                    losses.Add(-change);
                }
            }
            
            var avgGain = gains.TakeLast(period).Average();
            var avgLoss = losses.TakeLast(period).Average();
            
            if (avgLoss == 0)
                return 100;
            
            var rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        /// <summary>
        /// Calcule le MACD (Moving Average Convergence Divergence)
        /// </summary>
        private (decimal macd, decimal signal, decimal histogram) CalculateMACD(List<PricePoint> prices)
        {
            var ema12 = CalculateEMA(prices, 12);
            var ema26 = CalculateEMA(prices, 26);
            var macd = ema12 - ema26;
            
            // Calculer la ligne de signal (EMA de 9 périodes du MACD)
            var macdPoints = new List<PricePoint>();
            for (int i = 0; i < 9; i++)
            {
                macdPoints.Add(new PricePoint { Close = macd });
            }
            
            var signal = CalculateEMA(macdPoints, 9);
            var histogram = macd - signal;
            
            return (macd, signal, histogram);
        }

        /// <summary>
        /// Calcule le niveau de support
        /// </summary>
        private decimal CalculateSupportLevel(List<PricePoint> prices)
        {
            // Méthode simplifiée : utiliser le minimum des 14 derniers jours
            return prices.TakeLast(14).Min(p => p.Low);
        }

        /// <summary>
        /// Calcule le niveau de résistance
        /// </summary>
        private decimal CalculateResistanceLevel(List<PricePoint> prices)
        {
            // Méthode simplifiée : utiliser le maximum des 14 derniers jours
            return prices.TakeLast(14).Max(p => p.High);
        }

        /// <summary>
        /// Détermine le signal global en fonction des indicateurs techniques
        /// </summary>
        private RecommendationType DetermineOverallSignal(decimal rsi, decimal macd, decimal signal, bool goldenCross, bool deathCross)
        {
            // Règles simplifiées pour déterminer le signal
            if (goldenCross && rsi < 70 && macd > signal)
                return RecommendationType.StrongBuy;
            
            if (macd > signal && rsi < 60)
                return RecommendationType.Buy;
            
            if (deathCross || (rsi > 70 && macd < signal))
                return RecommendationType.StrongSell;
            
            if (macd < signal && rsi > 60)
                return RecommendationType.Sell;
            
            return RecommendationType.Hold;
        }

        /// <summary>
        /// Génère une description du signal technique
        /// </summary>
        private string GetSignalDescription(RecommendationType signal, decimal rsi, decimal macd, decimal macdSignal, bool goldenCross, bool deathCross)
        {
            var description = new List<string>();
            
            if (goldenCross)
                description.Add("Golden Cross détecté (SMA 50 croise au-dessus de SMA 200), indiquant un potentiel début de tendance haussière.");
            
            if (deathCross)
                description.Add("Death Cross détecté (SMA 50 croise en-dessous de SMA 200), indiquant un potentiel début de tendance baissière.");
            
            if (rsi < 30)
                description.Add($"RSI bas ({rsi:F2}) indiquant une condition de survente.");
            else if (rsi > 70)
                description.Add($"RSI élevé ({rsi:F2}) indiquant une condition de surachat.");
            
            if (macd > macdSignal)
                description.Add("MACD au-dessus de la ligne de signal, suggérant une dynamique haussière.");
            else
                description.Add("MACD en-dessous de la ligne de signal, suggérant une dynamique baissière.");
            
            switch (signal)
            {
                case RecommendationType.StrongBuy:
                    description.Insert(0, "Signal d'achat fort basé sur la convergence de plusieurs indicateurs techniques positifs.");
                    break;
                case RecommendationType.Buy:
                    description.Insert(0, "Signal d'achat modéré basé sur des indicateurs techniques généralement positifs.");
                    break;
                case RecommendationType.Hold:
                    description.Insert(0, "Signal neutre. Les indicateurs techniques sont mitigés ou ne montrent pas de tendance claire.");
                    break;
                case RecommendationType.Sell:
                    description.Insert(0, "Signal de vente modéré basé sur des indicateurs techniques généralement négatifs.");
                    break;
                case RecommendationType.StrongSell:
                    description.Insert(0, "Signal de vente fort basé sur la convergence de plusieurs indicateurs techniques négatifs.");
                    break;
            }
            
            return string.Join(" ", description);
        }

        /// <summary>
        /// Calcule le score de confiance en fonction de la force des signaux
        /// </summary>
        private decimal CalculateConfidenceScore(TechnicalSignals signals)
        {
            var score = 0.5m; // Score de base neutre
            
            // Ajuster en fonction du RSI
            if (signals.RSI < 30 || signals.RSI > 70)
                score += 0.1m; // Signal fort de survente ou surachat
            
            // Ajuster en fonction du MACD
            if (Math.Abs(signals.MACD - signals.MACDSignal) > 0.5m)
                score += 0.1m; // Écart significatif entre MACD et signal
            
            // Ajuster en fonction des croisements
            if (signals.GoldenCross || signals.DeathCross)
                score += 0.2m; // Les croisements sont des signaux forts
            
            // Limiter le score entre 0.3 et 0.9
            return Math.Min(0.9m, Math.Max(0.3m, score));
        }

        /// <summary>
        /// Estime le retour potentiel en fonction des tendances récentes
        /// </summary>
        private decimal EstimatePotentialReturn(Asset asset, TechnicalSignals signals)
        {
            // Méthode simplifiée basée sur l'écart entre le prix actuel et les niveaux de support/résistance
            if (signals.OverallSignal == RecommendationType.Buy || signals.OverallSignal == RecommendationType.StrongBuy)
            {
                // Pour un achat, estimer le retour jusqu'au niveau de résistance
                return (signals.ResistanceLevel / asset.CurrentPrice - 1) * 100;
            }
            else if (signals.OverallSignal == RecommendationType.Sell || signals.OverallSignal == RecommendationType.StrongSell)
            {
                // Pour une vente, estimer la perte évitée jusqu'au niveau de support
                return (1 - signals.SupportLevel / asset.CurrentPrice) * 100;
            }
            
            return 0; // Neutre
        }

        /// <summary>
        /// Évalue le niveau de risque
        /// </summary>
        private decimal EvaluateRiskLevel(Asset asset, TechnicalSignals signals)
        {
            // Méthode simplifiée basée sur la volatilité implicite et la distance aux niveaux de support/résistance
            var volatility = 0.5m; // Valeur par défaut
            
            // Ajuster en fonction de la distance au support/résistance
            if (signals.OverallSignal == RecommendationType.Buy || signals.OverallSignal == RecommendationType.StrongBuy)
            {
                // Pour un achat, le risque est lié à la distance au support
                var distanceToSupport = (asset.CurrentPrice / signals.SupportLevel - 1);
                volatility += distanceToSupport * 2;
            }
            else if (signals.OverallSignal == RecommendationType.Sell || signals.OverallSignal == RecommendationType.StrongSell)
            {
                // Pour une vente, le risque est lié à la distance à la résistance
                var distanceToResistance = (signals.ResistanceLevel / asset.CurrentPrice - 1);
                volatility += distanceToResistance * 2;
            }
            
            // Limiter le risque entre 0.2 et 0.9
            return Math.Min(0.9m, Math.Max(0.2m, volatility));
        }

        /// <summary>
        /// Suggère une période de détention
        /// </summary>
        private TimeSpan SuggestHoldingPeriod(RecommendationType recommendationType, decimal riskLevel)
        {
            // Méthode simplifiée basée sur le type de recommandation et le niveau de risque
            switch (recommendationType)
            {
                case RecommendationType.StrongBuy:
                    return TimeSpan.FromDays(riskLevel < 0.5m ? 90 : 30); // Long terme pour faible risque, moyen terme sinon
                case RecommendationType.Buy:
                    return TimeSpan.FromDays(riskLevel < 0.5m ? 30 : 14); // Moyen terme pour faible risque, court terme sinon
                case RecommendationType.Hold:
                    return TimeSpan.FromDays(7); // Court terme
                case RecommendationType.Sell:
                case RecommendationType.StrongSell:
                    return TimeSpan.FromDays(1); // Très court terme (vendre rapidement)
                default:
                    return TimeSpan.FromDays(7);
            }
        }

        /// <summary>
        /// Suggère un prix d'entrée
        /// </summary>
        private decimal SuggestEntryPrice(Asset asset, TechnicalSignals signals)
        {
            // Méthode simplifiée basée sur le prix actuel et les niveaux de support/résistance
            if (signals.OverallSignal == RecommendationType.Buy || signals.OverallSignal == RecommendationType.StrongBuy)
            {
                // Pour un achat, suggérer un prix légèrement au-dessus du support
                return signals.SupportLevel * 1.02m;
            }
            else if (signals.OverallSignal == RecommendationType.Sell || signals.OverallSignal == RecommendationType.StrongSell)
            {
                // Pour une vente, suggérer un prix légèrement en-dessous de la résistance
                return signals.ResistanceLevel * 0.98m;
            }
            
            return asset.CurrentPrice; // Prix actuel par défaut
        }

        /// <summary>
        /// Suggère un prix de sortie
        /// </summary>
        private decimal SuggestExitPrice(Asset asset, decimal entryPrice, decimal potentialReturn)
        {
            // Méthode simplifiée basée sur le retour potentiel
            return entryPrice * (1 + potentialReturn / 100);
        }

        #endregion
    }
}
