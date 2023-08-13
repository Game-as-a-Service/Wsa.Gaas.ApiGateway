using FluentAssertions;
using LobbyPlatformApiGatewayTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wsa.Gaas.ApiGatewayTests.ReverseProxyTests
{

    internal class LobbyApiTests : ReverseProxyServer
    {

        [Test]
        public async Task UsersMe_WithNothing_Return401()
        {
            var response = await Client.GetAsync("/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
