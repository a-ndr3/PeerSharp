using PeerSharp.Bencode.Tokens;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Web;

namespace PeerSharp.Torrent.Tests
{
    public class RequestTest
    {
        [Fact]
        public async Task Test()
        {
            var torrentFilePath = @"C:\Download\TestTorrent.torrent";
            using var stream = File.OpenRead(torrentFilePath);
            var bencodedData = Bencode.BencodeParserV2.Parse(stream);
            var torrent = TorrentFileStructure.Load(torrentFilePath);
            var dict = bencodedData.AsDictionary();
            var hash = SHA1.HashData(bencodedData.AsDictionary()["info"].MemorySegment);
            var hashString = HttpUtility.UrlEncode(hash);

            string url = torrent.Announce!;

            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["peer_id"] = "peer-sharp-000000000";
            query["port"] = "6881";
            query["uploaded"] = "0";
            query["downloaded"] = "0";
            query["left"] = "0";
            query["event"] = "started";
            query["numwant"] = "50";
            query["compact"] = "0";
            builder.Query = query.ToString();
            var finalUri = builder.ToString() + $"&info_hash={hashString}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PeerSharp/1.0");
            var result = client.Send(new HttpRequestMessage(HttpMethod.Get, finalUri));
            var content = await result.Content.ReadAsStreamAsync();
            var resultBencoded = Bencode.BencodeParserV2.Parse(content);
            Assert.True(!resultBencoded.AsDictionary().ContainsKey("failure reason"), $"Failed to retrieve information from server");

            var list = new List<string>();
            if (resultBencoded.AsDictionary()["peers"].Type == BencodeToken.TokenType.String)
            {
                var peers = resultBencoded.AsDictionary()["peers"].AsString().Data;
                var buf = new byte[2];
                for (int i = 0; i < peers.Length / 6; i++)
                {
                    var offset = i * 6;
                    var ip = $"{peers[offset + 0]}.{peers[offset + 1]}.{peers[offset + 2]}.{peers[offset + 3]}";
                    peers.AsSpan(offset + 4, 2).CopyTo(buf);
                    buf.AsSpan().Reverse();
                    var port = BitConverter.ToUInt16(buf);
                    list.Add($"{ip}:{port}");
                }
            }
            else if (resultBencoded.AsDictionary()["peers"].Type == BencodeToken.TokenType.List)
            {
                var peers = resultBencoded.AsDictionary()["peers"].AsList();
                foreach (var bpeer in peers)
                {
                    var peer = bpeer.AsDictionary();
                    var peerId = peer["peer id"].AsString();
                    var ip = peer["ip"].AsString();
                    var port = peer["port"].AsInteger();
                    list.Add($"{ip.Value}:{port.Value} - {peerId.Value}");
                }
            }
            list.ForEach(x => Debug.WriteLine(x));
        }
    }
}
