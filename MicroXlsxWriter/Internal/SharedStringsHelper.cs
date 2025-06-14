using MicroXlsxWriter.Models;
using System.Diagnostics;

namespace MicroXlsxWriter.Internal;

internal static class SharedStringsHelper
{
    public abstract record Result();
    public sealed record StringWritten(uint Length) : Result;
    public sealed record StringIgnored() : Result;

    private static IReadOnlyDictionary<char, string> Replacements => new Dictionary<char, string>
    {
        { '&', "&amp;" },
        { '<', "&lt;" },
        { '>', "&gt;" },
        { '\\', "&quot;" },
        { '\'', "&apos;" },
    };

    public static Result WriteNewSharedString(this ValueSource value,
        StreamWriter sharedStringsStreamWriter)
    {
        var writeFunction = value.WriteNewSharedStringCore(sharedStringsStreamWriter);
        if (writeFunction == null) {
            return new StringIgnored();
        }
        sharedStringsStreamWriter.Write(@"<si><t");
        var length = writeFunction();
        sharedStringsStreamWriter.Write(@"</t></si>");
        return new StringWritten(length);
    }

    private static Func<uint>? WriteNewSharedStringCore(this ValueSource value,
        StreamWriter sharedStringsStreamWriter)
    {
        switch (value)
        {
            case StringSource strSource:
                {
                    var str = strSource.Value;
                    if (string.IsNullOrEmpty(str))
                    {
                        return null;
                    }
                    return () =>
                    {
                        var length = str.Length;
                        if (str.EndsWith(' '))
                        {
                            sharedStringsStreamWriter.Write(@" xml:space=""preserve""");
                        }
                        sharedStringsStreamWriter.Write(@">");
                        foreach(var replacement in Replacements)
                        {
                            str = str.Replace(replacement.Key.ToString(), replacement.Value);
                        }
                        sharedStringsStreamWriter.Write(str);
                        return (uint)length;
                    };
                }
            case StringBuilderSource strBldrSource:
                {
                    return () =>
                    {
                        var stringBuilder = strBldrSource.Value;
                        var length = stringBuilder.Length;

                        if (stringBuilder.Length > 0 && stringBuilder[length - 1] == ' ')
                        {
                            sharedStringsStreamWriter.Write(@" xml:space=""preserve""");
                        }
                        foreach (var replacement in Replacements)
                        {
                            stringBuilder = stringBuilder.Replace(replacement.Key.ToString(), replacement.Value);
                        }
                        sharedStringsStreamWriter.Write(@">");
                        sharedStringsStreamWriter.Write(stringBuilder);
                        return (uint)length;
                    };
                }
            case StreamReaderSource strRdrSource:
                {
                    return () =>
                    {
                        var replacements = Replacements;
                        var badCharacters = replacements.Keys.ToList();
                        sharedStringsStreamWriter.Write(@">");
                        uint totalLength = 0;
                        var buffer = new char[1024];
                        int len;
                        while ((len = strRdrSource.Value.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalLength += (uint)len;
                            if (!buffer.Any(c => badCharacters.Contains(c)))
                            {
                                sharedStringsStreamWriter.Write(buffer, 0, len);
                                continue;
                            }

                            var secondBuffer = new List<char>(buffer.Length);
                            for (var i = 0; i < len; i++)
                            {
                                if (!badCharacters.Contains(buffer[i]))
                                {
                                    secondBuffer.Add(buffer[i]);
                                    continue;
                                }
                                else
                                {
                                    sharedStringsStreamWriter.Write([.. secondBuffer], 0, secondBuffer.Count);
                                    var replacement = replacements[buffer[i]];
                                    sharedStringsStreamWriter.Write(replacement.ToCharArray(), 0, replacement.Length);
                                    secondBuffer = new List<char>(buffer.Length);
                                }
                            }
                            if (secondBuffer.Count != 0)
                            {
                                sharedStringsStreamWriter.Write([.. secondBuffer], 0, secondBuffer.Count);
                            }
                        }
                        return totalLength;
                    };
                }
            default:
                throw new UnreachableException();
        }
    }
}
