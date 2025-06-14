namespace MicroXlsxWriter.Internal;

internal static class ConstantFileHelper
{
    public static void WriteXlWorkbookXml(this StreamWriter sw,
        string? sheetName = null)
    {
        sw.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""><fileVersion appName=""xl"" lastEdited=""5"" lowestEdited=""5"" rupBuild=""9302""/><workbookPr defaultThemeVersion=""124226""/><bookViews><workbookView xWindow=""870"" yWindow=""855"" windowWidth=""27255"" windowHeight=""11415""/>
</bookViews>
<sheets>
<sheet name=""");
        sw.Write(sheetName ?? "Sheet1");
        sw.Write(@""" sheetId=""1"" r:id=""rId1""/>
</sheets>
<definedNames/>
<calcPr calcId=""0""/><fileRecoveryPr repairLoad=""1""/>
</workbook>");
    }

    public static void WriteContentTypesXml(this StreamWriter sw)
    {
        sw.Write(
            @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
<Default Extension=""xml"" ContentType=""application/xml""/>
<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
<Override PartName=""/xl/sharedStrings.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml""/>
</Types>");
    }

    public static void WriteRelsRels(this StreamWriter sw)
    {
        sw.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");
    }

    public static void WriteXlRelsWorkbookXmlRels(this StreamWriter sw)
    {
        sw.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">");
        sw.Write(@"<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>");
        sw.Write(@"<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings"" Target=""sharedStrings.xml"" />");
        sw.Write(@"</Relationships>");
    }
}
