namespace PeerSharp.Bencode
{
    internal class BencodeStreamReader : IDisposable
    {
        private readonly Stream stream;
        private readonly byte[] bytes;

        public BencodeStreamReader(Stream stream)
        {
            bytes = new byte[stream.Length];
            var read = stream.Read(bytes, 0, (int)stream.Length);
            if (read != stream.Length) throw new Exception("meh");
            this.stream = new MemoryStream(bytes);
        }

        public char Peek()
        {
            var value = stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);
            return (char)value;
        }

        public bool EndOfStream => stream.Position >= stream.Length;

        public long Position { get => stream.Position; set => stream.Position = value; }

        public int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

        public char Read()
        {
            const int bufferSize = 1;
            unsafe
            {
                var buffer = stackalloc byte[bufferSize];
                var len = stream.Read(new Span<byte>(buffer, bufferSize));
                if (len != 1)
                    throw new EndOfStreamException($"Unable to read character from stream at '{stream.Position}'");
                return (char)buffer[0];
            }
        }

        public ArraySegment<byte> GetSegment(int offset, int count) => new(bytes, offset, count);

        public void Dispose() => stream.Dispose();
    }
}
