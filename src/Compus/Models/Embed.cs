using System;
using System.Collections.Generic;
using System.Drawing;

namespace Compus.Models
{
    /// <summary>
    ///     https://discord.com/developers/docs/resources/channel#embed-object
    /// </summary>
    public record Embed
    {
        public Option<string> Title { get; init; }

        public Option<string> Type { get; init; }

        public Option<string> Description { get; init; }

        public Option<string> Url { get; init; }

        public Option<DateTimeOffset> Timestamp { get; init; }

        public Option<Color> Color { get; init; }

        public Option<EmbedFooter> Footer { get; init; }

        public Option<EmbedImage> Image { get; init; }

        public Option<EmbedThumbnail> Thumbnail { get; init; }

        public Option<EmbedVideo> Video { get; init; }

        public Option<EmbedProvider> Provider { get; init; }

        public Option<EmbedAuthor> Author { get; init; }

        public Option<IReadOnlyList<EmbedField>> Fields { get; init; }
    }
}
