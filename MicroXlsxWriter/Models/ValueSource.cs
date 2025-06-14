using System.Text;

namespace MicroXlsxWriter.Models;

/// <summary>
/// Source of values
/// </summary>
public abstract class ValueSource
{
    protected ValueSource() { }

    /// <summary>
    /// Empty cell
    /// </summary>
    public static ValueSource CreateNull() => new StringSource(null);

    /// <summary>
    /// String value
    /// </summary>
    public static ValueSource Create(string? value) => new StringSource(value);

    /// <summary>
    /// Value from string builder
    /// </summary>
    public static ValueSource Create(StringBuilder value) => new StringBuilderSource(value);

    /// <summary>
    /// Value from stream
    /// </summary>
    public static ValueSource Create(StreamReader value) => new StreamReaderSource(value);
}

internal sealed class StringSource : ValueSource
{
    public StringSource(string? value)
    {
        Value = value;
    }

    public string? Value { get; }
}

internal sealed class StringBuilderSource : ValueSource
{
    public StringBuilderSource(StringBuilder value)
    {
        Value = value;
    }

    public StringBuilder Value { get; }
}

internal sealed class StreamReaderSource : ValueSource
{
    public StreamReaderSource(StreamReader value)
    {
        Value = value;
    }

    public StreamReader Value { get; }
}
