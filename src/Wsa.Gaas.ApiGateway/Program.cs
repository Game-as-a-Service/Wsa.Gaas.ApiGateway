using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Wsa.Gaas.ApiGateway;
using Wsa.Gaas.ApiGateway.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((cxt, cfg) => cfg.ReadFrom.Configuration(cxt.Configuration));

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<BffTransformer>()
    .Services
    .AddCors()
    .AddDataProtection()
    .Services
    .AddControllers()

    // Cookie Options
    .Services
    .AddMemoryCache()
    .AddSingleton<ITicketStore, DistributedCacheTicketStore>()
    .ConfigureOptions<PostConfigureCookieTicketStore>()

    // Frontend Options
    .Configure<FrontendOptions>(opt => builder.Configuration.Bind(nameof(FrontendOptions), opt))

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
