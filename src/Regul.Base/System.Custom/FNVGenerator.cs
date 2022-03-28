using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Security.Cryptography;

public static class FnvGenerator
{
    private static readonly Dictionary<int, Tuple<ulong, ulong>> _consts = new()
    {
        {
            32,
            Tuple.Create(16777619UL, 2166136261UL)
        },
        {
            64,
            Tuple.Create(1099511628211UL, 14695981039346656037UL)
        }
    };

    public static ulong Hash(string text, int size, ulong? offset = null, bool highBit = false)
    {
        int key = size <= 32 ? 32 : 64;

        ulong prime = _consts[key].Item1;
        ulong num1 = _consts[key].Item2;
        ulong baseMask = ulong.MaxValue >> (64 - key);

        byte[] encoded = Encoding.Unicode.GetBytes(text.ToLower());

        ulong num2 = Enumerable.Range(0, encoded.Length >> 1).Select(i => BitConverter.ToUInt16(encoded, i << 1))
            .ToArray().Aggregate(offset ?? num1, (hashValue, b) => ((hashValue * prime) ^ b) & baseMask);

        if (key != size)
            num2 = (num2 >> size) ^ (num2 & (ulong.MaxValue >> (64 - size)));
        if (highBit)
            num2 |= (ulong)(1L << (key - 1));

        return num2;
    }
}