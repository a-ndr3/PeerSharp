using PeerSharp.Bencode.Tokens;
using PeerSharp.TestsUtils;
using Xunit.Sdk;

namespace PeerSharp.Bencode.Tests
{
    public class BencodeParserV2_Tests
    {
        private readonly BencodeDictionary testData;

        public BencodeParserV2_Tests()
        {
            this.testData = (BencodeDictionary)BencodeParserV2.Parse(TestData.test_Aligment_256_P256_470);
        }

        [Fact]
        public void CanOpenFileCorrectly()
        {
            BencodeAssert.IsTypeOfDictionary(testData);
            Assert.Equal(7, testData.Count);

            BencodeAssert.IsTypeOfDictionary(testData["info"]);
            var info = (BencodeDictionary)testData["info"];

            BencodeAssert.IsTypeOfList(info["files"]);
            var files = (BencodeList)info["files"];
            Assert.Equal(13, files.Count);
        }
    }

    internal static class BencodeAssert
    {
        public static void IsTypeOfDictionary(this BencodeToken token) => ThrowIf(token.Type != BencodeToken.TokenType.Dictionary || token is not BencodeDictionary);
        public static void IsTypeOfList(this BencodeToken token) => ThrowIf(token.Type != BencodeToken.TokenType.List || token is not BencodeList);
        public static void IsTypeOfString(this BencodeToken token) => ThrowIf(token.Type != BencodeToken.TokenType.String || token is not BencodeDictionary);

        private static void ThrowIf(bool condition, string message = "Assert failed") { if (condition) throw new XunitException(message); }
    }
}