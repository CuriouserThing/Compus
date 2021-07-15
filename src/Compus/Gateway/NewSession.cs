using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Compus.Gateway
{
    internal class NewSession : Session
    {
        private const string LibraryName = "compus";
        private static readonly string OsName = GetOsName();

        private readonly IdentifyData _identityData;
        private readonly ILogger _logger;

        public NewSession(Identity identity, Action<string, JsonElement> eventDispatchCallback, ILogger logger) : base(eventDispatchCallback, logger)
        {
            _logger = logger;
            _identityData = new IdentifyData
            {
                Token = identity.Token,
                Properties = new IdentifyConnectionProperties
                {
                    Os      = OsName,
                    Browser = LibraryName,
                    Device  = LibraryName,
                },
                Shard   = identity.Shard,
                Intents = identity.Intents,
            };
        }

        protected override async Task RespondToHello()
        {
            _logger.LogInformation("Client identifying to gateway.");
            await SendPayload(GatewayOpcode.Identify, _identityData);
        }

        private static string GetOsName()
        {
            if (OperatingSystem.IsLinux())
            {
                return "linux";
            }
            else if (OperatingSystem.IsWindows())
            {
                return "windows";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "macos";
            }
            else if (OperatingSystem.IsFreeBSD())
            {
                return "freebsd";
            }
            else if (OperatingSystem.IsBrowser())
            {
                return "browser";
            }
            else if (OperatingSystem.IsAndroid())
            {
                return "android";
            }
            else if (OperatingSystem.IsIOS())
            {
                return "ios";
            }
            else if (OperatingSystem.IsTvOS())
            {
                return "tvos";
            }
            else if (OperatingSystem.IsWatchOS())
            {
                return "watchos";
            }
            else
            {
                return "unknown";
            }
        }
    }
}
