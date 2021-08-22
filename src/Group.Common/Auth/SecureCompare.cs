namespace Group.Common.Auth
{
    using System;

    public static class SecureCompare
    {
        public static bool Compare(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            if (a.Length != b.Length)
                return false;
            var same = true;
            for (var i = 0; i < a.Length; ++i)
            {
                same &= a[i] == b[i];
            }
            return same;
        }

        public static bool Compare(string a, string b)
        {
            if (a.Length != b.Length)
                return false;
            var same = true;
            for (var i = 0; i < a.Length; ++i)
            {
                same &= a[i] == b[i];
            }
            return same;
        }
    }
}