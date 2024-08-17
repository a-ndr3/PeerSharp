﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;


namespace PeerSharp.Bencode;

public abstract class BencodeToken
{
    public abstract TokenType Type { get; }

    public enum TokenType
    {
        Unknown = 0,
        Integer,
        List,
        Dictionary,
        String,
    }
}

public class BencodeString(string value) : BencodeToken
{
    public override TokenType Type { get; } = TokenType.String;
    public string Value { get; } = value;

    public static implicit operator string(BencodeString str) => str.Value;
}

//public class BencodeString(byte[] data) : BencodeToken
//{
//    public override TokenType Type { get; } = TokenType.String;
//    public byte[] Data { get; } = data;

//    public static implicit operator string(BencodeString str) => str.ToString();

//    private string? stringValue = null;
//    public override string ToString()
//    {
//        stringValue ??= Encoding.UTF8.GetString(Data);
//        return stringValue;
//    }
//}

public class BencodeInt(long value) : BencodeToken
{
    public override TokenType Type { get; } = TokenType.Integer;
    public long Value => value;
    public static implicit operator long(BencodeInt val) => val.Value;
}

public class BencodeDictionary : BencodeToken, IDictionary<string, BencodeToken>
{
    public override TokenType Type { get; } = TokenType.Dictionary;
    private Dictionary<string, BencodeToken> dict = new();

    public BencodeToken this[string key] { get => dict[key]; set => dict[key] = value; }

    public ICollection<string> Keys => dict.Keys;

    public ICollection<BencodeToken> Values => dict.Values;

    public int Count => dict.Count;

    public bool IsReadOnly => false;

    public void Add(string key, BencodeToken value) => dict.Add(key, value);

    public void Add(KeyValuePair<string, BencodeToken> item) => dict.Add(item.Key, item.Value);

    public void Clear() => dict.Clear();

    public bool Contains(KeyValuePair<string, BencodeToken> item) => dict.Contains(item);

    public bool ContainsKey(string key) => dict.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, BencodeToken>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, BencodeToken>>)dict).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, BencodeToken>> GetEnumerator() => dict.GetEnumerator();

    public bool Remove(string key) => dict.Remove(key);

    public bool Remove(KeyValuePair<string, BencodeToken> item) => dict.Remove(item.Key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out BencodeToken value) => dict.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
}

public class BencodeList : BencodeToken, IList<BencodeToken>
{
    public override TokenType Type { get; } = TokenType.List;
    private List<BencodeToken> list = new();

    public BencodeToken this[int index] { get => list[index]; set => list[index] = value; }

    public int Count => list.Count;

    public bool IsReadOnly => false;

    public void Add(BencodeToken item) => list.Add(item);

    public void Clear() => list.Clear();

    public bool Contains(BencodeToken item) => list.Contains(item);

    public void CopyTo(BencodeToken[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

    public IEnumerator<BencodeToken> GetEnumerator() => list.GetEnumerator();

    public int IndexOf(BencodeToken item) => list.IndexOf(item);

    public void Insert(int index, BencodeToken item) => list.Insert(index, item);

    public bool Remove(BencodeToken item) => list.Remove(item);

    public void RemoveAt(int index) => list.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}