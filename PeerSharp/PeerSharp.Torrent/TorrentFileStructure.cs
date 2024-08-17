using PeerSharp.Bencode.Tokens;
using System.Linq;
using System.Text;

namespace PeerSharp.Torrent
{
    public class TorrentFileStructure
    {
        public string? Announce { get; set; }
        public List<string> AnnounceList { get; set; } = [];
        public string? Comment { get; set; }
        public string? CreatedBy { get; set; }
        public long? CreationDate { get; set; }
        public Encoding? Encoding { get; set; }
        public TorrentFileInfo? Info { get; set; }
        public string? Publisher { get; set; }
        public Uri? PublisherURL { get; set; }

        public class TorrentFileInfo
        {
            public string Name { get; set; } = string.Empty;
            public long PieceLength { get; set; }
            public List<byte[]> Pieces { get; set; } = [];
            public long Length { get; set; }
            public List<FileInfo> Files { get; set; } = [];

            public class FileInfo(List<string> path, long length)
            {
                public List<string> Path { get; set; } = path;
                public long Length { get; set; } = length;
            }
        }

        public static TorrentFileStructure Load(Stream stream)
        {
            var torrent = new TorrentFileStructure();
            var bencode = (BencodeDictionary) Bencode.BencodeParserV2.Parse(stream);
            foreach (var item in bencode)
            {
                var (key, value) = item;
                switch (key)
                {
                    case "announce":
                        torrent.Announce = value.AsString();
                        break;
                    case "announce-list":
                        var announcers = value.AsList()
                            .SelectMany(x => x.AsList()
                                .Select(s => s.AsString().Value));
                        torrent.AnnounceList.AddRange(announcers);
                        break;
                    case "comment":
                        torrent.Comment = value.AsString();
                        break;
                    case "created by":
                        torrent.CreatedBy = value.AsString();
                        break;
                    case "creation date":
                        torrent.CreationDate = value.AsInteger();
                        break;
                    case "encoding":
                        torrent.Encoding = Encoding.GetEncoding(value.AsString());
                        break;
                    case "info":
                        torrent.Info = ReadTorrentFileInfo(value.AsDictionary());
                        break;
                    case "publisher":
                        torrent.Publisher = value.AsString();
                        break;
                    case "publisher-url":
                        torrent.PublisherURL = new Uri(value.AsString());
                        break;
                    default:
                        break;
                }
            }

            return torrent;
        }

        private static TorrentFileInfo ReadTorrentFileInfo(BencodeDictionary infoDict)
        {
            var info = new TorrentFileInfo()
            {
                Name = infoDict["name"]?.AsString(),
                PieceLength = infoDict["piece length"]?.AsInteger(),
                Length = infoDict.ContainsKey("length") ? infoDict["length"].AsInteger() : 0,
                Pieces = DecodePieces(infoDict["pieces"].AsString()),
                Files = infoDict["files"].AsList()
                    .Select(x => {
                        var file = x.AsDictionary();
                        var path = file["path"].AsList();
                        var filePath = path.Select(x => x.AsString().Value).ToList();
                        var fileLength = file["length"].AsInteger();

                        return new TorrentFileInfo.FileInfo(filePath, fileLength);
                    })
                    .ToList()
            };

            return info;

            static List<byte[]> DecodePieces(BencodeString str)
            {
                const int sha1Length = 20;
                if (str.Data.Length % 20 != 0 )
                    throw new InvalidDataException("pieces length is not a multiple of 20");

                var pieces = str.Data;
                var numberOfPieces = pieces.Length / sha1Length;
                var pieceHashes = new List<byte[]>(numberOfPieces);

                for (int i = 0; i < numberOfPieces; i++)
                {
                    var hash = new byte[sha1Length];
                    Array.Copy(pieces, i * sha1Length, hash, 0, sha1Length);
                    pieceHashes.Add(hash);
                }

                return pieceHashes;
            }
        }
    }
}
