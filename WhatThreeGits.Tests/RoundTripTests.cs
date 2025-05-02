using Microsoft.VisualStudio.TestPlatform.TestHost;
using WhatThreeGits;
using WTG;

namespace WhatThreeGits.Tests
{
    [TestClass]
    public sealed class RoundTripTests
    {
        [TestMethod]
        public void ShortHashShouldReturn3Words()
        {
            var prog = new Program();
            string[] args = { "encode", "--short" };
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            bool shortMode = args.Contains("--short");
            bool fullMode = args.Contains("--full") || !shortMode;     // default
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            // Short Git Hash
            string longGitHash = "c15cb901a0b2";
            byte[] sha1 = Convert.FromHexString(longGitHash[..12]);
            string output = shortMode
                ? enc.EncodeShort(sha1)          // NEW ✓ 3-word mode
                : enc.Encode(sha1, appendChecksum: true);
            string[] words = output.Split('.');
            Assert.AreEqual(3, words.Length);
            Assert.IsTrue(words.All(word => word.Length > 0));
            Assert.IsTrue(words.All(word => word.All(char.IsLetter)));
            Assert.IsTrue(words.All(word => word.All(char.IsLower)));
        }


        [TestMethod]
        public void LongHashShouldReturnTenWords()
        {
            var prog = new Program();
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            // Long Git Hash
            string longGitHash = "c15cb901a0b26d2321e4039918a9daaebd4877d9";
            byte[] sha1 = Convert.FromHexString(longGitHash[..40]);
            string output = enc.Encode(sha1, appendChecksum: true);
            string[] words = output.Split('.');

            //| Vocabulary size | Bits / word ≈ log₂ N | Words needed(160 / bits / word → round up) |
            //| -----------------| --------------------| -------------------------------------------|
            //| 32 768 | 15 bits | 11 |
            //| 40 000 | 15.3 bits | 11 |
            //| 466 k (DWYL) | 18.8 bits      | 9                                     |
            //| 1 048 576 | 20 bits | 8 |

            // 9 + 1 (CRC)
            Assert.AreEqual(10, words.Length);
            Assert.IsTrue(words.All(word => word.Length > 0));
        }

        [TestMethod]
        public void NineWordsShouldMatchLongGitHash()
        {
            var prog = new Program();
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);
            var bigString = "abeyant.silver-chiming.goannas.postadolescences.parrying.pragmaticality.shagginess.pendn.untribal";

            // Long Git Hash
            string longGitHash = "c15cb901a0b26d2321e4039918a9daaebd4877d9";
            string output = enc.DecodeAuto(bigString);
            Assert.IsTrue(output==longGitHash);
        }

        [TestMethod]
        public void ThreeWordsShouldMatchShortGitHash()
        {
            var prog = new Program();
            string[] args = { "encode", "--short" };
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            bool shortMode = args.Contains("--short");
            bool fullMode = args.Contains("--full") || !shortMode;     // default
            string threeWords = "abortionists.sappiness.hitherward";

            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            // Long Git Hash
            string longGitHash = "c15cb901a0b2";
            byte[] sha1 = Convert.FromHexString(longGitHash[..12]);
            string output = shortMode
                ? enc.DecodeAuto(threeWords)          // NEW 3-word mode
                : enc.Encode(sha1, appendChecksum: true);

            // Round tripped words should equal the original first 12 characters of the long hash.
            string shortHash = longGitHash.Substring(0,12);

            Assert.IsTrue(shortHash == output);
            Assert.IsTrue(output.Length==12);

        }


    }
}
