using System.Text;
using System.Text.Json;
using CM26.Application.Models;

namespace CM26.Application.Services;

/// <summary>
/// Safe DBM/RDBM-style table interchange for CM26. Exported files carry an
/// explicit row index, so importing can stage scalar edits without guessing a
/// primary key or performing an unsafe structural rewrite.
/// </summary>
public static class TableWorkspaceService
{
    public const string RowIndexColumn = "__rowindex";

    public static void ExportTable(DatabaseSession session, string tableName, string path)
    {
        var table = session.GetTable(tableName)
            ?? throw new InvalidOperationException($"Table '{tableName}' was not found.");
        var delimiter = DelimiterFor(path);
        using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
        WriteRow(writer, new[] { RowIndexColumn }.Concat(table.Columns.Select(c => c.Name)), delimiter);
        for (var rowIndex = 0; rowIndex < table.RowCount; rowIndex++)
        {
            var record = session.GetRecord(table.Name, rowIndex);
            if (record == null) continue;
            WriteRow(writer, new[] { rowIndex.ToString() }.Concat(record.Values), delimiter);
        }
    }

    public static IReadOnlyList<TableImportEdit> BuildImportPlan(
        DatabaseSession session,
        string tableName,
        string path)
    {
        var table = session.GetTable(tableName)
            ?? throw new InvalidOperationException($"Table '{tableName}' was not found.");
        var rows = Parse(File.ReadAllText(path), DelimiterFor(path));
        if (rows.Count == 0) return Array.Empty<TableImportEdit>();

        var header = rows[0];
        var rowIndexColumn = FindColumn(header, RowIndexColumn);
        if (rowIndexColumn < 0)
            throw new InvalidDataException($"Import requires the '{RowIndexColumn}' column produced by CM26 Export.");

        var mapped = header
            .Select((name, index) => (Column: table.FindColumn(name), Index: index))
            .Where(item => item.Column != null && item.Column.IsWritable)
            .Select(item => (Column: item.Column!, item.Index))
            .ToArray();
        if (mapped.Length == 0)
            throw new InvalidDataException("The file does not contain any writable fields from the selected table.");

        var edits = new List<TableImportEdit>();
        for (var line = 1; line < rows.Count; line++)
        {
            var values = rows[line];
            if (values.All(string.IsNullOrEmpty)) continue;
            if (rowIndexColumn >= values.Count || !int.TryParse(values[rowIndexColumn], out var rowIndex) ||
                rowIndex < 0 || rowIndex >= table.RowCount)
                throw new InvalidDataException($"Line {line + 1} has an invalid {RowIndexColumn} value.");

            foreach (var item in mapped)
            {
                if (item.Index >= values.Count) continue;
                var newValue = values[item.Index];
                var oldValue = session.GetCell(table.Name, rowIndex, item.Column.Name);
                if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                    edits.Add(new TableImportEdit(table.Name, rowIndex, item.Column.Name, oldValue, newValue));
            }
        }
        return edits;
    }

    public static void ExportRowTemplate(DatabaseSession session, string tableName, int rowIndex, string path)
    {
        var table = session.GetTable(tableName)
            ?? throw new InvalidOperationException($"Table '{tableName}' was not found.");
        if (rowIndex < 0 || rowIndex >= table.RowCount) throw new ArgumentOutOfRangeException(nameof(rowIndex));
        var fields = table.Columns
            .Where(column => column.IsWritable && !IsIdentityField(table, column))
            .ToDictionary(column => column.Name, column => session.GetCell(table.Name, rowIndex, column.Name));
        var template = new RowTemplate(table.Name, fields);
        File.WriteAllText(path, JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static IReadOnlyList<TableImportEdit> BuildTemplatePlan(
        DatabaseSession session, string tableName, int rowIndex, string path)
    {
        var table = session.GetTable(tableName)
            ?? throw new InvalidOperationException($"Table '{tableName}' was not found.");
        if (rowIndex < 0 || rowIndex >= table.RowCount) throw new ArgumentOutOfRangeException(nameof(rowIndex));
        var template = JsonSerializer.Deserialize<RowTemplate>(File.ReadAllText(path))
            ?? throw new InvalidDataException("The row template is empty or invalid.");
        if (!template.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"This template is for '{template.TableName}', not '{table.Name}'.");

        var edits = new List<TableImportEdit>();
        foreach (var (fieldName, newValue) in template.Fields)
        {
            var column = table.FindColumn(fieldName);
            if (column == null || !column.IsWritable || IsIdentityField(table, column)) continue;
            var oldValue = session.GetCell(table.Name, rowIndex, column.Name);
            if (oldValue != newValue)
                edits.Add(new(table.Name, rowIndex, column.Name, oldValue, newValue));
        }
        return edits;
    }

    public static bool MatchesFilter(string value, string expression)
    {
        expression = expression.Trim();
        if (expression.Length == 0) return true;
        foreach (var op in new[] { ">=", "<=", "!=", "=", ">", "<" })
        {
            if (!expression.StartsWith(op, StringComparison.Ordinal)) continue;
            var operand = expression[op.Length..].Trim();
            if (op == "=") return value.Equals(operand, StringComparison.OrdinalIgnoreCase);
            if (op == "!=") return !value.Equals(operand, StringComparison.OrdinalIgnoreCase);
            if (!double.TryParse(value, out var left) || !double.TryParse(operand, out var right)) return false;
            return op switch
            {
                ">=" => left >= right,
                "<=" => left <= right,
                ">" => left > right,
                "<" => left < right,
                _ => false,
            };
        }
        return value.Contains(expression, StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<IReadOnlyList<string>> Parse(string text, char delimiter)
    {
        var rows = new List<IReadOnlyList<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (quoted)
            {
                if (character == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (character == '"') quoted = false;
                else field.Append(character);
                continue;
            }

            if (character == '"' && field.Length == 0) quoted = true;
            else if (character == delimiter)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (character is '\r' or '\n')
            {
                if (character == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = new List<string>();
            }
            else field.Append(character);
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }
        return rows;
    }

    public static string FormatRow(IEnumerable<string> values, char delimiter) =>
        string.Join(delimiter, values.Select(value => Quote(value, delimiter)));

    private static void WriteRow(TextWriter writer, IEnumerable<string> values, char delimiter) =>
        writer.WriteLine(FormatRow(values, delimiter));

    private static string Quote(string value, char delimiter)
    {
        if (value.IndexOfAny(new[] { delimiter, '"', '\r', '\n' }) < 0) return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static int FindColumn(IReadOnlyList<string> header, string name)
    {
        for (var i = 0; i < header.Count; i++)
            if (header[i].Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    private static char DelimiterFor(string path) =>
        Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase) ? ',' : '\t';

    private static bool IsIdentityField(DbTable table, DbColumn column)
    {
        var singular = table.Name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? table.Name[..^1] : table.Name;
        return column.Name.Equals(singular + "id", StringComparison.OrdinalIgnoreCase) ||
               column.Name.Equals("artificialkey", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record TableImportEdit(
    string TableName,
    int RowIndex,
    string FieldName,
    string OldValue,
    string NewValue);

public sealed record RowTemplate(string TableName, Dictionary<string, string> Fields);
