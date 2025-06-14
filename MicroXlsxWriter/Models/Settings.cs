namespace MicroXlsxWriter.Models;

/// <summary>
/// Settings for created excelf ile
/// </summary>
public sealed class Settings
{
    public Settings(string? fileName = null, EColumnWidth columnWidth = EColumnWidth.None, string? sheetName = null)
    {
        FileName = fileName;
        ColumnWidth = columnWidth;
        SheetName = sheetName;
    }

    /// <summary>
    /// File name. Random name will be generated if null
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Width of columns
    /// </summary>
    public EColumnWidth ColumnWidth { get; }

    /// <summary>
    /// Sheet name
    /// </summary>
    public string? SheetName { get; }

    public enum EColumnWidth
    {
        /// <summary>
        /// Nothing specified, default width will be used
        /// </summary>
        None,

        /// <summary>
        /// Width will be calculated according to the longest string in each raw
        /// </summary>
        AutoWidth
    }
}
