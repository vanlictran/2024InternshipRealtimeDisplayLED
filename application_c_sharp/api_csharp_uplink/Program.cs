namespace api_csharp_uplink;

public static class Program
{
    public static async Task Main(string[] args)
    {
        IHostBuilder builder =  Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((_, config) =>
        {
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string jsonToRead = environment != "Test" ? "appsettings.json" : "appsettings.Test.json";
            config.AddJsonFile(jsonToRead, optional: false, reloadOnChange: true);
            config.AddCommandLine(args);
        }).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
        
        IHost host = builder.Build();
        await host.RunAsync();
    }
}