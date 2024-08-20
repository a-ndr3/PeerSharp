using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace PeerSharp.Torrent
{
    public class TorrentFileValidator
    {
        public static void ValidateFiles(TorrentFileStructure torrent, string filepath)
        {
            var pieceLength = (int)torrent.Info.PieceLength;
            var piece = new byte[pieceLength];
            var pieceIndex = 0;
            var readBytes = 0;

            foreach (var file in torrent.Info.Files)
            {
                var checkingFile = $@"{filepath}{file.NormalizedPath}";
                using var fileStream = File.OpenRead(checkingFile);

                var startIndex = readBytes;

                while ((readBytes = fileStream.Read(piece, startIndex, pieceLength - startIndex)) != 0)
                {
                    if (readBytes != pieceLength - startIndex)
                    {
                        if (pieceIndex == torrent.Info.Pieces.Count - 1) // if it is last piece in torrent
                        {
                            var lastPieceSha1 = SHA1.HashData(piece.AsSpan(0, readBytes));
                            var lastPieceSha1Str = Convert.ToHexString(lastPieceSha1);
                            var torrentLastPieceSha1Str = Convert.ToHexString(torrent.Info.Pieces[pieceIndex]);
                            if (lastPieceSha1Str != torrentLastPieceSha1Str)
                                ThrowValidationException(checkingFile);
                        }

                        break;
                    }

                    var pieceSha1 = SHA1.HashData(piece.AsSpan());
                    var pieceSha1Str = Convert.ToHexString(pieceSha1);
                    var torrentPieceSha1Str = Convert.ToHexString(torrent.Info.Pieces[pieceIndex]);
                    if (pieceSha1Str != torrentPieceSha1Str)
                        ThrowValidationException(checkingFile);
                    pieceIndex++;
                    startIndex = 0;
                }
            }
        }

        private static void ThrowValidationException(string filename) => throw new ValidationException($"Validation failed for {filename}");
    }
}
