using CM26.Application.Models;

namespace CM26.App.Sections;

internal sealed class LegacyGenericCompetitionsSection : GenericTableSection
{
    public LegacyGenericCompetitionsSection(AppServices s) : base(
        s, "competitions", "Competitions", "competition",
        d => GetCompetitions(s),
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["competitionid"] = "Competition ID",
            ["country_lock"] = "Country",
            ["ballid"] = "Ball",
            ["competitionimportance"] = "Importance",
            ["has_var"] = "Uses VAR",
            ["iswomencompetition"] = "Women's",
            ["isrealcompetition"] = "Licensed",
        },
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "competitionid", "country_lock", "ballid", "competitionimportance", "has_var" },
            ["Details"] = new[] { "iswomencompetition", "isrealcompetition", "crowdregion" },
        },
        idx =>
        {
            var t = s.Session.GetTable("competition")!;
            var rec = s.Session.GetRecord("competition", idx)!;
            int id = Col(t, "competitionid"), country = Col(t, "country_lock");
            return ($"Competition {rec.Get(id)}", s.Resolver!.NationName(Parse(rec.Get(country))));
        })
    { }

    private static IReadOnlyList<RecordListItem> GetCompetitions(AppServices s)
    {
        var t = s.Session.GetTable("competition");
        if (t == null) return Array.Empty<RecordListItem>();
        int id = Col(t, "competitionid"), country = Col(t, "country_lock"), ball = Col(t, "ballid");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = s.Session.GetRecord("competition", r); if (rec == null) continue;
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = $"Competition {rec.Get(id)}",
                Subtitle = s.Resolver!.NationName(Parse(rec.Get(country))),
                Detail = $"Ball {rec.Get(ball)}",
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }
}

internal sealed class LegacyGenericBallsSection : GenericTableSection
{
    public LegacyGenericBallsSection(AppServices s) : base(
        s, "balls", "Balls", "teamballs",
        d => GetBalls(s),
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ballid"] = "Ball ID",
            ["balltype"] = "Ball Type",
            ["islicensed"] = "Licensed",
            ["isavailableinstore"] = "In Store",
            ["isembargoed"] = "Embargoed",
        },
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "ballid", "balltype", "islicensed", "isavailableinstore", "isembargoed", "isrewardable" },
        },
        idx =>
        {
            var rec = s.Session.GetRecord("teamballs", idx)!;
            return ($"Ball {rec.Get(Col(s.Session.GetTable("teamballs")!, "ballid"))}", "Match ball");
        },
        previewProvider: idx =>
        {
            var t = s.Session.GetTable("teamballs")!;
            var rec = s.Session.GetRecord("teamballs", idx)!;
            int ballId = Parse(rec.Get(Col(t, "ballid")));
            return (s.Assets.GetBall(ballId), $"Ball · ID {ballId}");
        })
    { }

    private static IReadOnlyList<RecordListItem> GetBalls(AppServices s)
    {
        var t = s.Session.GetTable("teamballs");
        if (t == null) return Array.Empty<RecordListItem>();
        int id = Col(t, "ballid"), type = Col(t, "balltype");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = s.Session.GetRecord("teamballs", r); if (rec == null) continue;
            list.Add(new RecordListItem { RecordIndex = r, Title = $"Ball {rec.Get(id)}", Subtitle = $"Type {rec.Get(type)}" });
        }
        return list.OrderBy(x => x.Title).ToList();
    }
}

internal sealed class LegacyGenericBootsSection : GenericTableSection
{
    public LegacyGenericBootsSection(AppServices s) : base(
        s, "boots", "Boots", "playerboots",
        d => GetBoots(s),
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["manufacturerid"] = "Manufacturer",
            ["shoetype"] = "Shoe Type",
            ["shoedesign"] = "Design",
            ["shoecolor1"] = "Colour 1",
            ["shoecolor2"] = "Colour 2",
            ["islicensed"] = "Licensed",
            ["gender"] = "Gender",
        },
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "manufacturerid", "shoetype", "shoedesign", "gender", "islicensed" },
            ["Colours"] = new[] { "shoecolor1", "shoecolor2" },
        },
        idx => ($"Boots {idx + 1}", "Footwear"),
        previewProvider: idx =>
        {
            var t = s.Session.GetTable("playerboots")!;
            var rec = s.Session.GetRecord("playerboots", idx)!;
            int shoe = Parse(rec.Get(Col(t, "shoetype")));
            return (s.Assets.GetBoot(shoe), $"Boot · Type {shoe}");
        })
    { }

    private static IReadOnlyList<RecordListItem> GetBoots(AppServices s)
    {
        var t = s.Session.GetTable("playerboots");
        if (t == null) return Array.Empty<RecordListItem>();
        int manu = Col(t, "manufacturerid"), type = Col(t, "shoetype");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = s.Session.GetRecord("playerboots", r); if (rec == null) continue;
            list.Add(new RecordListItem { RecordIndex = r, Title = $"Boots {r + 1}", Subtitle = $"Manufacturer {rec.Get(manu)}", Detail = $"Type {rec.Get(type)}" });
        }
        return list;
    }
}
