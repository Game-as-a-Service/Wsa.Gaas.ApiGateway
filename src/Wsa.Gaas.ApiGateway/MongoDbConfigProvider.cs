using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

internal class MongoDbConfig : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; set; }

    public IReadOnlyList<ClusterConfig> Clusters { get; set; }

    public IChangeToken ChangeToken { get; set; }

    private readonly CancellationTokenSource _cts = new();

    public MongoDbConfig()
    {
        Routes = new List<RouteConfig>();
        Clusters = new List<ClusterConfig>();
        ChangeToken = new CancellationChangeToken(_cts.Token);
    }
}

internal class MongoDbConfigProvider : IProxyConfigProvider
{
    public IProxyConfig GetConfig()
    {
        return new MongoDbConfig();
    }
}