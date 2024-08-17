using System.Reflection;
using System.Text;

namespace PeerSharp.Bencode;

public class BencodeParserV2
{
    private const char endOfObject = 'e';

    public static BencodeToken Parse(Stream stream)
    {
        using var sr = new BencodeStreamReader(stream);
        return ParseInternal(sr);
    }

    private static BencodeToken ParseInternal(BencodeStreamReader stream)
    {
        int prefix;
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
                        throw new NotSupportedException($"Unknown bencode type '{type}'[{prefix}] at {stream.Position}");
            }
        }

        throw new EndOfStreamException("Unexpected end of stream while parsing tokens");
    }

    private static BencodeDictionary ParseDictionary(BencodeStreamReader stream)
    {
        var prefix = stream.Read();
        if (prefix != 'd')
            throw new InvalidDataException($"Unexpected token for Dictionary at '{stream.Position}'");

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
            var value = ParseInternal(stream);
            result.Add(key, value);
        }

        return result;
    }

    private static BencodeString ParseString(BencodeStreamReader stream)
    {
        var length = 0;
        int digit;
        while ((digit = stream.Read()) != ':')
        {
            Exceptions.ThrowIfEoF(digit);
            Exceptions.ThrowIfEoF(stream);

            if (!char.IsDigit((char)digit))
                throw new InvalidDataException($"Invalid string length at '{stream.Position}'");

            length = length * 10 + (digit - '0');
        }

        var buffer = new byte[length];
        var read = stream.Read(buffer, 0, length);

        Exceptions.ThrowIfEoF(read != length, $"Unexpected end of stream while reading string");

        return new BencodeString(buffer);
    }

    private static BencodeInt ParseInteger(BencodeStreamReader stream)
    {
        var prefix = stream.Read();
        if (prefix != 'i')
            throw new InvalidDataException($"Unexpected token for Integer at '{stream.Position}'");

        var value = 0;
        int digit;
        bool negative = false;

        var possibleSign = stream.Peek();
        if (possibleSign == '-')
            negative = true;
        if (possibleSign == '-' || possibleSign == '+')
            stream.Read(); // read sign
        if (possibleSign == '0')
            throw new InvalidDataException($"Invalid leading zero in integer at '{stream.Position}'");

        while ((digit = stream.Read()) != endOfObject)
        {
            Exceptions.ThrowIfEoF(digit);
            Exceptions.ThrowIfEoF(stream);

            if (!char.IsDigit((char)digit))
                throw new InvalidDataException($"Invalid integer at '{stream.Position}'");

            value = value * 10 + (digit - '0');
        }

        return new BencodeInt(value * (negative ? -1 : 1));
    }

    private static BencodeList ParseList(BencodeStreamReader stream)
    {
        var prefix = stream.Read();
        if (prefix != 'l')
            throw new InvalidDataException($"Unexpected token for List at '{stream.Position}'");

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

internal static class Exceptions
{
    public static void ThrowIfEoF(BencodeStreamReader stream, string? message = null) => ThrowIf<EndOfStreamException>(stream.EndOfStream, message ?? $"Unexpected end of stream at '{stream.Position}'");
    public static void ThrowIfEoF(int readByte, string? message = null) => ThrowIf<EndOfStreamException>(readByte < 1, message ?? $"Unexpected end of stream");
    public static void ThrowIfEoF(bool condition, string? message = null) => ThrowIf<EndOfStreamException>(condition, message ?? $"Unexpected end of stream");

    private static void ThrowIf<T>(bool condition, string message) where T : Exception, new()
    {
        if (!condition)
            return;
        T exception;
        try
        {
            exception = Activator.CreateInstance(typeof(T), message) as T ?? new T();
        }
        catch (MissingMethodException)
        {
            exception = new T();
        }
        throw exception;
    }
}