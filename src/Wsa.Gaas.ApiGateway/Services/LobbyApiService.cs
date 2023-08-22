using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Wsa.Gaas.ApiGateway.Options;

namespace Wsa.Gaas.ApiGateway.Services
{
    public class LobbyApiService
    {
        private readonly LobbyApiOptions _options;

        public LobbyApiService(IOptions<LobbyApiOptions> options)
        {
            _options = options.Value;
        }

        public async Task CreateUser(string jwt, string email)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            client.BaseAddress = new Uri(_options.BackEndUrl);

            var response = await client.PostAsJsonAsync("/users", new
            {
                email,
            });
        }
    }
}
