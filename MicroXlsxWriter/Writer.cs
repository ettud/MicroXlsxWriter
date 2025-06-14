using MicroXlsxWriter.Internal;
using MicroXlsxWriter.Models;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;

namespace MicroXlsxWriter
{
    public static class Writer
    {
        public static async Task<string> Write(RowsSource source,
            CancellationToken cancellationToken)
        {
            return await Write(source, new Settings(), cancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> Write(RowsSource source,
            Settings settings,
            CancellationToken cancellationToken)
        {
            var newFile = settings.FileName ?? 
                (Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".xlsx");
            try
            {
                using var archive = ZipFile.Open(newFile, ZipArchiveMode.Create);
                await WriteCore(archive, source, settings, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                try
                {
                    File.Delete(newFile);
                }
                catch { }
                throw;
            }
            return newFile;
        }

        private static async Task WriteCore(ZipArchive archive,
            RowsSource source,
            Settings settings,
            CancellationToken cancellationToken)
        {
            await WriteCommonFiles(archive, settings, cancellationToken);

            var columnMaxStringLength = settings.ColumnWidth switch
            {
                Settings.EColumnWidth.None => null,
                Settings.EColumnWidth.AutoWidth => new Dictionary<int, uint>(), //starts with 1, not 0
            };
            var rowsCount = 0;
            var maxNumberOfColumns = 0;
            var sharedStringsCount = 0;
            var worksheetPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var sharedStringsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            {
                await using var worksheetFile = File.Open(worksheetPath, FileMode.Create);
                await using var sharedStringsFile = File.Open(sharedStringsPath, FileMode.Create);
                await using var worksheetStreamWriter = new StreamWriter(worksheetFile);
                await using var sharedStringsStreamWriter = new StreamWriter(sharedStringsFile);

                async Task WriteRow(ColumnsSource row)
                {
                    rowsCount++;
                    worksheetStreamWriter.Write(@"<row r=""");
                    worksheetStreamWriter.Write(rowsCount);
                    worksheetStreamWriter.Write(@"""");
                    if (row is EnumerableColumnsSource { Source: { } source } && 
                        source.TryGetNonEnumeratedCount(out var length))
                    {
                        worksheetStreamWriter.Write(@" spans=""");
                        worksheetStreamWriter.Write(1);
                        worksheetStreamWriter.Write(@":");
                        worksheetStreamWriter.Write(length);
                        worksheetStreamWriter.Write(@"""");
                    }
                    worksheetStreamWriter.Write(@">");

                    var numberOfCells = 0;

                    async Task WriteCell(ValueSource cell)
                    {
                        worksheetStreamWriter.Write(@"<c r=""");
                        worksheetStreamWriter.Write((++numberOfCells).GetColumnName());
                        maxNumberOfColumns = Math.Max(maxNumberOfColumns, numberOfCells);
                        worksheetStreamWriter.Write(rowsCount);
                        worksheetStreamWriter.Write(@""" s=""0"" t=""s""><v>");
                        worksheetStreamWriter.Write(sharedStringsCount);
                        worksheetStreamWriter.Write(@"</v></c>");

                        var sharedStringResult = cell.WriteNewSharedString(sharedStringsStreamWriter);
                        if (sharedStringResult is SharedStringsHelper.StringWritten stringWritten)
                        {
                            sharedStringsCount++;
                            if(columnMaxStringLength != null)
                            {
                                if(!columnMaxStringLength.TryGetValue(numberOfCells, out var current))
                                {
                                    current = 0;
                                }
                                current = Math.Max(current, stringWritten.Length);
                                columnMaxStringLength[numberOfCells] = current;
                            }
                        }
                    }

                    switch (row)
                    {
                        case AsyncEnumerableColumnsSource asyncColumnsSource:
                            await foreach (var cell in asyncColumnsSource.Source.WithCancellation(cancellationToken))
                            {
                                await WriteCell(cell);
                            }
                            break;
                        case EnumerableColumnsSource syncColumnsSource:
                            foreach (var cell in syncColumnsSource.Source)
                            {
                                await WriteCell(cell);
                            }
                            break;
                        default:
                            throw new UnreachableException();
                    }
                    worksheetStreamWriter.Write(@"</row>");
                }

                switch (source)
                {
                    case AsyncEnumerableRowsSource asyncRowsSource:
                        await foreach (var row in asyncRowsSource.Source.WithCancellation(cancellationToken))
                        {
                            await WriteRow(row);
                        }
                        break;
                    case EnumerableRowsSource syncRowsSource:
                        foreach (var row in syncRowsSource.Source)
                        {
                            await WriteRow(row);
                        }
                        break;
                    default:
                        throw new UnreachableException();
                }

                sharedStringsStreamWriter.Write("</sst>");
                worksheetStreamWriter.Write(@"</sheetData><headerFooter /></worksheet>");
            }

            {
                var entry = archive.CreateEntry("xl/worksheets/sheet1.xml");
                await using var originalFile = File.Open(worksheetPath, FileMode.Open);
                await using var worksheetFile = entry.Open();

                await using (var worksheetStreamWriter = new StreamWriter(worksheetFile, leaveOpen: true))
                {
                    worksheetStreamWriter.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>");
                    worksheetStreamWriter.Write(@"<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:xdr=""http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"" xmlns:x14=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac xr xr2 xr3"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"" xmlns:xm=""http://schemas.microsoft.com/office/excel/2006/main"" xmlns:xr=""http://schemas.microsoft.com/office/spreadsheetml/2014/revision"" xmlns:xr2=""http://schemas.microsoft.com/office/spreadsheetml/2015/revision2"" xmlns:xr3=""http://schemas.microsoft.com/office/spreadsheetml/2016/revision3"" xr:uid=""{00000000-0001-0000-0000-000000000000}"">");
                    worksheetStreamWriter.Write(@"<dimension ref=""A1");
                    if (rowsCount != 0 || maxNumberOfColumns != 0)
                    {
                        worksheetStreamWriter.Write(@":");
                        if (maxNumberOfColumns == 0)
                        {
                            maxNumberOfColumns = 1;
                        }
                        var column = maxNumberOfColumns.GetColumnName();
                        worksheetStreamWriter.Write(column);
                        if (rowsCount == 0)
                        {
                            rowsCount = 1;
                        }
                        worksheetStreamWriter.Write(rowsCount);
                    }
                    worksheetStreamWriter.Write(@"""/>");
                    worksheetStreamWriter.Write(@"<sheetViews>");
                    worksheetStreamWriter.Write(@"<sheetView workbookViewId=""0"" topLeftCell=""A1""/>");
                    worksheetStreamWriter.Write(@"</sheetViews>");
                    worksheetStreamWriter.Write(@"<sheetFormatPr defaultRowHeight=""15"" />");
                    if(settings.ColumnWidth == Settings.EColumnWidth.AutoWidth)
                    {
                        worksheetStreamWriter.Write(@"<cols>");
                        foreach(var kvp in columnMaxStringLength!)
                        {
                            var width = 0.9M * kvp.Value; //Very simple conversion between string length and string width for Arial 10. The length of the string is used, new lines are ignored
                            worksheetStreamWriter.Write(@$"<col min=""{kvp.Key}"" max=""{kvp.Key}"" width=""{width.ToString("0.000000", new CultureInfo("en-US", false))}"" customWidth=""1""/>");
                        }
                        worksheetStreamWriter.Write(@"</cols>");
                    }
                    worksheetStreamWriter.Write(@"<sheetData>");
                }

                originalFile.CopyTo(worksheetFile);
            }

            {
                var entry = archive.CreateEntry("xl/sharedStrings.xml");
                await using var originalFile = File.Open(sharedStringsPath, FileMode.Open);
                await using var sharedStringsFile = entry.Open();
                await using (var sharedStringsStreamWriter = new StreamWriter(sharedStringsFile, leaveOpen: true))
                {
                    sharedStringsStreamWriter.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?><sst xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" ");
                    sharedStringsStreamWriter.Write(@$"count=""{sharedStringsCount}"" ");
                    sharedStringsStreamWriter.Write(@$"uniqueCount=""{sharedStringsCount}"">"); //not true but we care more about application memory usage rather then file size
                }

                originalFile.CopyTo(sharedStringsFile);
            }
        }


        private static async Task WriteCommonFiles(ZipArchive archive,
            Settings settings,
            CancellationToken cancellationToken)
        {
            {
                var entry = archive.CreateEntry("_rels/.rels");
                await using var relsFile = entry.Open();
                await using var relsStreamWriter = new StreamWriter(relsFile);
                relsStreamWriter.WriteRelsRels();
            }

            {
                var entry = archive.CreateEntry("[Content_Types].xml");
                await using var contentTypesFile = entry.Open();
                await using var contentTypesStreamWriter = new StreamWriter(contentTypesFile);
                contentTypesStreamWriter.WriteContentTypesXml();
            }

            {
                var entry = archive.CreateEntry("xl/workbook.xml");
                await using var workbookFile = entry.Open();
                await using var workbookStreamWriter = new StreamWriter(workbookFile);
                workbookStreamWriter.WriteXlWorkbookXml(settings.SheetName);
            }

            {
                var entry = archive.CreateEntry("xl/_rels/workbook.xml.rels");
                await using var workbookFile = entry.Open();
                await using var workbookStreamWriter = new StreamWriter(workbookFile);
                workbookStreamWriter.WriteXlRelsWorkbookXmlRels();
            }
        }
    }
}
