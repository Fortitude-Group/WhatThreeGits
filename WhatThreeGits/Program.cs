// ── Program.cs (replace the existing file) ─────────────────────────

using System.Diagnostics;

namespace WTG;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            Console.WriteLine("""
                              Usage
                                wtg encode [<hash>] [--short|--full (default) -json]   
                                wtg decode <phrase> [--json]
                              Other options
                                --json             output JSON (default: false)
                                --words <file>     path to main word-list  (default: words.txt)
                                --crc   <file>     256-word checksum list  (default: crc.txt)
                              """);
            return 0;
        }

        string wordsFile = Opt("--words", "words.txt");
        string crcFile = Opt("--crc", "crc.txt");
        bool shortMode = args.Contains("--short");
        bool jsonMode = args.Contains("--json");
        bool fullMode = args.Contains("--full") || !shortMode; // default

        var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

        try
        {
            switch (args[0])
            {
                case "encode":
                    string hex = args.Length > 1 && !args[1].StartsWith("--")
                        ? args[1]
                        : GitHead();
                    if (hex.Length < 40) throw new ArgumentException("need full 40-char SHA-1");

                    byte[] sha1 = Convert.FromHexString(hex[..40]);


                    string output = shortMode
                        ? enc.EncodeShort(sha1)
                        : enc.Encode(sha1, appendChecksum: true);

                    if (shortMode)
                    {
                        if (jsonMode)
                        {
                            Console.Write($"{{ \"{hex.Substring(0, 12)}\":\"{output}\"}}");
                        }
                        else
                        {
                            Console.WriteLine(output);
                        }
                    }
                    else
                    {
                        if (jsonMode)
                        {
                            Console.Write($"{{ \"{hex}\":\"{output}\"}}");
                        }
                        else
                        {
                            Console.WriteLine(output);
                        }
                    }

                    break;

                case "decode":
                    if (args.Length < 2) throw new ArgumentException("phrase missing");
                    string phrase = args[1];
                    var decoded = enc.DecodeAuto(phrase);
                    if (jsonMode)
                    {
                        Console.WriteLine($"{{\"{phrase}\":\"{decoded}\"}}");
                    }
                    else
                    {
                        Console.WriteLine(decoded);
                    }
                    break;

                default:
                    Console.Error.WriteLine("unknown command");
                    return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }

        /*─ helpers ─*/
        string Opt(string key, string def)
        {
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
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
            }) ?? throw new InvalidOperationException("git not found");
            p.WaitForExit();
            return p.StandardOutput.ReadLine()?.Trim()
                   ?? throw new InvalidOperationException("cannot read HEAD hash");
        }
    }
}