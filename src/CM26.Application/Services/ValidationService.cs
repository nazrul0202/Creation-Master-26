using CM26.Application.Models;

namespace CM26.Application.Services;

public sealed record ValidationIssue(string Table, int Row, string Field, string Message, bool IsError);

/// <summary>Aggregates validation over staged changes before save.</summary>
public sealed class ValidationService
{
    private readonly DatabaseSession _session;

    public ValidationService(DatabaseSession session) => _session = session;

    /// <summary>Validate every staged change against current schema ranges and engine rules.</summary>
    public IReadOnlyList<ValidationIssue> ValidateAll(IReadOnlyList<PendingChange> changes)
    {
        var issues = new List<ValidationIssue>();
        foreach (var c in changes)
        {
            var table = _session.GetTable(c.TableName);
            if (table == null)
            {
                issues.Add(new(c.TableName, c.RowIndex, c.FieldName, "Table no longer available", true));
                continue;
            }
            var col = table.FindColumn(c.FieldName);
            if (col == null)
            {
                issues.Add(new(c.TableName, c.RowIndex, c.FieldName, "Field no longer available", true));
                continue;
            }
            if (!col.IsWritable)
            {
                issues.Add(new(c.TableName, c.RowIndex, c.FieldName, "Field is read-only", true));
                continue;
            }
            if (col.IsInteger)
            {
                if (!int.TryParse(c.NewValue, out var v))
                    issues.Add(new(c.TableName, c.RowIndex, c.FieldName, "Integer value required", true));
                else if (v < col.RangeLow || v > col.RangeHigh)
                    issues.Add(new(c.TableName, c.RowIndex, c.FieldName, $"Out of range {col.RangeLow}..{col.RangeHigh}", true));
            }
            else if (col.IsFloat)
            {
                if (!float.TryParse(c.NewValue, out _))
                    issues.Add(new(c.TableName, c.RowIndex, c.FieldName, "Decimal value required", true));
            }
            else if (col.IsString && !col.IsCompressed)
            {
                if (c.NewValue.Length >= col.Depth / 8)
                    issues.Add(new(c.TableName, c.RowIndex, c.FieldName, $"Text exceeds capacity {col.Depth / 8 - 1}", true));
            }
        }
        return issues;
    }
}
