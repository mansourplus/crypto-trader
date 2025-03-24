using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using CryptoTrader.Infrastructure.Data;
using CryptoTrader.Core.Interfaces;
using CryptoTrader.Infrastructure.Repositories;
using CryptoTrader.Infrastructure.External;
using CryptoTrader.Application.Services;
using CryptoTrader.Application.Mappings;
using FluentValidation;
using CryptoTrader.Application.Validators;
using CryptoTrader.Application.DTOs;
using RecommendationService = CryptoTrader.Infrastructure.External.RecommendationService;

namespace CryptoTrader.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Cette méthode est appelée par le runtime pour ajouter des services au conteneur.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configuration de la base de données
            services.AddDbContext<CryptoTraderDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // Configuration de l'authentification JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });

            // Enregistrement des repositories
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IStrategyRepository, StrategyRepository>();

            // Enregistrement des services externes
            services.AddHttpClient<ICoinbaseService, CoinbaseService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["CoinbaseApi:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddScoped<IRecommendationService, RecommendationService>();

            // Enregistrement des services d'application
            services.AddScoped<AssetService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<StrategyService>();
            services.AddScoped<Application.Services.RecommendationService>();

            // Configuration d'AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Enregistrement des validateurs
            services.AddScoped<IValidator<CreateAssetDto>, CreateAssetDtoValidator>();
            services.AddScoped<IValidator<UpdateAssetDto>, UpdateAssetDtoValidator>();
            services.AddScoped<IValidator<CreateTransactionDto>, CreateTransactionDtoValidator>();
            services.AddScoped<IValidator<UpdateTransactionDto>, UpdateTransactionDtoValidator>();
            services.AddScoped<IValidator<CreateStrategyDto>, CreateStrategyDtoValidator>();
            services.AddScoped<IValidator<UpdateStrategyDto>, UpdateStrategyDtoValidator>();

            // Configuration de CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", builder =>
                {
                    builder.WithOrigins(Configuration["CorsOrigins:AngularApp"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // Configuration des contrôleurs
            services.AddControllers();

            // Configuration de Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CryptoTrader API",
                    Version = "v1",
                    Description = "API pour l'application de trading de cryptomonnaies",
                    Contact = new OpenApiContact
                    {
                        Name = "CryptoTrader Team",
                        Email = "contact@cryptotrader.com",
                        Url = new Uri("https://cryptotrader.com")
                    }
                });

                // Configuration de l'authentification dans Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Configuration de SignalR pour les communications en temps réel
            services.AddSignalR();

            // Configuration du cache
            services.AddMemoryCache();
        }

        // Cette méthode est appelée par le runtime pour configurer le pipeline de requêtes HTTP.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoTrader API v1"));
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAngularApp");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CryptoTrader.API.Hubs.TradingHub>("/tradingHub");
            });
        }
    }
}
