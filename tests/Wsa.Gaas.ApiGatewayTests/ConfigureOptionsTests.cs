using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Xml.Linq;

namespace LobbyPlatformApiGatewayTests
{
    public class ConfigureOptionsTests
    {
        public class Opt 
        { 
            public string? StringValue { get; set; }
        }

        /// <summary>
        /// Default Configuration for Options
        /// </summary>
        public class ConfigureOpt 
            : IConfigureOptions<Opt>
            , IConfigureNamedOptions<Opt>
        {
            public void Configure(Opt options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string? name, Opt options)
            {
                options.StringValue = GetStringValue(name);
            }

            public static string GetStringValue(string? name)
            {
                return (string.IsNullOrEmpty(name) ? "" : $"{name}.")
                    + nameof(ConfigureOpt);
            }
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ConfigureOptionsTest()
        {
            var services = new ServiceCollection()
                .ConfigureOptions<ConfigureOpt>()
                .Configure<Opt>(opt => 
                { 
                    opt.StringValue = "abc"; 
                })
                ;

            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<Opt>>();

            var expectedStringValue = nameof(ConfigureOpt);

            var actualStringValue = options.Value.StringValue;

            actualStringValue.Should().Be(expectedStringValue);
        }

        [Test]
        [TestCase("")]
        [TestCase(nameof(ConfigureNamedOptionsTest))]
        public void ConfigureNamedOptionsTest(string name)
        {
            var services = new ServiceCollection()
                .ConfigureOptions<ConfigureOpt>()
                .Configure<Opt>(name, opt => { opt.StringValue = "abc"; })
                ;

            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptionsMonitor<Opt>>();

            var actualStringValue = options.Get(name).StringValue;

            var expectedStringValue = ConfigureOpt.GetStringValue(name);

            actualStringValue.Should().Be(expectedStringValue);
        }
    }
}