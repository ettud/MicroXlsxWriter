namespace MicroXlsxWriter.Models;

/// <summary>
/// Source of column values for row
/// </summary>
public abstract class ColumnsSource
{
    protected ColumnsSource() { }

    public static ColumnsSource Create(IAsyncEnumerable<ValueSource> source)
    {
        return new AsyncEnumerableColumnsSource(source);
    }

    public static ColumnsSource Create(IEnumerable<ValueSource> source)
    {
        return new EnumerableColumnsSource(source);
    }
}

internal class AsyncEnumerableColumnsSource : ColumnsSource
{
    public AsyncEnumerableColumnsSource(IAsyncEnumerable<ValueSource> source)
    {
        Source = source;
    }

    public IAsyncEnumerable<ValueSource> Source { get; }
}

internal class EnumerableColumnsSource : ColumnsSource
{
    public EnumerableColumnsSource(IEnumerable<ValueSource> source)
    {
        Source = source;
    }

    public IEnumerable<ValueSource> Source { get; }
}