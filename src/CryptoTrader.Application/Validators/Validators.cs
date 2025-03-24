using System;
using FluentValidation;
using CryptoTrader.Application.DTOs;

namespace CryptoTrader.Application.Validators
{
    /// <summary>
    /// Validateur pour la création d'un actif
    /// </summary>
    public class CreateAssetDtoValidator : AbstractValidator<CreateAssetDto>
    {
        public CreateAssetDtoValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Le symbole est requis")
                .MaximumLength(10).WithMessage("Le symbole ne peut pas dépasser 10 caractères")
                .Matches("^[A-Za-z0-9]+$").WithMessage("Le symbole ne peut contenir que des lettres et des chiffres");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom est requis")
                .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères");

            RuleFor(x => x.CurrentPrice)
                .GreaterThan(0).WithMessage("Le prix doit être supérieur à 0");
        }
    }

    /// <summary>
    /// Validateur pour la mise à jour d'un actif
    /// </summary>
    public class UpdateAssetDtoValidator : AbstractValidator<UpdateAssetDto>
    {
        public UpdateAssetDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("L'identifiant est requis");

            RuleFor(x => x.CurrentPrice)
                .GreaterThan(0).WithMessage("Le prix doit être supérieur à 0");

            RuleFor(x => x.MarketCap)
                .GreaterThanOrEqualTo(0).WithMessage("La capitalisation boursière doit être positive ou nulle");

            RuleFor(x => x.Volume24h)
                .GreaterThanOrEqualTo(0).WithMessage("Le volume sur 24h doit être positif ou nul");

            RuleFor(x => x.CirculatingSupply)
                .GreaterThanOrEqualTo(0).WithMessage("L'offre en circulation doit être positive ou nulle");

            RuleFor(x => x.MaxSupply)
                .GreaterThanOrEqualTo(0).When(x => x.MaxSupply.HasValue).WithMessage("L'offre maximale doit être positive ou nulle");
        }
    }

    /// <summary>
    /// Validateur pour la création d'une transaction
    /// </summary>
    public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("L'identifiant de l'utilisateur est requis");

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Le symbole de l'actif est requis")
                .MaximumLength(10).WithMessage("Le symbole ne peut pas dépasser 10 caractères");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Le type de transaction est requis")
                .Must(type => type.ToUpper() == "BUY" || type.ToUpper() == "SELL")
                .WithMessage("Le type de transaction doit être 'Buy' ou 'Sell'");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");

            RuleFor(x => x.LimitPrice)
                .GreaterThan(0).When(x => x.LimitPrice.HasValue).WithMessage("Le prix limite doit être supérieur à 0");
        }
    }

    /// <summary>
    /// Validateur pour la mise à jour d'une transaction
    /// </summary>
    public class UpdateTransactionDtoValidator : AbstractValidator<UpdateTransactionDto>
    {
        public UpdateTransactionDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("L'identifiant est requis");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Le statut est requis")
                .Must(status => Enum.TryParse<Core.Entities.TransactionStatus>(status, true, out _))
                .WithMessage("Le statut n'est pas valide");
        }
    }

    /// <summary>
    /// Validateur pour la création d'une stratégie
    /// </summary>
    public class CreateStrategyDtoValidator : AbstractValidator<CreateStrategyDto>
    {
        public CreateStrategyDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom est requis")
                .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("L'identifiant de l'utilisateur est requis");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Le type de stratégie est requis")
                .Must(type => Enum.TryParse<Core.Entities.StrategyType>(type, true, out _))
                .WithMessage("Le type de stratégie n'est pas valide");

            RuleFor(x => x.AssetSymbols)
                .NotEmpty().WithMessage("Au moins un actif doit être spécifié");

            RuleFor(x => x.MaxInvestmentAmount)
                .GreaterThan(0).When(x => x.MaxInvestmentAmount.HasValue)
                .WithMessage("Le montant maximum d'investissement doit être supérieur à 0");

            RuleFor(x => x.TakeProfitPercentage)
                .GreaterThan(0).When(x => x.TakeProfitPercentage.HasValue)
                .WithMessage("Le pourcentage de take-profit doit être supérieur à 0");

            RuleFor(x => x.StopLossPercentage)
                .GreaterThan(0).When(x => x.StopLossPercentage.HasValue)
                .WithMessage("Le pourcentage de stop-loss doit être supérieur à 0");

            RuleFor(x => x.ExecutionFrequencyMinutes)
                .GreaterThan(0).When(x => x.ExecutionFrequencyMinutes.HasValue)
                .WithMessage("La fréquence d'exécution doit être supérieure à 0");
        }
    }

    /// <summary>
    /// Validateur pour la mise à jour d'une stratégie
    /// </summary>
    public class UpdateStrategyDtoValidator : AbstractValidator<UpdateStrategyDto>
    {
        public UpdateStrategyDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("L'identifiant est requis");

            RuleFor(x => x.Name)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Le nom ne peut pas dépasser 100 caractères");

            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || Enum.TryParse<Core.Entities.StrategyStatus>(status, true, out _))
                .WithMessage("Le statut n'est pas valide");

            RuleFor(x => x.MaxInvestmentAmount)
                .GreaterThan(0).When(x => x.MaxInvestmentAmount.HasValue)
                .WithMessage("Le montant maximum d'investissement doit être supérieur à 0");

            RuleFor(x => x.TakeProfitPercentage)
                .GreaterThan(0).When(x => x.TakeProfitPercentage.HasValue)
                .WithMessage("Le pourcentage de take-profit doit être supérieur à 0");

            RuleFor(x => x.StopLossPercentage)
                .GreaterThan(0).When(x => x.StopLossPercentage.HasValue)
                .WithMessage("Le pourcentage de stop-loss doit être supérieur à 0");

            RuleFor(x => x.ExecutionFrequencyMinutes)
                .GreaterThan(0).When(x => x.ExecutionFrequencyMinutes.HasValue)
                .WithMessage("La fréquence d'exécution doit être supérieure à 0");
        }
    }
}
