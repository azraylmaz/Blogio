using System;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;


namespace Blogio.Blazor;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting web host.");
            var builder = WebApplication.CreateBuilder(args);


            //Blazor
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            //Blazorise
            builder.Services.AddBlazorise(options =>
            {
                options.Immediate = true;
            }).AddBootstrap5Providers()
                .AddFontAwesomeIcons();

            builder.Services.AddBlazoriseRichTextEdit();

            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();

            builder.Services.AddEndpointsApiExplorer();

            await builder.AddApplicationAsync<BlogioBlazorModule>();

            var app = builder.Build();

            await app.InitializeApplicationAsync();
            app.UseSwagger();
            app.UseAbpSwaggerUI(
                c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogio API v1");
                });

            app.MapControllers();

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
