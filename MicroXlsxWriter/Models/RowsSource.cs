namespace MicroXlsxWriter.Models;

/// <summary>
/// Source of row models for table
/// </summary>
public abstract class RowsSource
{
    protected RowsSource() { }

    public static RowsSource Create(IAsyncEnumerable<ColumnsSource> source)
    {
        return new AsyncEnumerableRowsSource(source);
    }

    public static RowsSource Create(IEnumerable<ColumnsSource> source)
    {
        return new EnumerableRowsSource(source);
    }
}

internal class AsyncEnumerableRowsSource : RowsSource
{
    public AsyncEnumerableRowsSource(IAsyncEnumerable<ColumnsSource> source) : base()
    {
        Source = source;
    }

    public IAsyncEnumerable<ColumnsSource> Source { get; }
}

internal class EnumerableRowsSource : RowsSource
{
    public EnumerableRowsSource(IEnumerable<ColumnsSource> source) : base()
    {
        Source = source;
    }

    public IEnumerable<ColumnsSource> Source { get; }
}