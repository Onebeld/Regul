﻿namespace System.Security.Cryptography;

public class FNV
{
    public static uint Hash24(string s, bool highBit)
    {
        return (uint)FnvGenerator.Hash(s, 24, new ulong?(), (highBit ? 1 : 0) != 0);
    }

    public static uint Hash32(string s, bool highBit)
    {
        return (uint)FnvGenerator.Hash(s, 32, new ulong?(), (highBit ? 1 : 0) != 0);
    }

    public static ulong Hash56(string s, bool highBit)
    {
        return FnvGenerator.Hash(s, 56, new ulong?(), (highBit ? 1 : 0) != 0);
    }

    public static ulong Hash64(string s, bool highBit)
    {
        return FnvGenerator.Hash(s, 64, new ulong?(), (highBit ? 1 : 0) != 0);
    }
}