using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Compus.Gateway
{
    internal class ResumedSession : Session
    {
        private readonly ILogger _logger;
        private readonly ResumeData _resumeData;

        public ResumedSession(ResumeData resumeData, Action<string, JsonElement> eventDispatchCallback, ILogger logger) : base(eventDispatchCallback, logger)
        {
            _logger     = logger;
            _resumeData = resumeData;
        }

        protected override async Task RespondToHello()
        {
            _logger.LogInformation("Client resuming a reconnected session.");
            await SendPayload(GatewayOpcode.Resume, _resumeData);
        }
    }
}
