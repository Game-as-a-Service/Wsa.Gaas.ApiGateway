using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Wsa.Gaas.ApiGateway;
using Wsa.Gaas.ApiGateway.Options;
using Wsa.Gaas.ApiGateway.Services;
using Wsa.Gaas.ApiGateway.TransformProviders;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((cxt, cfg) => cfg.ReadFrom.Configuration(cxt.Configuration));

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<BffTransformer>()
    .Services
    .AddSingleton<IProxyConfigProvider, MongoDbConfigProvider>()
    .AddScoped<LobbyApiService>()
    .AddCors()
    .AddDataProtection()
    .Services
    .AddControllers()

    // Cookie Options
    .Services
    .AddMemoryCache()
    .AddSingleton<ITicketStore, DistributedCacheTicketStore>()
    .ConfigureOptions<PostConfigureCookieTicketStore>()

    // LobbyApi Options
    .Configure<LobbyApiOptions>(opt => builder.Configuration.Bind(nameof(LobbyApiOptions), opt))
    .Configure<Auth0Options>(opt => builder.Configuration.Bind(nameof(Auth0Options), opt))

    // Open Id Options
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect("auth0", opt =>
    {
        builder.Configuration.Bind(nameof(OpenIdConnectOptions), opt);
        
        opt.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = cxt =>
            {
                cxt.ProtocolMessage.SetParameter("connection", cxt.Properties.GetString("connection"));

                cxt.ProtocolMessage.SetParameter("audience", "https://api.gaas.waterballsa.tw");

                return Task.CompletedTask;
            }
        };
    })
    .Services
    .Configure<ForwardedHeadersOptions>(opt =>
    {
        opt.ForwardedHeaders = ForwardedHeaders.All;
        opt.KnownNetworks.Clear();
        opt.KnownProxies.Clear();
    })
    ;

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors(opt =>
{
    opt.AllowAnyHeader()
        .AllowCredentials()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true);
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapReverseProxy();

app.Run();
