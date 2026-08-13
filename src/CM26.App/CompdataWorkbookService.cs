using System.Data;
using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CM26.App;

internal sealed class CompdataWorkbookService
{
    public string FilePath { get; private set; } = string.Empty;
    private string _gameFolderCompdataPath = string.Empty;
    public IReadOnlyList<string> SheetNames { get; private set; } = [];

    public void Open(string filePath)
    {
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException("Workbook data is missing.");
        var workbook = workbookPart.Workbook
            ?? throw new InvalidDataException("Workbook definition is missing.");
        var sheets = workbook.Sheets?.Elements<Sheet>()
            .Select(sheet => sheet.Name?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray() ?? [];
        if (sheets.Length == 0)
            throw new InvalidDataException("The workbook has no worksheets.");
        FilePath = Path.GetFullPath(filePath);
        SheetNames = sheets;
    }

    public void OpenFromGameFolder(string compdataPath)
    {
        var subdirs = Directory.GetDirectories(compdataPath)
            .Select(d => Path.GetFileName(d))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();
        if (subdirs.Length == 0)
            throw new InvalidDataException("No subdirectories found in the compdata folder.");
        FilePath = string.Empty;
        _gameFolderCompdataPath = compdataPath;
        SheetNames = subdirs;
    }

    public DataTable ReadSheet(string sheetName)
    {
        if (string.IsNullOrWhiteSpace(FilePath) && string.IsNullOrWhiteSpace(_gameFolderCompdataPath))
            throw new InvalidOperationException("Open a Compdata workbook first.");
        if (!string.IsNullOrWhiteSpace(_gameFolderCompdataPath))
            return ReadTxtSheet(sheetName);
        using var document = SpreadsheetDocument.Open(FilePath, false);
        var worksheetPart = FindWorksheet(document, sheetName);
        var shared = document.WorkbookPart?.SharedStringTablePart?.SharedStringTable;
        var worksheet = worksheetPart.Worksheet
            ?? throw new InvalidDataException($"Worksheet '{sheetName}' is empty.");
        var rows = worksheet.GetFirstChild<SheetData>()?
            .Elements<Row>().ToArray() ?? [];
        var width = rows.SelectMany(row => row.Elements<Cell>())
            .Select(cell => ColumnIndex(cell.CellReference?.Value))
            .DefaultIfEmpty(0).Max() + 1;
        width = Math.Max(width, CompdataSchema.GetColumns(sheetName, width).Length);

        var values = rows.Select(row => ReadRow(row, width, shared)).ToList();
        ResolveReferenceFormulas(document, values, shared);
        var table = new DataTable(sheetName);
        var header = CompdataSchema.HasFixedColumns(sheetName)
            ? CompdataSchema.GetColumns(sheetName, width)
            : values.Count > 0 ? values[0] : Enumerable.Repeat(string.Empty, width).ToArray();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var column = 0; column < width; column++)
        {
            var name = string.IsNullOrWhiteSpace(header[column])
                ? $"Column {column + 1}"
                : header[column].Trim();
            var unique = name;
            for (var suffix = 2; !usedNames.Add(unique); suffix++)
                unique = $"{name} ({suffix})";
            table.Columns.Add(unique, typeof(string));
        }
        foreach (var source in CompdataSchema.HasFixedColumns(sheetName) ? values : values.Skip(1))
            table.Rows.Add(source.Cast<object>().ToArray());
        return table;
    }

    private DataTable ReadTxtSheet(string sheetName)
    {
        var sheetDir = Path.Combine(_gameFolderCompdataPath, sheetName);
        if (!Directory.Exists(sheetDir))
            throw new InvalidDataException($"Compdata subdirectory '{sheetName}' was not found.");
        var txtFiles = Directory.GetFiles(sheetDir, "*.txt").OrderBy(f => f).ToArray();
        if (txtFiles.Length == 0)
            throw new InvalidDataException($"No .txt files found in '{sheetName}'.");
        var allRows = new List<string[]>();
        foreach (var file in txtFiles)
        {
            foreach (var line in File.ReadAllLines(file, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                allRows.Add(ParseCsvLine(line));
            }
        }
        if (allRows.Count == 0)
            throw new InvalidDataException($"The '{sheetName}' folder contains no data rows.");
        var width = allRows.Max(r => r.Length);
        var columns = CompdataSchema.GetColumns(sheetName, width);
        var table = new DataTable(sheetName);
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var column = 0; column < width; column++)
        {
            var name = column < columns.Length ? columns[column] : $"Column {column + 1}";
            var unique = name;
            for (var suffix = 2; !usedNames.Add(unique); suffix++)
                unique = $"{name} ({suffix})";
            table.Columns.Add(unique, typeof(string));
        }
        foreach (var source in allRows)
        {
            var padded = new string[width];
            Array.Copy(source, padded, Math.Min(source.Length, width));
            table.Rows.Add(padded.Cast<object>().ToArray());
        }
        return table;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    public void SaveCopy(string outputPath, IReadOnlyDictionary<string, DataTable> tables)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new InvalidOperationException("Open a Compdata workbook first.");
        File.Copy(FilePath, outputPath, overwrite: true);
        using var document = SpreadsheetDocument.Open(outputPath, true);
        foreach (var (sheetName, table) in tables)
            WriteSheet(document, sheetName, table);
    }

    /// <summary>
    /// Exports the Compdata text interchange used by the supplied STOP tool:
    /// one UTF-8 comma-separated .txt file per worksheet, no header row and a
    /// single space for an otherwise empty value.
    /// </summary>
    public static void ExportTextFiles(string outputDirectory, IReadOnlyDictionary<string, DataTable> tables)
    {
        Directory.CreateDirectory(outputDirectory);
        foreach (var (sheetName, table) in tables)
        {
            var path = Path.Combine(outputDirectory, SafeFileName(sheetName) + ".txt");
            using var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            foreach (DataRow row in table.Rows)
            {
                var values = table.Columns.Cast<DataColumn>()
                    .Select(column => CsvValue(Convert.ToString(row[column], CultureInfo.InvariantCulture)))
                    .ToArray();
                writer.WriteLine(string.Join(',', values));
            }
        }
    }

    private static void WriteSheet(
        SpreadsheetDocument document, string sheetName, DataTable table)
    {
        var worksheetPart = FindWorksheet(document, sheetName);
        var worksheet = worksheetPart.Worksheet
            ?? throw new InvalidDataException($"Worksheet '{sheetName}' is empty.");
        var sheetData = worksheet.GetFirstChild<SheetData>()
            ?? worksheet.AppendChild(new SheetData());
        sheetData.RemoveAllChildren<Row>();

        var hasFixedColumns = CompdataSchema.HasFixedColumns(sheetName);
        if (!hasFixedColumns)
        {
            var header = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            AppendRow(sheetData, 1, header, preserveText: true);
        }
        for (var row = 0; row < table.Rows.Count; row++)
        {
            var values = table.Columns.Cast<DataColumn>()
                .Select(column => Convert.ToString(table.Rows[row][column], CultureInfo.InvariantCulture) ?? string.Empty)
                .ToArray();
            AppendRow(sheetData, (uint)row + (hasFixedColumns ? 1U : 2U), values, preserveText: false);
        }
        worksheet.Save();
    }

    private static WorksheetPart FindWorksheet(SpreadsheetDocument document, string sheetName)
    {
        var workbook = document.WorkbookPart
            ?? throw new InvalidDataException("Workbook data is missing.");
        var definition = workbook.Workbook
            ?? throw new InvalidDataException("Workbook definition is missing.");
        var sheet = definition.Sheets?.Elements<Sheet>()
            .FirstOrDefault(item => string.Equals(item.Name?.Value, sheetName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException($"Worksheet '{sheetName}' was not found.");
        return (WorksheetPart)workbook.GetPartById(sheet.Id!);
    }

    private static string[] ReadRow(Row row, int width, SharedStringTable? shared)
    {
        var result = Enumerable.Repeat(string.Empty, width).ToArray();
        foreach (var cell in row.Elements<Cell>())
        {
            var index = ColumnIndex(cell.CellReference?.Value);
            if (index < 0 || index >= width) continue;
            result[index] = CellText(cell, shared);
        }
        return result;
    }

    private static string CellText(Cell cell, SharedStringTable? shared)
    {
        if (cell.DataType?.Value == CellValues.InlineString)
            return cell.InlineString?.Text?.Text ?? cell.InlineString?.InnerText ?? string.Empty;
        var formula = cell.CellFormula?.Text;
        if (!string.IsNullOrWhiteSpace(formula)) return "=" + formula;
        var raw = cell.CellValue?.Text ?? cell.InnerText ?? string.Empty;
        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(raw, out var index) && shared != null && index >= 0 && index < shared.Count())
            return shared.ElementAt(index).InnerText;
        if (cell.DataType?.Value == CellValues.Boolean)
            return raw == "1" ? "TRUE" : "FALSE";
        return raw;
    }

    // The supplied Compdata workbook uses direct formulas such as
    // =compobj!A373 for object IDs. OpenXML does not guarantee a cached value,
    // so resolve these one-cell references before validating/exporting data.
    private static void ResolveReferenceFormulas(
        SpreadsheetDocument document, IReadOnlyList<string[]> rows, SharedStringTable? shared)
    {
        foreach (var row in rows)
        for (var column = 0; column < row.Length; column++)
        {
            var formula = row[column];
            if (!TryParseReference(formula, out var sheetName, out var cellReference)) continue;
            try
            {
                var worksheet = FindWorksheet(document, sheetName).Worksheet;
                var source = worksheet?.Descendants<Cell>()
                    .FirstOrDefault(cell => string.Equals(cell.CellReference?.Value, cellReference, StringComparison.OrdinalIgnoreCase));
                if (source == null) continue;
                var value = CellText(source, shared);
                if (!value.StartsWith("=", StringComparison.Ordinal)) row[column] = value;
            }
            catch (Exception ex) { Program.Log($"Compdata formula resolution failed: {ex.Message}"); }
        }
    }

    private static bool TryParseReference(string value, out string sheetName, out string cellReference)
    {
        sheetName = string.Empty;
        cellReference = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("=", StringComparison.Ordinal))
            return false;
        var bang = value.IndexOf('!');
        if (bang is < 2 or >= 64) return false;
        sheetName = value[1..bang].Trim('\'');
        cellReference = value[(bang + 1)..].Replace("$", string.Empty, StringComparison.Ordinal);
        return cellReference.Length > 1 && cellReference.All(character => char.IsLetterOrDigit(character));
    }

    private static int ColumnIndex(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return 0;
        var index = 0;
        foreach (var c in reference)
        {
            if (!char.IsLetter(c)) break;
            index = (index * 26) + (char.ToUpperInvariant(c) - 'A' + 1);
        }
        return Math.Max(0, index - 1);
    }

    private static string CellReference(int column, uint row)
    {
        var name = string.Empty;
        for (var value = column + 1; value > 0; value = (value - 1) / 26)
            name = (char)('A' + ((value - 1) % 26)) + name;
        return name + row.ToString(CultureInfo.InvariantCulture);
    }

    private static void AppendRow(
        SheetData sheetData, uint rowIndex, IReadOnlyList<string> values, bool preserveText)
    {
        var row = new Row { RowIndex = rowIndex };
        for (var column = 0; column < values.Count; column++)
        {
            var value = values[column] ?? string.Empty;
            var numeric = !preserveText && IsSafeNumber(value);
            var cell = numeric
                ? new Cell
                {
                    CellReference = CellReference(column, rowIndex),
                    DataType = CellValues.Number,
                    CellValue = new CellValue(value),
                }
                : new Cell
                {
                    CellReference = CellReference(column, rowIndex),
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(value)
                    {
                        Space = SpaceProcessingModeValues.Preserve,
                    }),
                };
            row.Append(cell);
        }
        sheetData.Append(row);
    }

    private static bool IsSafeNumber(string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            return false;
        var trimmed = value.TrimStart('-', '+');
        return trimmed.Length <= 1 || trimmed[0] != '0' || trimmed.StartsWith("0.", StringComparison.Ordinal);
    }

    private static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private static string CsvValue(string? value)
    {
        var result = string.IsNullOrEmpty(value) ? " " : value;
        return result.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? '"' + result.Replace("\"", "\"\"") + '"'
            : result;
    }
}
