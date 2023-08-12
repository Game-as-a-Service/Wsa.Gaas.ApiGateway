using LobbyPlatform.ApiGateway.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LobbyPlatform.ApiGateway.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet("~/login")]
        [HttpGet("~/auth/login")]
        public async Task<IActionResult> Login(
            [FromServices] IOptions<FrontendOptions> options,
            string? type = null,
            string? returnUrl = null
        )
        {
            var _options = options.Value;

            type ??= _options.Providers.Contains(type)
                ? type
                : _options.Providers.First();

            returnUrl ??= _options.Url;

            if (User.Identity?.IsAuthenticated is true)
            {
                var token = await HttpContext.GetTokenAsync("access_token");

                return Redirect(
                    string.Format(returnUrl, token)
                );
            }

            var props = new AuthenticationProperties();
            
            props.SetString("connection", type);

            return Challenge(props, "auth0");
        }
    }
}
