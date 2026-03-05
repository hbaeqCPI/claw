using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using R10.Web.Services;
using Serilog;

namespace R10.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(Configuration)
                            .CreateLogger();

                Log.Information("Starting up");

                var builder = WebApplication.CreateBuilder(args);
                var env = builder.Environment;

                builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));


                var configuration = builder.Configuration;

                // Manually create an instance of the Startup class
                var startup = new Startup(builder.Configuration, env);

                // Manually call ConfigureServices()
                startup.ConfigureServices(builder.Services);

                var app = builder.Build();

                // Support path base for IIS virtual directory hosting (e.g. /r10v2)
                var pathBase = configuration["PathBase"];
                if (!string.IsNullOrEmpty(pathBase))
                {
                    app.UsePathBase(pathBase);
                }

                // Call Configure(), passing in the dependencies
                startup.Configure(app, env);

                app.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FATAL STARTUP ERROR: " + ex);
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.Information("Shut down complete");
                Log.CloseAndFlush();
            }
        }

        public static IConfiguration Configuration {
            get {
                var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);

                string? keyVaultEndpoint = Environment.GetEnvironmentVariable("KV_ENDPOINT");
                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {
                    var secretClient = new SecretClient(new(keyVaultEndpoint), new DefaultAzureCredential());
                    builder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                }
                return builder.Build();
            }
        }
    }
}
