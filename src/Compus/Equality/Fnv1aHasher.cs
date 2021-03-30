namespace Compus.Equality
{
    // ReSharper disable once InconsistentNaming
    internal class Fnv1aHasher : IHasher
    {
        #region ✨ Magic ✨

        private const int FnvBasis = unchecked((int)0x811c9dc5);
        private const int FnvPrime = 0x01000193; // 2^24 + 2^8 + (2^7 + 2^4 + 2^1 + 2^0)

        private const byte FalseCode = 0x03;
        private const byte TrueCode = 0x65;
        private const byte NullObjectCode = 0x9d;
        private const byte NullSequenceCode = 0xfb;

        #endregion

        #region Implementation

        public int Start()
        {
            return FnvBasis;
        }

        public int Hash(int seed, byte value)
        {
            // XOR first, multiply second per FNV-1a
            return unchecked((seed ^ value) * FnvPrime);
        }

        public int Hash(int seed, bool value)
        {
            return Hash(seed, value ? TrueCode : FalseCode);
        }

        public int HashNull(int seed)
        {
            return Hash(seed, NullObjectCode);
        }

        public int HashNullSequence(int seed)
        {
            return Hash(seed, NullSequenceCode);
        }

        #endregion
    }
}
