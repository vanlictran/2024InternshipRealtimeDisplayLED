using api_csharp_uplink.Connectors;
using api_csharp_uplink.Settings;

namespace api_csharp_uplink;

using System.Reflection;
using Composant;
using Repository;
using DB;
using Interface;
using Microsoft.OpenApi.Models;
using api_csharp_uplink.Repository.Interface;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IScheduleRegistration, ScheduleComposant>();
        services.AddScoped<ScheduleComposant>();
        services.AddScoped<IScheduleFinder, ScheduleComposant>();
        services.AddScoped<ICardFinder, CardComposant>();
        services.AddScoped<ICardRegistration, CardComposant>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IPositionRegister, PositionComposant>();
        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<IStationRegister, StationComposant>();
        services.AddScoped<IStationFinder, StationComposant>();
        services.AddScoped<IItineraryRepository, ItineraryRepository>();
        services.AddScoped<IItineraryRegister, ItineraryComposant>();
        services.AddScoped<IItineraryFinder, ItineraryComposant>();
        services.AddScoped<IConnexionRepository, ItineraryRepository>();
        services.AddScoped<IConnexionFinder, ConnexionComposant>();
        services.AddSingleton<IGraphHelper, GraphHelperService>();
        services.AddScoped<IPositionProcessor, TimeComposant>();
        services.AddScoped<ITimeProcessor, TimeComposant>();
        services.AddSingleton<IGlobalInfluxDb, GlobalInfluxDb>();
        services.AddSingleton<IGraphPosition, GraphComposant>();
        services.AddSingleton<IGraphItinerary>(sp => sp.GetRequiredService<IGraphPosition>() as GraphComposant ?? 
                                                     throw new InvalidOperationException());
        services.Configure<InfluxDbSettings>(configuration.GetSection("InfluxDB"));
        services.Configure<GraphHopperSettings>(configuration.GetSection("GraphHopper"));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        });
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}