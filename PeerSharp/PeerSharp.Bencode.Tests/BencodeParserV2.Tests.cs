namespace PeerSharp.Bencode.Tests
{
    public class BencodeParserV2_Tests
    {
        private const string filepath = @"C://test.torrent";

        static Stream TestFileStream => File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        [Fact]
        public void CanOpenFile()
        {
            var result = BencodeParserV2.Parse(TestFileStream);
        }
    }
}