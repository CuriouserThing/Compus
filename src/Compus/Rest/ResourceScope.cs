namespace Compus.Rest;

/// <summary>
///     Describes the scope of a Discord API call through the snowflake IDs and string tokens of its parameterized
///     resources, for the purpose of rate limiting and other tracking.
/// </summary>
public record ResourceScope
{
    public Option<Snowflake> Application { get; init; }
    public Option<Snowflake> Channel { get; init; }
    public Option<Snowflake> Command { get; init; }
    public Option<Snowflake> Emoji { get; init; }
    public Option<Snowflake> Guild { get; init; }
    public Option<Snowflake> Integration { get; init; }
    public Option<Snowflake> Interaction { get; init; }
    public Option<string> InteractionToken { get; init; }
    public Option<Snowflake> Message { get; init; }
    public Option<Snowflake> Overwrite { get; init; }
    public Option<Snowflake> Role { get; init; }
    public Option<Snowflake> User { get; init; }
    public Option<Snowflake> Webhook { get; init; }
    public Option<string> WebhookToken { get; init; }
}
