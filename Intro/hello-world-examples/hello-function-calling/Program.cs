using Microsoft.Extensions.Hosting;
using Serilog;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;

try
{
	var host = Host
		.CreateDefaultBuilder(args)
        .UseConsoleLifetime()
		.UseSerilog((ctx, logger) => logger.ReadFrom.Configuration(ctx.Configuration))
		.ConfigureServices((ctx, services) =>
		{
			services.AddSingleton(new OpenAIClient(ctx.Configuration["OPENAI_API_KEY"]));
            services.AddHostedService<ConsoleWorker>();
		})       
		.Build();

    await host.RunAsync();
}
catch (Exception ex)
{
	Console.Error.WriteLine("An exception occured: {0}", ex);
	Log.Fatal(ex, "A fatal exception caused the application to crash");
}
finally
{
	await Log.CloseAndFlushAsync();
}
