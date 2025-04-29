using System;
using System.Diagnostics;
using System.IO;

namespace WhatThreeGits;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h")
        {
            Console.WriteLine("""
                Usage:
                  wtg encode [<hash>]
                  wtg decode <phrase>
                Options:
                  --words <file>     path to main vocabulary (default: words.txt)
                  --crc <file>       256‑word checksum vocab (default: crc.txt)
                """);
            return 0;
        }

        string wordsFile = GetOption("--words", "words.txt");
        string crcFile = GetOption("--crc", "crc.txt");

        try
        {
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            switch (args[0])
            {
                case "encode":
                    string hash = args.Length > 1 ? args[1] : GitHead();
                    if (hash.Length < 40)
                    {
                        throw new ArgumentException("Full 40‑hex SHA‑1 required.");
                    }

                    byte[] bytes = Convert.FromHexString(hash[..40]);
                    Console.WriteLine(enc.Encode(bytes, appendChecksum: true));
                    break;

                case "decode":
                    if (args.Length < 2)
                    {
                        throw new ArgumentException("Phrase missing.");
                    }

                    string phrase = string.Join('.', args[1..]);
                    Console.WriteLine(enc.Decode(phrase, verifyChecksum: true));
                    break;

                default:
                    Console.Error.WriteLine("Unknown command.");
                    return 1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }

        /*──── helpers ────*/
        string GetOption(string key, string @default)
        {
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : @default;
        }

        static string GitHead()
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse HEAD",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }) ?? throw new InvalidOperationException("Git not found.");
            p.WaitForExit();
            return p.StandardOutput.ReadLine()?.Trim()
                   ?? throw new InvalidOperationException("Unable to read HEAD hash. (is this a repository?)");
        }
    }
}
