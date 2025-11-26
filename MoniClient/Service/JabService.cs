using System.IO;
using Grpc.Net.Client;
using Jab;
using Microsoft.Extensions.Configuration;

namespace MoniClient.Service;

[ServiceProvider]
[Import<IUtilitiesModule>]
[Singleton<MainWindowViewModel>]
[Singleton(typeof(IConfiguration), Factory = nameof(BuildConfig))]
[Singleton(typeof(GrpcChannel), Factory = nameof(BuildChannel))]
public partial class JabService
{
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

    private static GrpcChannel BuildChannel(IConfiguration config) =>
        GrpcChannel.ForAddress(config["MagicOnion:ServerUrl"] ?? "http://localhost:5000");
}
