using System;
using Compus.Equality;

namespace Compus
{
    public readonly struct Snowflake : IEquatable<Snowflake>
    {
        private static readonly DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private const long MaxTimestamp = (1L << 42) - 1;

        private readonly ulong _value;

        public Snowflake(ulong value)
        {
            _value = value;
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///     Time is before the Discord epoch (i.e. before 2015) or after the latest
        ///     value a snowflake can store (e.g. after a point in 2154).
        /// </exception>
        public static Snowflake FromTimestamp(DateTimeOffset time)
        {
            long timestamp = (time - DiscordEpoch).Ticks / TimeSpan.TicksPerMillisecond;
            return timestamp switch
            {
                < 0            => throw new ArgumentOutOfRangeException(nameof(time)),
                > MaxTimestamp => throw new ArgumentOutOfRangeException(nameof(time)),
                _              => new Snowflake((ulong)timestamp << 22),
            };
        }

        public DateTimeOffset Timestamp => DiscordEpoch + TimeSpan.FromMilliseconds(_value >> 22);

        public static implicit operator Snowflake(ulong id)
        {
            return new(id);
        }

        public static implicit operator ulong(Snowflake id)
        {
            return id._value;
        }

        public override string ToString()
        {
            // Some methods may rely on this returning the raw numerical value for simplicity. Do not change.
            return _value.ToString();
        }

        #region Equality

        public static IFullEqualityComparer<Snowflake> EqualityComparer { get; } = new Identity<Snowflake>()
            .With(snowflake => snowflake._value)
            .ToComparer();

        public override int GetHashCode()
        {
            return EqualityComparer.GetHashCode(this);
        }

        public override bool Equals(object? obj)
        {
            return EqualityComparer.Equals(this, obj);
        }

        public bool Equals(Snowflake other)
        {
            return EqualityComparer.Equals(this, other);
        }

        public static bool operator ==(Snowflake left, Snowflake right)
        {
            return EqualityComparer.Equals(left, right);
        }

        public static bool operator !=(Snowflake left, Snowflake right)
        {
            return !(left == right);
        }

        #endregion
    }
}
