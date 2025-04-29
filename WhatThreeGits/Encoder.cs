using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace WhatThreeGits;

public sealed class Encoder
{
    private readonly string[] _words;          // main vocabulary
    private readonly string[]? _crcWords;      // optional checksum vocabulary (256 words)
    private readonly int _bitsPerWord;
    private readonly int _wordsPerHash;        // 11 for SHA‑1 + 1 CRC
    private const int _sha1Bits = 160;

    public Encoder(string wordsFile, string? crcFile = null)
    {
        _words = File.ReadAllLines(wordsFile)
                     .Select(w => w.Trim().ToLowerInvariant())
                     .Where(w => w.Length > 0).Distinct().ToArray();

        _bitsPerWord = (int)Math.Floor(Math.Log2(_words.Length));
        _wordsPerHash = (int)Math.Ceiling((double)_sha1Bits / _bitsPerWord);

        if (_wordsPerHash < 3)
        {
            throw new InvalidOperationException("Word list too small; need ≥ ~4 000 entries.");
        }

        if (crcFile is not null && File.Exists(crcFile))
        {
            _crcWords = File.ReadAllLines(crcFile)
                            .Select(w => w.Trim().ToLowerInvariant())
                            .Where(w => w.Length > 0).Distinct().ToArray();
            if (_crcWords.Length != 256)
            {
                throw new InvalidOperationException("crc.txt must contain exactly 256 words.");
            }
        }
    }

    /*──────────────────────── Encode ────────────────────────*/
    public string Encode(ReadOnlySpan<byte> sha1Hash, bool appendChecksum = true)
    {
        if (sha1Hash.Length != 20)
        {
            throw new ArgumentException("SHA‑1 must be 20 bytes.", nameof(sha1Hash));
        }

        // Interpret the 20‑byte SHA‑1 as ONE unsigned, big‑endian integer.
        // This avoids the old 'reverse‑then‑concat‑[0]' dance and the
        // compiler error bollox
        BigInteger value = new BigInteger(sha1Hash, isUnsigned: true, isBigEndian: true);

        Span<int> idx = stackalloc int[_wordsPerHash];
        int baseN = _words.Length;

        // Extract base‑N “digits” (least‑significant first)
        for (int i = 0; i < _wordsPerHash; i++)
        {
            idx[i] = (int)(value % baseN);
            value /= baseN;
        }

        // Emit most‑significant word first → reverse the span
        var phraseWords = idx.ToArray().Reverse().Select(i => _words[i]).ToList();

        if (appendChecksum && _crcWords is not null)
        {
            byte crc = Crc8(sha1Hash);
            phraseWords.Add(_crcWords[crc]);
        }

        return string.Join('.', phraseWords);
    }

    /*──────────────────────── Decode ────────────────────────*/
    public string Decode(string phrase, bool verifyChecksum = true)
    {
        var tokens = phrase.Split('.', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.ToLowerInvariant()).ToList();

        // optional CRC word at the end?
        string? crcToken = null;
        if (verifyChecksum && _crcWords is not null && tokens.Count == _wordsPerHash + 1)
        {
            crcToken = tokens.Last();
            tokens.RemoveAt(tokens.Count - 1);
        }

        if (tokens.Count != _wordsPerHash)
        {
            throw new FormatException($"Phrase must contain {_wordsPerHash} words (+1 checksum optional).");
        }

        BigInteger value = 0;
        int baseN = _words.Length;
        foreach (string w in tokens)
        {
            int idx = IndexOfNearestWord(w);
            value = value * baseN + idx;
        }

        // back to 20‑byte SHA‑1
        var bytes = value.ToByteArray();
        Array.Resize(ref bytes, 21); // ensure at least 21 bytes (extra sign byte)
        Array.Reverse(bytes);
        ReadOnlySpan<byte> sha1 = bytes.AsSpan()[1..21];

        if (verifyChecksum && _crcWords is not null && crcToken is not null)
        {
            byte crc = Crc8(sha1);
            if (!string.Equals(_crcWords[crc], crcToken, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Checksum failed – possible word error.");
            }
        }

        return Convert.ToHexString(sha1).ToLowerInvariant();
    }

    /*───────────────── Helpers ─────────────────*/
    private int IndexOfNearestWord(string token)
    {
        int idx = Array.IndexOf(_words, token);
        if (idx >= 0)
        {
            return idx;
        }

        // simple Levenshtein distance ≤ 1 fallback
        for (int i = 0; i < _words.Length; i++)
        {
            if (Levenshtein(token, _words[i]) == 1)
            {
                return i;
            }
        }

        throw new KeyNotFoundException($"Unknown word '{token}'.");
    }

    /* CRC‑8‑ATM (x⁸+x²+x+1) */
    private static byte Crc8(ReadOnlySpan<byte> data)
    {
        byte crc = 0;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (byte)((crc & 0x80) != 0 ? (crc << 1) ^ 0x07 : crc << 1);
            }
        }
        return crc;
    }

    private static int Levenshtein(string s, string t)
    {
        if (s == t)
        {
            return 0;
        }

        if (Math.Abs(s.Length - t.Length) > 1)
        {
            return 2; // fast exit
        }

        int[,] d = new int[2, t.Length + 1];
        for (int j = 0; j <= t.Length; j++)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= s.Length; i++)
        {
            d[i & 1, 0] = i;
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i & 1, j] = Math.Min(
                    Math.Min(d[(i - 1) & 1, j] + 1, d[i & 1, j - 1] + 1),
                    d[(i - 1) & 1, j - 1] + cost);
            }
        }
        return d[s.Length & 1, t.Length];
    }


    // ── Encoder.cs (add two tiny helpers) ──────────────────────────────
    public string EncodeShort(ReadOnlySpan<byte> sha1)          // 3-word, 48-bit slice
    {
        Span<byte> slice = stackalloc byte[6];    // first 48 bits = 12 hex chars
        sha1[..6].CopyTo(slice);
        BigInteger v = new BigInteger(slice.ToArray().Reverse().Append((byte)0).ToArray());

        int baseN = _words.Length;
        int[] idx = { (int)(v % baseN), 0, 0 };
        v /= baseN; idx[1] = (int)(v % baseN);
        v /= baseN; idx[2] = (int)v;

        return $"{_words[idx[2]]}.{_words[idx[1]]}.{_words[idx[0]]}";
    }

    /* decode chooses 3-word or 11(+1)-word automatically */
    public string DecodeAuto(string phrase)
        => phrase.Split('.', StringSplitOptions.RemoveEmptyEntries).Length switch
        {
            3 => DecodeShort(phrase),
            _ => Decode(phrase, verifyChecksum: true)
        };

    private string DecodeShort(string phrase)                  // returns 12-hex slice
    {
        string[] tok = phrase.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (tok.Length != 3) throw new FormatException("need exactly 3 words");
        BigInteger v = 0; int baseN = _words.Length;
        foreach (string w in tok) v = v * baseN + IndexOfNearestWord(w);
        var bytes = v.ToByteArray();
        Array.Resize(ref bytes, 6); Array.Reverse(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();   // 12 hex chars
    }


}
