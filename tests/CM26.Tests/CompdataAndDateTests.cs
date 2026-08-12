using System.Data;
using CM26.App;
using CM26.Application.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CM26.Tests;

public sealed class CompdataAndDateTests
{
    [Fact]
    public void CompdataSchemaValidatesParentReferences()
    {
        var table = new DataTable("compobj");
        foreach (var column in CompdataSchema.GetColumns("compobj", 5)) table.Columns.Add(column);
        table.Rows.Add("1", "0", "World", "World", "-1");
        table.Rows.Add("2", "4", "Cup", "Cup", "1");
        var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase) { ["compobj"] = table };
        Assert.Empty(CompdataSchema.Validate(tables));
        table.Rows.Add("3", "4", "Broken", "Broken", "999");
        Assert.Contains(CompdataSchema.Validate(tables), issue => issue.IsError && issue.Message.Contains("does not exist"));
    }

    [Fact]
    public void CompdataWorkbookRoundTripsFixedColumnSheet()
    {
        var directory = Path.Combine(Path.GetTempPath(), "cm26-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "source.xlsx");
            CreateWorkbook(source);
            var service = new CompdataWorkbookService();
            service.Open(source);
            var table = service.ReadSheet("compobj");
            Assert.Single(table.Rows);
            table.Rows[0][2] = "Updated";
            var copy = Path.Combine(directory, "copy.xlsx");
            service.SaveCopy(copy, new Dictionary<string, DataTable> { ["compobj"] = table });
            var verify = new CompdataWorkbookService();
            verify.Open(copy);
            Assert.Equal("Updated", verify.ReadSheet("compobj").Rows[0][2]);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("1582-10-14", "0")]
    [InlineData("2000-01-01", "152385")]
    public void FifaDateConverterRoundTripsIso(string iso, string raw)
    {
        Assert.True(FifaDateConverter.TryFromIso(iso, out var encoded));
        Assert.Equal(raw, encoded);
        Assert.True(FifaDateConverter.TryToIso(encoded, out var decoded));
        Assert.Equal(iso, decoded);
    }

    [Theory]
    [InlineData("1582-10-13")]
    [InlineData("not-a-date")]
    [InlineData("")]
    public void FifaDateConverterRejectsInvalidBoundaries(string iso) =>
        Assert.False(FifaDateConverter.TryFromIso(iso, out _));

    private static void CreateWorkbook(string path)
    {
        using var document = SpreadsheetDocument.Create(path, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        var data = new SheetData();
        var row = new Row();
        foreach (var value in new[] { "1", "0", "World", "Root", "-1" })
            row.Append(new Cell { DataType = CellValues.InlineString, InlineString = new InlineString(new Text(value)) });
        data.Append(row);
        worksheetPart.Worksheet = new Worksheet(data);
        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        sheets.Append(new Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "compobj" });
        workbookPart.Workbook.Save();
    }
}
