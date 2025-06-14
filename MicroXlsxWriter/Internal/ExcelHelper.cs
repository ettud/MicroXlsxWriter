namespace MicroXlsxWriter.Internal;

internal static class ExcelHelper
{
    public static string GetColumnName(this int columnNumber)
    {
        const int NUMBER_OF_LETTERS = 'Z' - 'A' + 1;

        var dividend = columnNumber;
        var columnName = "";
        int modulo;

        while (dividend > 0)
        {
            modulo = (dividend - 1) % NUMBER_OF_LETTERS;
            var val = (char)('A' + modulo);
            columnName += val;
            dividend = (dividend - modulo) / NUMBER_OF_LETTERS;
        }

        return columnName;
    }
}
