using PeerSharp.TestsUtils;

namespace PeerSharp.Torrent.Tests
{
    public class TorrentFileParsing_Tests
    {
        [Fact]
        public void CanOpenFile()
        {
            var files = new Stream[]
            {
                TestData.test_Aligment_256_P256_470,
                TestData.test_Aligment_512_P2M_60,
                TestData.test_Aligment_512_P64_1873,
                TestData.test_NoAligment_P4M_30,
                TestData.test_NoAligment_P16_7480,
                TestData.test_NoAligment_P32M_4,
                TestData.test_NoAligment_P512_234,
            };

            foreach (var file in files)
            {
                var torrent = TorrentFileStructure.Load(file);
            }
        }
    }
}