using Microsoft.VisualStudio.TestPlatform.TestHost;
using WhatThreeGits;

namespace WhatThreeGits.Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void ShortWTGShouldReturn3Words()
        {
            var prog = new Program();
            string[] args = { "encode", "--short" };
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            bool shortMode = args.Contains("--short");
            bool fullMode = args.Contains("--full") || !shortMode;     // default
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            // Long Git Hash
            string longGitHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            byte[] sha1 = Convert.FromHexString(longGitHash[..40]);
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
        public void LongWTGShouldReturn9Words()
        {
            var prog = new Program();
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            // Long Git Hash
            string longGitHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            byte[] sha1 = Convert.FromHexString(longGitHash[..40]);
            string output = enc.Encode(sha1, appendChecksum: true);
            string[] words = output.Split('.');
            Assert.AreEqual(9, words.Length);
            Assert.IsTrue(words.All(word => word.Length > 0));
        }

        [TestMethod]
        public void NineWordsShouldMatchLongGitHash()
        {
            var prog = new Program();
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);
            var bigString = "abhiseka.bittner.single-trunked.seatmates.welsh-english.afrikah.pithecolobium.appoints.flinty";

            // Long Git Hash
            string longGitHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            byte[] sha1 = Convert.FromHexString(longGitHash[..40]);
            string output = enc.DecodeAuto(bigString);
            Assert.IsTrue(output==longGitHash);
        }




        [TestMethod]
        public void ThreeWordsShouldMatchShotGitHash()
        {
            var prog = new Program();
            string[] args = { "encode", "--short" };
            string wordsFile = "words.txt";
            string crcFile = "crc.txt";
            bool shortMode = args.Contains("--short");
            bool fullMode = args.Contains("--full") || !shortMode;     // default
            string threeWords = "abroma.cylindrodendrite.puke";

            var enc = new Encoder(wordsFile, File.Exists(crcFile) ? crcFile : null);

            // Long Git Hash
            string longGitHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            byte[] sha1 = Convert.FromHexString(longGitHash[..40]);
            string output = shortMode
                ? enc.DecodeAuto(threeWords)          // NEW ✓ 3-word mode
                : enc.Encode(sha1, appendChecksum: true);

            // Round tripped words should equal the original first 12 characters of the long hash.
            string shortHash = longGitHash.Substring(0,12);

            Assert.IsTrue(shortHash == output);
            Assert.IsTrue(output.Length==12);

        }


    }
}
