namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/interactions/slash-commands#application-command-object-application-command-option-choice-structure
    /// </summary>
    public record ApplicationCommandOptionChoice
    {
        public string Name { get; init; }

        public string Value { get; init; }
    }
}
