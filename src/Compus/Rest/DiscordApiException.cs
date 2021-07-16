using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Compus.Rest
{
    public class DiscordApiException : HttpRequestException
    {
        public DiscordApiException(string message, HttpStatusCode statusCode, Exception? inner = null) : base(message, inner, statusCode)
        {
        }

        public ErrorCode Code { get; init; }

        /// <summary>
        ///     https://discord.com/developers/docs/reference#error-messages
        /// </summary>
        public Option<IReadOnlyList<DataError>> Errors { get; init; }
    }
}
