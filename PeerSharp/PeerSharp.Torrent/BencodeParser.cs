using PeerSharp.Torrent;
using System.Text;

namespace PeerSharp.Bencode
{
    internal class BencodeParser
    {
        private Stream stream;
        private TorrentFileStructure parsedTorrentFile;

        public BencodeParser(byte[] data)
        {
            using (stream = new MemoryStream(data))
            {
                parsedTorrentFile = ParseTorrentFile((Dictionary<string, object>)Parse());
            }
        }

        public TorrentFileStructure GetParsedResult()
        {
            return parsedTorrentFile;
        }

        private TorrentFileStructure ParseTorrentFile(Dictionary<string, object> root)
        {
            TorrentFileStructure torrent = new TorrentFileStructure();

            if (root.TryGetValue("announce", out var announce))
                torrent.Announce = announce as string;

            if (root.TryGetValue("announce-list", out var announceListObj))
            {
                var announceList = (List<object>)announceListObj;

                foreach (var sublistObj in announceList)
                {
                    var sublist = (List<object>)sublistObj;

                    if (sublist.Count > 0)
                    {
                        torrent.AnnounceList.Add(sublist[0] as string);
                    }
                }
            }

            if (root.TryGetValue("comment", out var comment))
                torrent.Comment = comment as string;

            if (root.TryGetValue("created by", out var createdBy))
                torrent.CreatedBy = createdBy as string;

            if (root.TryGetValue("creation date", out var creationDate))
                torrent.CreationDate = (long)creationDate;

            if (root.TryGetValue("encoding", out var encoding))
                torrent.Encoding = Encoding.GetEncoding(encoding as string);

            if (root.TryGetValue("info", out var infoObject))
            {
                var infoDict = (Dictionary<string, object>)infoObject;

                var x = infoDict.GetValueOrDefault("pieces");

                TorrentFileStructure.TorrentFileInfo info = new TorrentFileStructure.TorrentFileInfo
                {
                    Name = infoDict.TryGetValue("name", out var name) ? name as string : null,

                    PieceLength = infoDict.TryGetValue("piece length", out var pieceLength) ? (long)pieceLength : 0,

                    Pieces = infoDict.TryGetValue("pieces", out var pieces) ? DecodePieces(pieces as byte[]) : null,

                    Length = infoDict.TryGetValue("length", out var length) ? (long)length : 0
                };

                if (infoDict.TryGetValue("files", out var filesObj))
                {
                    var filesList = (List<object>)filesObj;
                    foreach (var fileObj in filesList)
                    {
                        var fileDict = (Dictionary<string, object>)fileObj;
                        long fileLength = (long)fileDict["length"];
                        var pathList = (List<object>)fileDict["path"];

                        List<string> filePath = new List<string>();

                        foreach (var path in pathList)
                        {
                            filePath.Add(path as string);
                        }

                        info.Files.Add(new TorrentFileStructure.TorrentFileInfo.FileInfo(filePath, fileLength));
                    }
                }

                torrent.Info = info;
            }

            if (root.TryGetValue("publisher", out var publisher))
                torrent.Publisher = publisher as string;

            if (root.TryGetValue("publisher-url", out var publisherUrl))
                torrent.PublisherURL = new Uri(publisherUrl as string);

            return torrent;
        }

        private object Parse()
        {
            int prefix = stream.ReadByte();

            char type = (char)prefix;

            switch (type)
            {
                case 'i': return ParseInteger();
                case 'l': return ParseList();
                case 'd': return ParseDictionary();
                default:
                    if (char.IsDigit(type))
                    {
                        return ParseString(type);
                    }
                    else
                    {
                        throw new InvalidDataException("missing bencode type");
                    }
            }
        }

        private object ParseRaw()
        {
            int prefix = stream.ReadByte();

            char type = (char)prefix;

            if (!char.IsDigit(type))
                throw new InvalidDataException("not digit in parseRaw prefix");

            return ParseStringRaw(type);
        }

        private string ParseString(char firstDigit)
        {
            int length = firstDigit - '0';

            int digit;
            while ((digit = stream.ReadByte()) != ':')
            {
                if (digit == -1)
                    throw new EndOfStreamException();

                if (!char.IsDigit((char)digit))
                    throw new InvalidDataException("invalid string length");

                length = length * 10 + (digit - '0');
            }

            byte[] buffer = new byte[length];

            int read = stream.Read(buffer, 0, length);

            if (read != length)
                throw new EndOfStreamException();

            return Encoding.UTF8.GetString(buffer);
        }

        private byte[] ParseStringRaw(char firstDigit)
        {
            int length = firstDigit - '0';

            int digit;
            while ((digit = stream.ReadByte()) != ':')
            {
                if (digit == -1)
                    throw new EndOfStreamException();

                if (!char.IsDigit((char)digit))
                    throw new InvalidDataException("invalid string length");

                length = length * 10 + (digit - '0');
            }

            byte[] buffer = new byte[length];
            int read = stream.Read(buffer, 0, length);

            if (read != length)
                throw new EndOfStreamException();

            return buffer;
        }

        private long ParseInteger()
        {
            StringBuilder sb = new StringBuilder();
            int readByte;

            while ((readByte = stream.ReadByte()) != 'e')
            {
                sb.Append((char)readByte);
            }

            if (!long.TryParse(sb.ToString(), out long result))
                throw new InvalidDataException("invalid integer in bencode");

            return result;
        }

        private List<object> ParseList()
        {
            List<object> list = new List<object>();

            while (true)
            {
                int prefix = stream.ReadByte();

                if (prefix == -1)
                    throw new EndOfStreamException();

                if ((char)prefix == 'e')
                    break;

                stream.Seek(-1, SeekOrigin.Current);

                list.Add(Parse());
            }

            return list;
        }

        public List<byte[]> DecodePieces(byte[] pieces)
        {
            const int sha1Length = 20;

            if (pieces.Length % sha1Length != 0)
                throw new InvalidDataException("pieces length is not a multiple of 20");

            int numberOfPieces = pieces.Length / sha1Length;

            List<byte[]> pieceHashes = new List<byte[]>(numberOfPieces);

            for (int i = 0; i < numberOfPieces; i++)
            {
                byte[] hash = new byte[sha1Length];

                Array.Copy(pieces, i * sha1Length, hash, 0, sha1Length);

                pieceHashes.Add(hash);
            }

            return pieceHashes;
        }

        private Dictionary<string, object> ParseDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            while (true)
            {
                int prefix = stream.ReadByte();

                if (prefix == -1)
                    throw new EndOfStreamException();

                if ((char)prefix == 'e')
                    break;

                stream.Seek(-1, SeekOrigin.Current);

                string key = (string)Parse();

                object value = null;

                if (key == "pieces")
                    value = ParseRaw();
                else
                    value = Parse();

                dict[key] = value;
            }

            return dict;
        }
    }

}
