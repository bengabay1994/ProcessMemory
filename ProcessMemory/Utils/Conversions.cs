using System.Collections.Generic;
using System.Linq;

namespace ProcessMemory.Utils
{
    public static class Conversions
    {
        public static string GetByteArrayAsHexString(byte[] ba)
        {
            return string.Join(" ", ba.Select(b => b.ToString("X2")));
        }

        public static string GetOffsetsAsString(IEnumerable<long> offsets)
        {
            return string.Join(", ", offsets.Select(offset => offset.ToString("X")));
        }

        public static string GetOffsetsAsKey(IEnumerable<long> offsets)
        {
            return string.Join(",", offsets.Select(offset => offset.ToString("X")));
        }
    }
}
