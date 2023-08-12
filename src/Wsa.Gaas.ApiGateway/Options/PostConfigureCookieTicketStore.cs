using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Net;

namespace Wsa.Gaas.ApiGateway.Options;

public class PostConfigureCookieTicketStore :
    IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _store;
    private readonly IConfiguration _configuration;

    public PostConfigureCookieTicketStore(ITicketStore store, IConfiguration configuration)
    {
        _store = store;
        _configuration = configuration;
    }

    public void Configure(CookieAuthenticationOptions opt)
    {
        _configuration.Bind(nameof(CookieAuthenticationOptions), opt);
        opt.SessionStore = _store;
    }

    public void Configure(string? name, CookieAuthenticationOptions opt)
    {
        Configure(opt);
    }


}
