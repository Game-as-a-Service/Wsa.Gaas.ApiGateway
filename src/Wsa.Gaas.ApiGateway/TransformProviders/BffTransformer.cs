using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Wsa.Gaas.ApiGateway.TransformProviders;

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
        // Request 沒帶 JWT 就從 Cookie 解析
        if (StringValues.IsNullOrEmpty(cxt.HttpContext.Request.Headers.Authorization)
            && await cxt.HttpContext.GetTokenAsync("access_token") is string jwt
        )
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
