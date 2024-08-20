using System.Security.Cryptography;

namespace PeerSharp.Torrent.Tests
{
    public class TorrentFile_Pieces_Tests
    {
        [Fact]
        public void PiecesAreAwesome()
        {
            var torrentFilePath = @"C:\test.torrent";
            var filesFolder = @"C:\Download\Test\";
            
            var torrent = TorrentFileStructure.Load(torrentFilePath);

            TorrentFileValidator.ValidateFiles(torrent, filesFolder);
        }
    }
}
