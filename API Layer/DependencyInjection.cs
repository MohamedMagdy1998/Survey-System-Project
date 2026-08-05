using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Reflection;

namespace API_Layer;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection  services)
    {
        services.AddMapsterConfigurations();
        services.AddFluentValidationConfigurations();
        services.AddSwaggerServices();
        services.AddResponseCompressionConfigurations();

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

}
