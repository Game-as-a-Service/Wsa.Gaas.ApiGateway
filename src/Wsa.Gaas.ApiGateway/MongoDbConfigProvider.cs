using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;
using Wsa.Gaas.ApiGateway.Options;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;

namespace Wsa.Gaas.ApiGateway;

internal class MongoDbConfig : IProxyConfig
{
    public IReadOnlyList<RouteConfig> Routes { get; }

    public IReadOnlyList<ClusterConfig> Clusters { get; }

    public IChangeToken ChangeToken { get; } = NullChangeToken.Singleton;

    public MongoDbConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters
    )
    {
        Routes = routes;
        Clusters = clusters;
    }


}

internal class MongoDbConfigProvider : IProxyConfigProvider
{
    private readonly MongoClient _client;

    public MongoDbConfigProvider(IOptions<MongoDbOptions> options)
    {
        _client = new MongoClient(options.Value.Url);
        
    }

    public IProxyConfig GetConfig()
    {
        var database = _client.GetDatabase("gaas-lobby");

        var collection = database.GetCollection<BsonDocument>("gameRegistrationData");

        var documents = collection.Find(new BsonDocument()).ToList();

        var routes = new List<RouteConfig>();
        var clusters = new List<ClusterConfig>();

        foreach (var doc in documents)
        {
            var uniqueName = doc["uniqueName"].AsString;
            var backEndUrl = doc["backEndUrl"].AsString;
            var health = doc.Contains("health") ? doc["health"].AsString : null;
            
            var backendClusterId = $"{uniqueName}-backend-cluster";
            var backendRouteId = $"{uniqueName}-backend-route";

            routes.Add(new RouteConfig()
            {
                RouteId = backendRouteId,
                AuthorizationPolicy = "Anonymous",
                ClusterId = backendClusterId,
                Match = new RouteMatch()
                {
                    Path = $"{uniqueName}/{{**catch-all}}"
                },
            });

            clusters.Add(new ClusterConfig()
            {
                ClusterId = backendClusterId,
                HealthCheck = string.IsNullOrEmpty(health) 
                    ? null 
                    : new HealthCheckConfig()
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = true,
                            Interval = TimeSpan.FromSeconds(30),
                            Timeout = TimeSpan.FromSeconds(30),
                            Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
                            Path = health,
                        },
                    },
                Destinations = new Dictionary<string, DestinationConfig>()
                {
                    { 
                        $"{uniqueName}-backend", 
                        new DestinationConfig() 
                        {  
                            Address = backEndUrl,
                        }
                    }
                },
            });

            var frontEndUrl = doc["frontEndUrl"].AsString;
            var frontendClusterId = $"{uniqueName}-frontend-cluster";
            var frontendRouteId = $"{uniqueName}-frontend-route";

            routes.Add(new RouteConfig()
            {
                RouteId = frontendRouteId,
                AuthorizationPolicy = "Anonymous",
                ClusterId = frontendClusterId,
                Match = new RouteMatch()
                {
                    Path = uniqueName,
                },
            });

            clusters.Add(new ClusterConfig()
            {
                ClusterId = frontendClusterId,
                Destinations = new Dictionary<string, DestinationConfig>()
                {
                    {
                        $"{uniqueName}-frontend",
                        new DestinationConfig()
                        {
                            Address = frontEndUrl,
                        }
                    }
                },
            });
        }

        return new MongoDbConfig(
            routes,
            clusters
        );
    }
}