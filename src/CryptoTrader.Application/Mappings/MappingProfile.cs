using System;
using AutoMapper;
using CryptoTrader.Core.Entities;
using CryptoTrader.Application.DTOs;
using System.Linq;

namespace CryptoTrader.Application.Mappings
{
    /// <summary>
    /// Profil de mapping AutoMapper pour les entités et DTOs
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Asset mappings
            CreateMap<Asset, AssetDto>();
            CreateMap<CreateAssetDto, Asset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateAssetDto, Asset>()
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            
            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<TransactionType>(src.Type)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.Pending))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Asset, opt => opt.Ignore())
                .ForMember(dest => dest.AssetId, opt => opt.Ignore());
            
            CreateMap<UpdateTransactionDto, Transaction>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TransactionStatus>(src.Status)))
                //.ForAllOtherMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                ;

            // Strategy mappings
            CreateMap<Strategy, StrategyDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Assets, opt => opt.Ignore());
            
            CreateMap<CreateStrategyDto, Strategy>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<StrategyType>(src.Type)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StrategyStatus.Draft))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.AssetIds, opt => opt.Ignore());
            
            CreateMap<UpdateStrategyDto, Strategy>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<StrategyStatus>(src.Status)))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.AssetIds, opt => opt.Ignore())
                //.ForAllOtherMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                ;

            // Recommendation mappings
            CreateMap<Core.Interfaces.AssetRecommendation, Application.DTOs.RecommendationDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.SuggestedHoldingPeriodDays, opt => opt.MapFrom(src => src.SuggestedHoldingPeriod.TotalDays));
            
            CreateMap<Core.Interfaces.TimingRecommendation, Application.DTOs.TimingRecommendationDto>();
            
            CreateMap<Core.Interfaces.TechnicalSignals, Application.DTOs.TechnicalSignalsDto>()
                .ForMember(dest => dest.OverallSignal, opt => opt.MapFrom(src => src.OverallSignal.ToString()));
        }
    }
}
