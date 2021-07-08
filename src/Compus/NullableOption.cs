namespace Compus
{
    public readonly struct NullableOption<T>
    {
        private readonly T? _value;
        private readonly bool _isSome;

        private NullableOption(T? value, bool isSome)
        {
            _value  = value;
            _isSome = isSome;
        }

        public bool IsNone => !_isSome;

        public bool IsSome(out T? value)
        {
            value = _value;
            return _isSome;
        }

        public static NullableOption<T> None => default;

        public static NullableOption<T> Some(T? value)
        {
            return new(value, true);
        }

        public static implicit operator NullableOption<T>(T value)
        {
            return new(value, true);
        }

        public override string ToString()
        {
            return _isSome ? _value?.ToString() ?? "null" : "None";
        }
    }
}
