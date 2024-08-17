using PeerSharp.Bencode.Tokens;
using Xunit.Sdk;

namespace PeerSharp.Bencode.Tests
{
    public class BencodeParserV2_Tests
    {
        private const string filepath = @"C://test.torrent";
        private readonly BencodeDictionary testData;

        private static Stream TestFileStream => File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        public BencodeParserV2_Tests()
        {
            this.testData = (BencodeDictionary)BencodeParserV2.Parse(TestFileStream);
        }

        [Fact]
        public void CanOpenFileCorrectly()
        {
            BencodeAssert.IsTypeOfDictionary(testData);
            Assert.Equal(9, testData.Count);
            Assert.Equal("UTF-8", testData["encoding"] as BencodeString);

            BencodeAssert.IsTypeOfDictionary(testData["info"]);
            var info = (BencodeDictionary)testData["info"];

            BencodeAssert.IsTypeOfList(info["files"]);
            var files = (BencodeList)info["files"];
            Assert.Equal(10, files.Count);
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