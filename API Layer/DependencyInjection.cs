using Application.Options;
using Application.Services_Implementations;
using Application.Services_Interfaces;
using Domain.Common.Interfaces;
using Domain.Contracts;
using Domain.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure_Layer;
using Infrastructure_Layer.Implementations;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Reflection;

namespace API_Layer;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection  services,IConfiguration configuration)
    {
        #region Configurations

        services.AddMapsterConfigurations();
        services.AddFluentValidationConfigurations();
        services.AddSwaggerServices();
        services.AddResponseCompressionConfigurations();
        services.AddEntityFrameworkConfiguration(configuration);
        services.AddAuthenticationConfigurations(configuration);
        services.AddOptionsPatternConfigurations(configuration);
        #endregion

        #region Services Registeration
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPollService, PollService>();
        #endregion


        return services;
    }


    #region Configuration Methods

    private static IServiceCollection AddAuthenticationConfigurations(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
             .AddEntityFrameworkStores<ApplicationDbContext>();


        var JwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtSettings!.Issuer,
                    ValidAudience = JwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtSettings.Key)) 
                };
            });


        return services;

    }
    private static IServiceCollection AddOptionsPatternConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
    private static IServiceCollection AddEntityFrameworkConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ??

            throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
        });


        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    private static IServiceCollection AddMapsterConfigurations(this IServiceCollection services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return services;
    }

    private static IServiceCollection AddFluentValidationConfigurations(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }

    private static IServiceCollection AddResponseCompressionConfigurations(this IServiceCollection services)
    {

        services.AddResponseCompression(options =>
        {

            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();

            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
           new[] { "application/json", "text/plain", "image/svg+xml" });

            options.EnableForHttps = false;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {

            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
        return services;
    }

    #endregion


}
