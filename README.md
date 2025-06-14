# MicroXlsxWriter

The aim of the library is to simply create tables containing text values only in excel format. No pictures, fonts, no colors. Just text values without storing a lot of information in memory while creating the file. Simple, fast, with minimal memory usage. That's why there is no mapper here: not this library problem. It just writes text to xlsx.

```
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
                        //any big string can be built, no .ToString() will be called in library
                        yield return ValueSource.Create(sb);
                    }
                    {
                        var ms = new MemoryStream(); //or use any file instead of memory stream, no .ReadToEnd() will be called in library
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

            var path = await Writer.Write(RowsSource.Create(Source()),
                new Settings(columnWidth: Settings.EColumnWidth.AutoWidth, sheetName: "Some name"),
                CancellationToken.None);
```