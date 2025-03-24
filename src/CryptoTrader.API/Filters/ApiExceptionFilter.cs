using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.API.Filters
{
    /// <summary>
    /// Filtre d'exception global pour l'API
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            HandleException(context);
            return base.OnExceptionAsync(context);
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            var exception = context.Exception;
            _logger.LogError(exception, "Une exception non gérée s'est produite: {Message}", exception.Message);

            var statusCode = 500;
            var message = "Une erreur interne s'est produite. Veuillez réessayer ultérieurement.";

            // Personnaliser la réponse en fonction du type d'exception
            if (exception is ArgumentException || exception is FormatException || exception is InvalidOperationException)
            {
                statusCode = 400;
                message = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = 403;
                message = "Vous n'êtes pas autorisé à effectuer cette action.";
            }
            else if (exception is TimeoutException)
            {
                statusCode = 504;
                message = "La requête a expiré. Veuillez réessayer ultérieurement.";
            }

            // Créer une réponse d'erreur structurée
            var result = new ObjectResult(new
            {
                error = new
                {
                    message,
                    timestamp = DateTime.UtcNow,
                    requestId = context.HttpContext.TraceIdentifier
                }
            })
            {
                StatusCode = statusCode
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
