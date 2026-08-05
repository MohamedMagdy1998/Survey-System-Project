
namespace API_Layer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
       
        builder.Services.AddDependencies();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

      
        app.UseHsts();

        app.UseHttpsRedirection();
    
        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
