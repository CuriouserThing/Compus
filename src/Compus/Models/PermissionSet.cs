using System;
using System.Globalization;
using System.Numerics;

namespace Compus.Models
{
    public readonly struct PermissionSet
    {
        private readonly BigInteger _set;

        internal PermissionSet(string set)
        {
            _set = BigInteger.Parse(set, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite);
        }

        public PermissionSet(params Permission[] permissions)
        {
            if (permissions.Length == 0)
            {
                _set = 0;
                return;
            }

            var max = 0;
            foreach (Permission perm in permissions)
            {
                var p = (int)perm;
                if (p > max) { max = p; }
            }

            Span<byte> bytes = stackalloc byte[max / 8 + 1];
            foreach (Permission perm in permissions)
            {
                var p = (int)perm;
                bytes[p / 8] |= (byte)(1 << (p % 8));
            }

            _set = new BigInteger(bytes, true);
        }

        public bool HasPermission(Permission permission)
        {
            return ((_set >> (int)permission) & 0x1) == 0x1;
        }

        public override string ToString()
        {
            return _set.ToString();
        }
    }
}
