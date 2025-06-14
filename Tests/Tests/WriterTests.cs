using FluentAssertions;
using MicroXlsxWriter;
using MicroXlsxWriter.Models;
using System.Text;

namespace Tests.Tests
{
    public class WriterTests
    {
        [Fact]
        public async Task SuccessAsync()
        {
            // Arrange
            async IAsyncEnumerable<ColumnsSource> Source()
            {
                async IAsyncEnumerable<ValueSource> Row1()
                {
                    yield return ValueSource.Create((string?)null);
                    yield return ValueSource.Create("String");
                }

                IEnumerable<ValueSource> Row2()
                {
                    yield return ValueSource.CreateNull();
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello ");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check &");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                }

                yield return ColumnsSource.Create(Row1());
                yield return ColumnsSource.Create(Row2());
            }

            RowsSource source = RowsSource.Create(Source());

            // Act
            var path = await Writer.Write(source, CancellationToken.None);

            // Assert
            await VerifyHelper.Verify(path);
        }

        [Fact]
        public async Task SuccessSync()
        {
            // Arrange
            async IAsyncEnumerable<ColumnsSource> Source()
            {
                async IAsyncEnumerable<ValueSource> Row1()
                {
                    yield return ValueSource.Create((string?)null);
                    yield return ValueSource.Create("String");
                    yield return ValueSource.Create("Str <");
                    yield return ValueSource.Create("Str >");
                    yield return ValueSource.Create("Str \\");
                    yield return ValueSource.Create("Str '");
                    yield return ValueSource.Create("Str &");
                }

                IEnumerable<ValueSource> Row2()
                {
                    yield return ValueSource.CreateNull();
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello ");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello <");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello >");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello \\");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello '");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var sb = new StringBuilder();
                        sb.Append("Hello &");
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check &");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check ");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check &");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check '");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check \\");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check >");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                    {
                        var ms = new MemoryStream();
                        {
                            using var sw = new StreamWriter(ms, leaveOpen: true);
                            sw.WriteLine("Check < very long string kind of");
                        }
                        ms.Seek(0, SeekOrigin.Begin);
                        using var sr = new StreamReader(ms);
                        yield return ValueSource.Create(sr);
                    }
                }

                yield return ColumnsSource.Create(Row1());
                yield return ColumnsSource.Create(Row2());
            }

            RowsSource source = RowsSource.Create(Source());
            var settings = new Settings(columnWidth: Settings.EColumnWidth.AutoWidth, sheetName: "Some name");

            // Act
            var path = await Writer.Write(source, settings, CancellationToken.None);

            // Assert
            await VerifyHelper.Verify(path);
        }


        [Fact]
        public async Task Error()
        {
            // Arrange
            async IAsyncEnumerable<ColumnsSource> Source()
            {
                async IAsyncEnumerable<ValueSource> Row1()
                {
                    throw new Exception("Very specific exception");
                    yield break;
                }

                yield return ColumnsSource.Create(Row1());
            }

            RowsSource source = RowsSource.Create(Source());

            // Act
            Exception? error = null;
            try
            {
                var path = await Writer.Write(source, CancellationToken.None);
            }
            catch(Exception ex)
            {
                error = ex;
            }

            // Assert
            error?.Message.Should().Be("Very specific exception");
        }
    }
}