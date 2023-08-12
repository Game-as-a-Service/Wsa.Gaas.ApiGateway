using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace LobbyPlatform.ApiGateway
{
    public class BffTransformer : ITransformProvider
    {
        private readonly ILogger<BffTransformer> _logger;

        public BffTransformer(ILogger<BffTransformer> logger)
        {
            _logger = logger;
        }

        public void Apply(TransformBuilderContext context)
        {
            context.AddRequestTransform(ExtractJwtToken);
        }

        private async ValueTask ExtractJwtToken(RequestTransformContext cxt)
        {
            if (await cxt.HttpContext.GetTokenAsync("access_token") is string jwt)
            {
                _logger.LogInformation("Forward with JWT {jwt}", jwt);

                cxt.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    jwt
                );
            }
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
        }

        public void ValidateRoute(TransformRouteValidationContext context)
        {
        }
    }
}
