﻿namespace Group.WebApi.Auth
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Security.Cryptography;

    public class SecretManager
    {
        readonly Func<HMAC> _genHmac;

        public SecretManager(Func<HMAC> genHmac)
        {
            _genHmac = genHmac;
        }

        public (byte[] Salt, byte[] Hash) Create()
        {
            using var hmac = _genHmac();
            var salt = new byte[hmac.HashSize / 8];
            RandomNumberGenerator.Fill(salt);
            var hash = hmac.ComputeHash(salt);
            return (salt, hash);
        }

        public bool Check(ReadOnlyMemory<byte> salt, ReadOnlyMemory<byte> hash)
        {
            if (salt.Length != hash.Length)
                return false;
            using var hmac = _genHmac();
            if (salt.Length != hmac.HashSize / 8)
                return false;
            using var rental = MemoryPool<byte>.Shared.Rent(salt.Length);
            try
            {
                var buffer = rental.Memory.Span[..salt.Length];
                if (!hmac.TryComputeHash(
                    salt.Span,
                    buffer,
                    out var numBytesWritten))
                    return false;
                return numBytesWritten == buffer.Length && Compare(buffer, hash.Span);
            }
            finally
            {
                rental.Memory.Span.Clear();
            }
        }

        public bool CheckRaw(ReadOnlyMemory<byte> secret)
        {
            using var hmac = _genHmac();
            if (secret.Length != hmac.HashSize / 8)
                return false;
            return Compare(hmac.Key, secret.Span);
        }

        static bool Compare(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            Debug.Assert(a.Length == b.Length);
            var same = true;
            for (var i = 0; i < a.Length; ++i)
            {
                same &= a[i] == b[i];
            }
            return same;
        }
    }
}