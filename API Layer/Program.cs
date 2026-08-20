using Serilog;  
namespace API_Layer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
            
        // Add services to the container.

        builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

        builder.Services.AddControllers();
       
        builder.Services.AddDependencies(builder.Configuration);

        

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
      
        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseCors();
    
        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseSerilogRequestLogging();

        app.MapControllers();

        app.Run();
    }
}
