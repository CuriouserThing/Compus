using System.Text.Json;

namespace Compus.Rest
{
    /// <summary>
    ///     https://discord.com/developers/docs/topics/opcodes-and-status-codes#json
    /// </summary>
    internal record ErrorContent
    {
        public ErrorCode Code { get; init; }

        /// <summary>
        ///     https://discord.com/developers/docs/reference#error-messages
        /// </summary>
        public Option<JsonElement> Errors { get; init; }

        public string Message { get; init; }
    }
}
