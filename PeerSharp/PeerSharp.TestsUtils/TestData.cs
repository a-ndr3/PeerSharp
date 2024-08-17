namespace PeerSharp.TestsUtils
{
    public static class TestData
    {
        public static Stream TestFileStream(string fileName) => File.Open(FilePath(fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        public static string FilePath(string fileName) => $@"{Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName}\PeerSharp.TestsUtils\TestData\{fileName}";

        public static Stream test_Aligment_256_P256_470 => TestFileStream("test_Aligment_256_P256_470.torrent");
        public static Stream test_Aligment_512_P2M_60 => TestFileStream("test_Aligment_512_P2M_60.torrent");
        public static Stream test_Aligment_512_P64_1873 => TestFileStream("test_Aligment_512_P64_1873.torrent");
        public static Stream test_NoAligment_P4M_30 => TestFileStream("test_NoAligment_P4M_30.torrent");
        public static Stream test_NoAligment_P16_7480 => TestFileStream("test_NoAligment_P16_7480.torrent");
        public static Stream test_NoAligment_P32M_4 => TestFileStream("test_NoAligment_P32M_4.torrent");
        public static Stream test_NoAligment_P512_234 => TestFileStream("test_NoAligment_P512_234.torrent");
    }
}
