using System.Diagnostics.CodeAnalysis;

namespace Compus
{
    public readonly struct Option<T> where T : notnull
    {
        private readonly T _value;
        private readonly bool _isSome;

        private Option(T value, bool isSome)
        {
            _value  = value;
            _isSome = isSome;
        }

        public bool IsNone => !_isSome;

        public bool IsSome([NotNullWhen(true)] out T? value)
        {
            value = _value;
            return _isSome;
        }

        public static Option<T> None => default;

        public static Option<T> Some(T value)
        {
            return new(value, true);
        }

        public static implicit operator Option<T>(T value)
        {
            return new(value, true);
        }

        public override string? ToString()
        {
            return _isSome ? _value.ToString() : "None";
        }
    }
}
