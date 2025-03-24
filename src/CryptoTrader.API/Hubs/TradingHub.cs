using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CryptoTrader.Application.DTOs;

namespace CryptoTrader.API.Hubs
{
    /// <summary>
    /// Hub SignalR pour les communications en temps réel liées au trading
    /// </summary>
    public class TradingHub : Hub
    {
        private readonly ILogger<TradingHub> _logger;

        public TradingHub(ILogger<TradingHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Méthode appelée lorsqu'un client se connecte au hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connecté: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Méthode appelée lorsqu'un client se déconnecte du hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Client déconnecté: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Permet à un client de s'abonner aux mises à jour d'un actif spécifique
        /// </summary>
        public async Task SubscribeToAsset(string symbol)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"asset_{symbol.ToUpper()}");
            _logger.LogInformation("Client {ConnectionId} s'est abonné à l'actif {Symbol}", Context.ConnectionId, symbol);
        }

        /// <summary>
        /// Permet à un client de se désabonner des mises à jour d'un actif spécifique
        /// </summary>
        public async Task UnsubscribeFromAsset(string symbol)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"asset_{symbol.ToUpper()}");
            _logger.LogInformation("Client {ConnectionId} s'est désabonné de l'actif {Symbol}", Context.ConnectionId, symbol);
        }

        /// <summary>
        /// Permet à un client de s'abonner aux mises à jour d'une stratégie spécifique
        /// </summary>
        public async Task SubscribeToStrategy(Guid strategyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"strategy_{strategyId}");
            _logger.LogInformation("Client {ConnectionId} s'est abonné à la stratégie {StrategyId}", Context.ConnectionId, strategyId);
        }

        /// <summary>
        /// Permet à un client de se désabonner des mises à jour d'une stratégie spécifique
        /// </summary>
        public async Task UnsubscribeFromStrategy(Guid strategyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"strategy_{strategyId}");
            _logger.LogInformation("Client {ConnectionId} s'est désabonné de la stratégie {StrategyId}", Context.ConnectionId, strategyId);
        }

        /// <summary>
        /// Permet à un client de s'abonner aux mises à jour du portefeuille d'un utilisateur
        /// </summary>
        public async Task SubscribeToPortfolio(string userId)
        {
            // Vérifier que l'utilisateur est autorisé à s'abonner à ce portefeuille
            var currentUserId = Context.User?.FindFirst("sub")?.Value;
            if (currentUserId == userId || Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"portfolio_{userId}");
                _logger.LogInformation("Client {ConnectionId} s'est abonné au portefeuille de l'utilisateur {UserId}", Context.ConnectionId, userId);
            }
            else
            {
                _logger.LogWarning("Client {ConnectionId} a tenté de s'abonner au portefeuille de l'utilisateur {UserId} sans autorisation", Context.ConnectionId, userId);
            }
        }

        /// <summary>
        /// Permet à un client de se désabonner des mises à jour du portefeuille d'un utilisateur
        /// </summary>
        public async Task UnsubscribeFromPortfolio(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"portfolio_{userId}");
            _logger.LogInformation("Client {ConnectionId} s'est désabonné du portefeuille de l'utilisateur {UserId}", Context.ConnectionId, userId);
        }

        /// <summary>
        /// Permet à un client de s'abonner aux recommandations
        /// </summary>
        public async Task SubscribeToRecommendations()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "recommendations");
            _logger.LogInformation("Client {ConnectionId} s'est abonné aux recommandations", Context.ConnectionId);
        }

        /// <summary>
        /// Permet à un client de se désabonner des recommandations
        /// </summary>
        public async Task UnsubscribeFromRecommendations()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "recommendations");
            _logger.LogInformation("Client {ConnectionId} s'est désabonné des recommandations", Context.ConnectionId);
        }
    }
}
