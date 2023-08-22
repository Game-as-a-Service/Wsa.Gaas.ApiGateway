using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Wsa.Gaas.ApiGateway.Options;
using Wsa.Gaas.ApiGateway.Services;

namespace Wsa.Gaas.ApiGateway.Controllers;

public class AuthController : Controller
{
    [HttpGet("~/login")]
    [HttpGet("~/auth/login")]
    public IActionResult Login(
        [FromServices] IOptions<Auth0Options> auth0Options,
        [FromServices] IOptions<LobbyApiOptions> lobbyApiOptions,
        string? type = null,
        string? returnUrl = null
    )
    {
        var _options = auth0Options.Value;

        type ??= _options.Providers.Contains(type)
            ? type
            : _options.Providers.First();

        var props = new AuthenticationProperties();

        props.SetString("connection", type);
        props.RedirectUri = Url.Action(nameof(Success));
        
        return Challenge(props, "auth0");
    }

    [HttpGet("~/auth/success")]
    public async Task<IActionResult> Success(
        [FromServices] IOptions<LobbyApiOptions> lobbyApiOptions,
        [FromServices] LobbyApiService lobbyApiService
    )
    {

        if (User.Identity?.IsAuthenticated is true)
        {
            var returnUrl = lobbyApiOptions.Value.FrontEndUrl;

            var token = await HttpContext.GetTokenAsync("access_token");

            if (token is string && User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value is string email)
            {
                await lobbyApiService.CreateUser(token, email);
            }

            return Redirect(
                string.Format(returnUrl, token)
            );
        }

        return RedirectToAction(nameof(Login));
    }
}
