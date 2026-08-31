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
        table.Rows.Add("2", "1", "UEFA", "UEFA", "1");
        var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase) { ["compobj"] = table };
        Assert.Empty(CompdataSchema.Validate(tables));
        table.Rows.Add("3", "4", "Broken", "Broken", "999");
        Assert.Contains(CompdataSchema.Validate(tables), issue => issue.IsError && issue.Message.Contains("does not exist"));
    }

    [Fact]
    public void CompdataSchemaRejectsMissingParentAndInvalidHierarchy()
    {
        var table = Table("compobj");
        table.Rows.Add("0", "0", "World", "World", "-1");
        table.Rows.Add("1", "4", "S1", "Stage", "0");
        table.Rows.Add("2", "5", "G1", "Group", "");
        var issues = CompdataSchema.Validate(new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase)
        { ["compobj"] = table });
        Assert.Contains(issues, issue => issue.Message.Contains("cannot use a type 0 parent"));
        Assert.Contains(issues, issue => issue.Message.Contains("Parent Object ID is required"));
    }

    [Fact]
    public void CompdataBuilderCreatesDatabaseMappedTournament()
    {
        var tables = MinimalCompdataTables();
        var result = CompdataBuilder.CreateLeagueOrCup(tables,
            new CompdataLeagueBuildRequest("Audit Cup", 2256, 1, 1));

        var competition = tables["compobj"].Rows.Cast<DataRow>()
            .Single(row => Convert.ToString(row[0]) == result.CompetitionObjectId.ToString());
        Assert.Equal("C2256", competition[2]);
        Assert.Equal("0", competition[4]);
        Assert.Contains(tables["settings"].Rows.Cast<DataRow>(), row =>
            Convert.ToString(row[0]) == result.CompetitionObjectId.ToString() &&
            Convert.ToString(row[1]) == "asset_id" && Convert.ToString(row[2]) == "2256");
        Assert.Empty(CompdataSchema.Validate(tables));
    }

    [Fact]
    public void CompdataBuilderRejectsDuplicateDatabaseCompetition()
    {
        var tables = MinimalCompdataTables();
        _ = CompdataBuilder.CreateLeagueOrCup(tables,
            new CompdataLeagueBuildRequest("First", 2256, 1, 1));
        var error = Assert.Throws<InvalidOperationException>(() => CompdataBuilder.CreateLeagueOrCup(tables,
            new CompdataLeagueBuildRequest("Duplicate", 2256, 1, 1)));
        Assert.Contains("already has a Compdata object", error.Message);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(12)]
    [InlineData(20)]
    [InlineData(24)]
    public void CountryCareerLeagueBuildsCompleteDirectSaveData(int teamCount)
    {
        var tables = CareerCompdataTables();
        var teamIds = Enumerable.Range(700000, teamCount).ToArray();
        var result = CompdataBuilder.CreateCountryCareerLeague(tables,
            new CountryCareerBuildRequest("Malaysia", 95, 5, "Malaysia Test League",
                900000 + teamCount, teamIds));

        var expectedRounds = (teamCount - 1) * 2;
        Assert.Equal(expectedRounds, tables["schedule"].Rows.Cast<DataRow>().Count(row =>
            Convert.ToString(row[0]) == result.StageIds.Single().ToString()));
        Assert.Equal(teamCount, tables["initteams"].Rows.Cast<DataRow>().Count(row =>
            Convert.ToString(row[0]) == result.CompetitionObjectId.ToString()));
        Assert.Equal(teamIds, tables["initteams"].Rows.Cast<DataRow>()
            .Where(row => Convert.ToString(row[0]) == result.CompetitionObjectId.ToString())
            .OrderBy(row => Convert.ToInt32(row[1]))
            .Select(row => Convert.ToInt32(row[2])));
        Assert.Equal(teamCount, tables["standings"].Rows.Cast<DataRow>().Count(row =>
            Convert.ToString(row[0]) == result.GroupIds.Single().ToString()));
        Assert.Contains(tables["settings"].Rows.Cast<DataRow>(), row =>
            Convert.ToString(row[0]) == result.CompetitionObjectId.ToString() &&
            Convert.ToString(row[1]) == "asset_id" &&
            Convert.ToString(row[2]) == (900000 + teamCount).ToString());
        Assert.DoesNotContain(CompdataSchema.Validate(tables), issue => issue.IsError);
    }

    [Fact]
    public void CompdataSchemaRejectsMissingDatabaseAssetMapping()
    {
        var tables = MinimalCompdataTables();
        tables["compobj"].Rows.Add("1", "3", "C2256", "Audit League", "0");
        tables["compids"].Rows.Add("1");
        tables["settings"].Rows.Add("1", "comp_type", "LEAGUE");
        var issues = CompdataSchema.Validate(tables);
        Assert.Contains(issues, issue => issue.IsError && issue.Message.Contains("asset_id 2256"));
    }

    [Fact]
    public void CompdataSchemaUsesFc26ObjectiveAndTaskColumns()
    {
        Assert.Equal("Competition Object ID", CompdataSchema.GetColumns("objectives", 3)[0]);
        var tasks = CompdataSchema.GetColumns("tasks", 7);
        Assert.Equal(7, tasks.Length);
        Assert.Equal("Competition Object ID", tasks[0]);
        Assert.Equal("Target Object ID", tasks[6]);
    }

    [Fact]
    public void CompdataCalendarDetectsSharedTeamOnSameDayAcrossCompetitions()
    {
        var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase)
        {
            ["compobj"] = Table("compobj"),
            ["initteams"] = Table("initteams"),
            ["schedule"] = Table("schedule")
        };
        tables["compobj"].Rows.Add("0", "0", "WORLD", "World", "-1");
        tables["compobj"].Rows.Add("10", "3", "C100", "League", "0");
        tables["compobj"].Rows.Add("11", "4", "S1", "League stage", "10");
        tables["compobj"].Rows.Add("20", "3", "C200", "Cup", "0");
        tables["compobj"].Rows.Add("21", "4", "S1", "Cup stage", "20");
        tables["initteams"].Rows.Add("10", "0", "501");
        tables["initteams"].Rows.Add("20", "0", "501");
        tables["schedule"].Rows.Add("11", "210", "1", "1", "1", "1500");
        tables["schedule"].Rows.Add("21", "210", "1", "1", "1", "2000");

        var conflict = Assert.Single(CompdataSchema.FindTeamCalendarConflicts(tables));
        Assert.Equal(501, conflict.TeamId);
        Assert.Equal(210, conflict.Day);
        Assert.Equal(10, conflict.FirstCompetitionId);
        Assert.Equal(20, conflict.SecondCompetitionId);
    }

    [Fact]
    public void CompdataCalendarAllowsSeveralKickoffRowsInsideOneCompetition()
    {
        var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase)
        {
            ["compobj"] = Table("compobj"),
            ["initteams"] = Table("initteams"),
            ["schedule"] = Table("schedule")
        };
        tables["compobj"].Rows.Add("0", "0", "WORLD", "World", "-1");
        tables["compobj"].Rows.Add("10", "3", "C100", "League", "0");
        tables["compobj"].Rows.Add("11", "4", "S1", "League stage", "10");
        tables["initteams"].Rows.Add("10", "0", "501");
        tables["schedule"].Rows.Add("11", "210", "1", "1", "1", "1500");
        tables["schedule"].Rows.Add("11", "210", "1", "1", "1", "2000");

        Assert.Empty(CompdataSchema.FindTeamCalendarConflicts(tables));
    }

    [Fact]
    public void LegacyAssetBatchStagingRejectsPartialInputBeforeChangingState()
    {
        var temp = Path.Combine(Path.GetTempPath(), "cm26-tests-" + Guid.NewGuid().ToString("N"));
        var fingerprint = Guid.NewGuid().ToString("N");
        var workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Creation Master 26", "direct-edit-workspace", fingerprint[..16]);
        Directory.CreateDirectory(temp);
        try
        {
            var valid = Path.Combine(temp, "compobj.txt");
            File.WriteAllText(valid, "0,0,WORLD,World,-1\r\n");
            var mods = new LegacyAssetModService();
            mods.Open(fingerprint);
            Assert.Throws<FileNotFoundException>(() => mods.StageFilesAtomically(new[]
            {
                ("data/compobj.txt", valid),
                ("data/settings.txt", Path.Combine(temp, "missing.txt")),
            }));
            Assert.False(mods.HasChanges);

            var second = Path.Combine(temp, "settings.txt");
            File.WriteAllText(second, "0,asset_id,1\r\n");
            var staged = mods.StageFilesAtomically(new[]
            {
                ("data/compobj.txt", valid),
                ("data/settings.txt", second),
            });
            Assert.Equal(2, staged.Count);
            Assert.All(staged, path => Assert.True(File.Exists(path)));
            Assert.Equal(2, mods.Count);
        }
        finally
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, recursive: true);
            if (Directory.Exists(workspace)) Directory.Delete(workspace, recursive: true);
        }
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

    private static Dictionary<string, DataTable> MinimalCompdataTables()
    {
        var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in new[] { "compobj", "compids", "settings", "standings", "schedule" })
            tables[name] = Table(name);
        tables["compobj"].Rows.Add("0", "0", "World", "World", "-1");
        return tables;
    }

    private static Dictionary<string, DataTable> CareerCompdataTables()
    {
        var tables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in new[]
                 {
                     "compobj", "compids", "settings", "initteams", "standings", "schedule"
                 })
            tables[name] = Table(name);
        tables["compobj"].Rows.Add("0", "0", "WORLD", "World", "-1");
        tables["compobj"].Rows.Add("1", "1", "AFC", "AFC", "0");
        return tables;
    }

    private static DataTable Table(string name)
    {
        var table = new DataTable(name);
        foreach (var column in CompdataSchema.GetColumns(name, 0)) table.Columns.Add(column);
        return table;
    }
}
