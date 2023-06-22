using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Example.App.Configurations;
using Example.App.Services.AzureStorageAccountServices;
using System.Reflection;

const string RESOURCE_NAME = "filelist.txt";

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        var stage = Environment.GetEnvironmentVariable("NETCORE_STAGE");
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{stage}.json", optional: true);
    })
    .ConfigureServices(services =>
    {
        services.AddOptions<AzureStorageAccountConfiguration>()
            .Configure<IConfiguration>((option, configuration) =>
            {
                configuration.GetSection(AzureStorageAccountConfiguration.Name).Bind(option);
            });

        services.AddTransient<BlobService>();
    })
    .Build();

await ExecuteServiceAsync(host.Services);


await host.RunAsync();

static async Task ExecuteServiceAsync(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var currentAssembly = Assembly.GetExecutingAssembly();
    var resourceName = currentAssembly
        .GetManifestResourceNames()
        .FirstOrDefault(name => name.EndsWith(RESOURCE_NAME));

    List<string> uris = new();

    if (!string.IsNullOrWhiteSpace(resourceName))
    {
        using var stream = currentAssembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new Exception("There is no filelist.txt as embedded resource");
        }

        using var reader = new StreamReader(stream);
        string? line;
        do
        {
            line = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                uris.Add(line);
            }
        }
        while (!string.IsNullOrWhiteSpace(line));

        reader.Close();
        stream.Close();
    }

    foreach (var (uri, index) in uris.Select((x, index) => (x, index)))
    {
        var uriActual = uri;

        var i = index;

        // for (var i = 0; i < 100; i++)
        // { }

        var blobService = provider.GetRequiredService<BlobService>();

        var uriWithSasToken = await blobService.GenerateReadAccessUrlAsync(uriActual, cancellationToken: CancellationToken.None);

        Console.WriteLine("{0}:{1}\tUri: {2}", i + 1, blobService.GetHashCode(), uriWithSasToken);
    }
}