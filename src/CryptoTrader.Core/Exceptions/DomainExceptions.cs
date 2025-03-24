using System;

namespace CryptoTrader.Core.Exceptions
{
    /// <summary>
    /// Exception de base pour toutes les exceptions du domaine
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException() : base() { }
        
        public DomainException(string message) : base(message) { }
        
        public DomainException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
    
    /// <summary>
    /// Exception levée lorsqu'une entité n'est pas trouvée
    /// </summary>
    public class EntityNotFoundException : DomainException
    {
        public string EntityName { get; }
        public object EntityId { get; }
        
        public EntityNotFoundException(string entityName, object entityId)
            : base($"L'entité {entityName} avec l'identifiant {entityId} n'a pas été trouvée.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
    
    /// <summary>
    /// Exception levée lorsqu'une opération de trading n'est pas valide
    /// </summary>
    public class InvalidTradeOperationException : DomainException
    {
        public InvalidTradeOperationException(string message) : base(message) { }
    }
    
    /// <summary>
    /// Exception levée lorsque le solde est insuffisant pour une opération
    /// </summary>
    public class InsufficientBalanceException : DomainException
    {
        public string UserId { get; }
        public string Symbol { get; }
        public decimal RequiredAmount { get; }
        public decimal AvailableAmount { get; }
        
        public InsufficientBalanceException(string userId, string symbol, decimal requiredAmount, decimal availableAmount)
            : base($"Solde insuffisant pour l'utilisateur {userId}. Requis: {requiredAmount} {symbol}, Disponible: {availableAmount} {symbol}")
        {
            UserId = userId;
            Symbol = symbol;
            RequiredAmount = requiredAmount;
            AvailableAmount = availableAmount;
        }
    }
    
    /// <summary>
    /// Exception levée lorsqu'une stratégie n'est pas valide
    /// </summary>
    public class InvalidStrategyException : DomainException
    {
        public Guid StrategyId { get; }
        
        public InvalidStrategyException(Guid strategyId, string message)
            : base($"Stratégie invalide (ID: {strategyId}): {message}")
        {
            StrategyId = strategyId;
        }
    }
    
    /// <summary>
    /// Exception levée lors d'erreurs de communication avec l'API Coinbase
    /// </summary>
    public class CoinbaseApiException : DomainException
    {
        public string ErrorCode { get; }
        
        public CoinbaseApiException(string message, string errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }
        
        public CoinbaseApiException(string message, Exception innerException, string errorCode = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
