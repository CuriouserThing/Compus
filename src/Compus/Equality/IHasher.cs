using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Compus.Equality
{
    public interface IHasher
    {
        int Start();

        int Hash(int seed, byte value);

        int Hash(int seed, sbyte value)
        {
            return Hash(seed, (byte)value);
        }

        int HashNull(int seed)
        {
            const byte @null = 0;
            return Hash(seed, @null);
        }

        int HashNullSequence(int seed)
        {
            return HashNull(seed);
        }

        int HashBytes(int seed, Span<byte> bytes)
        {
            foreach (byte b in bytes)
            {
                seed = Hash(seed, b);
            }

            return seed;
        }

        int Hash(int seed, string? value)
        {
            if (value is null) { return HashNull(seed); }

            Span<byte> bytes = stackalloc byte[sizeof(char)];
            foreach (char c in value.AsSpan())
            {
                BitConverter.TryWriteBytes(bytes, c);
                HashBytes(seed, bytes);
            }

            return seed;
        }

        int HashSequence(int seed, IEnumerable<string?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        #region Generics

        int Hash<T>(int seed, T? obj) where T : IEquatablePart<T>
        {
            return obj is null ? HashNull(seed) : obj.ContinueHashCode(this, seed);
        }

        int HashSequence<T>(int seed, IEnumerable<T?>? objs) where T : IEquatablePart<T>
        {
            return objs?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int Hash<T>(int seed, T? obj, IPartialEqualityComparer<T> comparer)
        {
            return obj is null ? HashNull(seed) : comparer.ContinueHashCode(this, seed, obj);
        }

        int HashSequence<T>(int seed, IEnumerable<T?>? objs, IPartialEqualityComparer<T> comparer)
        {
            int HashLocal(int current, T? obj)
            {
                return Hash(current, obj, comparer);
            }

            return objs?.Aggregate(seed, HashLocal) ?? HashNullSequence(seed);
        }

        int HashReference<T>(int seed, T? obj) where T : class
        {
            return obj is null ? HashNull(seed) : Hash(seed, RuntimeHelpers.GetHashCode(obj));
        }

        int HashReferenceSequence<T>(int seed, IEnumerable<T?>? objs) where T : class
        {
            return objs?.Aggregate(seed, HashReference) ?? HashNullSequence(seed);
        }

        #endregion

        #region TryWriteBytes Primitives

        int Hash(int seed, bool value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(bool)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, char value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(char)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, double value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(double)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, float value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(float)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, int value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(int)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, uint value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(uint)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, long value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(long)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, ulong value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(ulong)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, short value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(short)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        int Hash(int seed, ushort value)
        {
            Span<byte> bytes = stackalloc byte[sizeof(ushort)];
            BitConverter.TryWriteBytes(bytes, value);
            return HashBytes(seed, bytes);
        }

        #endregion

        #region Nullable Primitives

        int Hash(int seed, bool? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, byte? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, sbyte? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, char? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, double? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, float? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, int? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, uint? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, long? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, ulong? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, short? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        int Hash(int seed, ushort? value)
        {
            return value.HasValue ? Hash(seed, value.Value) : HashNull(seed);
        }

        #endregion

        #region Primitive Sequences

        int HashSequence(int seed, IEnumerable<bool>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<bool?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<byte>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<byte?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<sbyte>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<sbyte?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<char>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<char?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<double>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<double?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<float>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<float?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<int>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<int?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<uint>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<uint?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<long>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<long?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<ulong>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<ulong?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<short>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<short?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<ushort>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        int HashSequence(int seed, IEnumerable<ushort?>? values)
        {
            return values?.Aggregate(seed, Hash) ?? HashNullSequence(seed);
        }

        #endregion
    }
}
