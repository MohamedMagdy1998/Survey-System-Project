using API_Layer.Filters;
using API_Layer.Middleware;
using Application.Options;
using Application.Services_Implementations;
using Application.Services_Interfaces;
using Application.Settings;
using Domain;
using Domain.Common.Interfaces;
using Domain.Contracts;
using Domain.Contracts.Repositories;
using Domain.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Infrastructure_Layer;
using Infrastructure_Layer.Implementations;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        services.AddDistributedMemoryCache();
        services.AddMapsterConfigurations();
        services.AddFluentValidationConfigurations();
        services.AddSwaggerServices();
        services.AddResponseCompressionConfigurations();
        services.AddEntityFrameworkConfiguration(configuration);
        services.AddAuthenticationConfigurations(configuration);
        services.AddOptionsPatternConfigurations(configuration);
        services.AddCorsConfigurations(configuration);
        #endregion

        #region Services Registeration
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IPollRepository, PollRepository>();
        services.AddScoped<IQuestionRespository, QuestionRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IRoleRepository,RoleRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddBackgroundJobsConfig(configuration);
        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
        services.AddHttpContextAccessor();

        #endregion


        return services;
    }


    #region Configuration Methods

    private static IServiceCollection AddAuthenticationConfigurations(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>()
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();


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

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;

            //options.SignIn.RequireConfirmedEmail = true;
            
            options.User.RequireUniqueEmail = true;
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

    private static IServiceCollection AddCorsConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
            options.AddDefaultPolicy(builder =>
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>()!)
            )
        );
        return services;
    }

    private static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        services.AddHangfireServer();

        return services;
    }

    #endregion


}
