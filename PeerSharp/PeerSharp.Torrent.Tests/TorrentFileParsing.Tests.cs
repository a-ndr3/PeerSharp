namespace PeerSharp.Torrent.Tests
{
    public class TorrentFileParsing_Tests
    {
        private const string filepath = @"C://test.torrent";
        private static Stream TestFileStream => File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        [Fact]
        public void CanOpenFile()
        {
            var file = TorrentFileStructure.Load(TestFileStream);
        }
    }
}