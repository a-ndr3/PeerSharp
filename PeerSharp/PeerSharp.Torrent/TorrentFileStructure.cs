using System.Text;

namespace PeerSharp.Torrent
{
    public class TorrentFileStructure
    {
        public String Announce { get; set; }

        public List<String> AnnounceList { get; set; } = new List<String>();

        public String Comment { get; set; }

        public String CreatedBy { get; set; }

        public long CreationDate { get; set; }

        public Encoding Encoding { get; set; }

        public TorrentFileInfo Info { get; set; }

        public String Publisher { get; set; }

        public Uri PublisherURL { get; set; }


        public class TorrentFileInfo
        {
            public string Name { get; set; }
            public long PieceLength { get; set; }
            public List<byte[]> Pieces { get; set; }
            public long Length { get; set; }
            public List<FileInfo> Files { get; set; } = new List<FileInfo>();

            public class FileInfo
            {
                public List<string> Path { get; set; } = new List<string>();
                public long Length { get; set; }

                public FileInfo(List<string> path, long length)
                {
                    Path = path;
                    Length = length;
                }
            }
        }
    }
}
