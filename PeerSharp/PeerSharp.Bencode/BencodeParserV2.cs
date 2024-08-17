using System.Reflection;
using System.Text;

namespace PeerSharp.Bencode;

public class BencodeParserV2
{
    private const char endOfObject = 'e';

    public static BencodeToken Parse(Stream stream)
    {
        using var sr = new StreamReader(stream, Encoding.UTF8);
        return ParseInternal(sr);
    }

    public static BencodeToken ParseInternal(StreamReader stream)
    {
        var prefix = 0;

        while ((prefix = stream.Peek()) >= 0)
        {
            var type = (char)prefix;

            switch (type)
            {
                case 'd': return ParseDictionary(stream);
                case 'i': return ParseInteger(stream);
                case 'l': return ParseList(stream);
                default:
                    if (char.IsDigit(type))
                        return ParseString(stream);
                    else
                        throw new NotSupportedException($"Unknown bencode type '{type}'[{prefix}] at {stream.GetPosition()}");
            }
        }

        throw new EndOfStreamException("Unexpected end of stream while parsing tokens");
    }

    private static BencodeDictionary ParseDictionary(StreamReader stream)
    {
        var prefix = stream.Read();
        if (prefix != 'd')
            throw new InvalidDataException($"Unexpected token for Dictionary at '{stream.GetPosition()}'");

        var result = new BencodeDictionary();
        while (true)
        {
            prefix = stream.Peek();
            if (stream.EndOfStream)
                throw new EndOfStreamException();
            if (prefix == endOfObject)
            {
                stream.Read();
                break;
            }

            var key = ParseString(stream);
            if (key == "pieces")
            {}
            var value = ParseInternal(stream);
            result.Add(key, value);
        }

        return result;
    }

    private static BencodeString ParseString(StreamReader stream)
    {
        var length = 0;
        int digit;
        while ((digit = stream.Read()) != ':')
        {
            Exceptions.ThrowIfEoF(digit);
            Exceptions.ThrowIfEoF(stream);

            if (!char.IsDigit((char)digit))
                throw new InvalidDataException($"Invalid string length at '{stream.GetPosition()}'");

            length = length * 10 + (digit - '0');
        }

        var buffer = new char[length];
        var read = stream.Read(buffer, 0, length);
        return new BencodeString(new string(buffer));

        //var buffer = new byte[length];
        //var read = stream.BaseStream.Read(buffer, 0, length);

        //Exceptions.ThrowIfEoF(read != length, $"Unexpected end of stream while reading string");

        //return new BencodeString(buffer);
    }

    private static BencodeInt ParseInteger(StreamReader stream)
    {
        var prefix = stream.Read();
        if (prefix != 'i')
            throw new InvalidDataException($"Unexpected token for Integer at '{stream.GetPosition()}'");

        var value = 0;
        int digit;
        bool negative = false;

        var possibleSign = stream.Peek();
        if (possibleSign == '-')
            negative = true;
        if (possibleSign == '-' || possibleSign == '+')
            stream.Read(); // read sign
        if (possibleSign == '0')
            throw new InvalidDataException($"Invalid leading zero in integer at '{stream.GetPosition()}'");

        while ((digit = stream.Read()) != endOfObject)
        {
            Exceptions.ThrowIfEoF(digit);
            Exceptions.ThrowIfEoF(stream);

            if (!char.IsDigit((char)digit))
                throw new InvalidDataException($"Invalid integer at '{stream.GetPosition()}'");

            value = value * 10 + (digit - '0');
        }

        return new BencodeInt(value * (negative ? -1 : 1));
    }

    private static BencodeList ParseList(StreamReader stream)
    {
        var prefix = stream.Read();
        if (prefix != 'l')
            throw new InvalidDataException($"Unexpected token for List at '{stream.GetPosition()}'");

        var result = new BencodeList();
        while (true)
        {
            prefix = stream.Peek();
            if (stream.EndOfStream)
                throw new EndOfStreamException();
            if (prefix == endOfObject)
            {
                stream.Read();
                break;
            }

            var value = ParseInternal(stream);
            result.Add(value);
        }

        return result;
    }
}

public static class Exceptions
{
    public static void ThrowIfEoF(StreamReader stream, string? message = null) => ThrowIf<EndOfStreamException>(stream.EndOfStream, message ?? $"Unexpected end of stream at '{stream.GetPosition()}'");
    public static void ThrowIfEoF(int readByte, string? message = null) => ThrowIf<EndOfStreamException>(readByte < 0, message ?? $"Unexpected end of stream");
    public static void ThrowIfEoF(bool condition, string? message = null) => ThrowIf<EndOfStreamException>(condition, message ?? $"Unexpected end of stream");

    private static void ThrowIf<T>(bool condition, string message) where T : Exception, new()
    {
        if (!condition)
            return;

        try
        {
            throw Activator.CreateInstance(typeof(T), message) as T;
        }
        catch (MissingMethodException ex)
        {
            throw new T();
        }
    }
}

public static class StreamExtensions
{
    public static int GetPosition(this StreamReader s)
    {
        Int32 charpos = (Int32)s.GetType().InvokeMember("_charPos",
            BindingFlags.DeclaredOnly |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.GetField
        , null, s, null);
        Int32 charlen = (Int32)s.GetType().InvokeMember("_charLen",
            BindingFlags.DeclaredOnly |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.GetField
        , null, s, null);
        return (Int32)s.BaseStream.Position - charlen + charpos;
    }
}