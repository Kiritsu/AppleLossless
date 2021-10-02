using AppleLossless;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

ConsoleHelper.CleanWriteHeader();

var builder = Host.CreateDefaultBuilder(args);

builder.UseSerilog((ctx, logger) =>
{
    if (ctx.HostingEnvironment.IsDevelopment())
    {
        logger.MinimumLevel.Verbose();
    }
    else
    {
        logger.MinimumLevel.Information();
        logger.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    }

    logger.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code);
});

builder.ConfigureServices((ctx, services) =>
{
    services.Configure<AppleLosslessOptions>(ctx.Configuration.GetSection("A_LOSSLESS"));
    services.AddHostedService<ConverterService>();
});

builder.UseDefaultServiceProvider(x => x.ValidateOnBuild = true);

builder.Build().Run();