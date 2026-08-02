using CM26.Application.Models;

namespace CM26.App.Sections;

/// <summary>Legacy generic editor retained temporarily as a schema reference for the CM16 layout port.</summary>
internal sealed class LegacyCountriesSection : GenericTableSection
{
    public LegacyCountriesSection(AppServices s) : base(
        s, "countries", "Countries", "nations",
        d => d.GetCountries(),
        LabelMaps.Nations,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "nationname", "isocountrycode", "confederation", "nationstartingfirstletter" },
            ["Details"] = new[] { "groupid", "top_tier", "streetdressing" },
            ["Technical"] = new[] { "nationid" },
        },
        idx =>
        {
            var d = s.RequireData(); var t = s.Session.GetTable("nations")!;
            var rec = s.Session.GetRecord("nations", idx)!;
            int name = Col(t, "nationname"), conf = Col(t, "confederation"), iso = Col(t, "isocountrycode");
            return (rec.Get(name), $"{s.Resolver!.ConfederationLabel(Parse(rec.Get(conf)))} · {rec.Get(iso).ToUpperInvariant()}");
        },
        previewProvider: idx =>
        {
            var t = s.Session.GetTable("nations")!;
            var rec = s.Session.GetRecord("nations", idx)!;
            int nid = Parse(rec.Get(Col(t, "nationid")));
            return (s.Assets.GetFlag(nid), $"Flag · Nation {nid}");
        })
    { }
}

/// <summary>Legacy generic editor retained temporarily as a schema reference for the CM16 layout port.</summary>
internal sealed class LegacyLeaguesSection : GenericTableSection
{
    public LegacyLeaguesSection(AppServices s) : base(
        s, "leagues", "Leagues", "leagues",
        d => d.GetLeagues(),
        LabelMaps.Leagues,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "leaguename", "countryid", "level", "leaguetype" },
            ["Details"] = new[] { "iswomencompetition", "isinternationalleague", "iswithintransferwindow", "leaguetimeslice" },
            ["Technical"] = new[] { "leagueid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("leagues")!;
            var rec = s.Session.GetRecord("leagues", idx)!;
            int name = Col(t, "leaguename"), country = Col(t, "countryid"), level = Col(t, "level");
            return (rec.Get(name), $"{s.Resolver!.NationName(Parse(rec.Get(country)))} · Level {rec.Get(level)}");
        })
    { }
}

/// <summary>Legacy generic editor retained temporarily as a schema reference for the CM16 layout port.</summary>
internal sealed class LegacyTeamsSection : GenericTableSection
{
    public LegacyTeamsSection(AppServices s) : base(
        s, "teams", "Teams", "teams",
        d => d.GetTeams(),
        LabelMaps.Teams,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "teamname", "overallrating", "attackrating", "midfieldrating", "defenserating", "foundationyear" },
            ["Match"] = new[] { "captainid", "penaltytakerid", "freekicktakerid", "leftcornerkicktakerid", "rightcornerkicktakerid", "ballid" },
            ["Club"] = new[] { "domesticprestige", "internationalprestige", "clubworth", "popularity", "youthdevelopment" },
            ["Technical"] = new[] { "teamid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("teams")!;
            var rec = s.Session.GetRecord("teams", idx)!;
            int id = Col(t, "teamid"), name = Col(t, "teamname"), ovr = Col(t, "overallrating");
            int teamId = Parse(rec.Get(id));
            return (rec.Get(name), $"{s.Resolver!.TeamLeagueName(teamId)} · {s.Resolver.TeamNationName(teamId)} · OVR {rec.Get(ovr)}");
        },
        (field, value) =>
        {
            // resolve coded/ID values to names in the editor where helpful
            return field switch
            {
                "captainid" or "penaltytakerid" or "freekicktakerid" or "leftcornerkicktakerid" or "rightcornerkicktakerid"
                    => value, // keep numeric; player names not decodable (see TASK_STATE)
                _ => value,
            };
        })
    { }
}

internal sealed class LegacyGenericManagersSection : GenericTableSection
{
    public LegacyGenericManagersSection(AppServices s) : base(
        s, "managers", "Managers", "manager",
        d => d.GetManagers(),
        LabelMaps.Managers,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "firstname", "surname", "commonname", "nationality", "birthdate" },
            ["Career"] = new[] { "teamid", "starrating", "managerjointeamdate" },
            ["Physical"] = new[] { "height", "weight" },
            ["Technical"] = new[] { "managerid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("manager")!;
            var rec = s.Session.GetRecord("manager", idx)!;
            int fn = Col(t, "firstname"), sn = Col(t, "surname"), team = Col(t, "teamid"), nat = Col(t, "nationality");
            int teamId = Parse(rec.Get(team));
            var full = $"{rec.Get(fn)} {rec.Get(sn)}".Trim();
            return (string.IsNullOrWhiteSpace(full) ? $"Manager {idx}" : full,
                $"{(teamId > 0 ? s.Resolver!.TeamName(teamId) : "—")} · {s.Resolver!.NationName(Parse(rec.Get(nat)))}");
        })
    { }
}

internal sealed class LegacyGenericStadiumsSection : GenericTableSection
{
    public LegacyGenericStadiumsSection(AppServices s) : base(
        s, "stadiums", "Stadiums", "stadiums",
        d => d.GetStadiums(),
        LabelMaps.Stadiums,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "name", "capacity", "hometeamid", "countrycode", "cityid" },
            ["Pitch"] = new[] { "stadiumpitchlength", "stadiumpitchwidth", "defaultweather", "defaultseason", "defaulttime" },
            ["Technical"] = new[] { "stadiumid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("stadiums")!;
            var rec = s.Session.GetRecord("stadiums", idx)!;
            int name = Col(t, "name"), cap = Col(t, "capacity"), home = Col(t, "hometeamid");
            int homeId = Parse(rec.Get(home));
            return (rec.Get(name),
                $"{(homeId > 0 ? s.Resolver!.TeamName(homeId) : "—")} · Capacity {rec.Get(cap)}");
        },
        previewProvider: idx =>
        {
            var t = s.Session.GetTable("stadiums")!;
            var rec = s.Session.GetRecord("stadiums", idx)!;
            int sid = Parse(rec.Get(Col(t, "stadiumid")));
            return (s.Assets.GetStadium(sid), $"Stadium · ID {sid}");
        })
    { }
}

internal sealed class LegacyGenericRefereesSection : GenericTableSection
{
    public LegacyGenericRefereesSection(AppServices s) : base(
        s, "referees", "Referees", "referee",
        d => d.GetReferees(),
        LabelMaps.Referees,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "firstname", "surname", "nationalitycode", "birthdate" },
            ["Officiating"] = new[] { "leagueid", "cardstrictness", "foulstrictness" },
            ["Physical"] = new[] { "height", "weight" },
            ["Technical"] = new[] { "refereeid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("referee")!;
            var rec = s.Session.GetRecord("referee", idx)!;
            int fn = Col(t, "firstname"), sn = Col(t, "surname"), league = Col(t, "leagueid"), nat = Col(t, "nationalitycode");
            int leagueId = Parse(rec.Get(league));
            var full = $"{rec.Get(fn)} {rec.Get(sn)}".Trim();
            return (string.IsNullOrWhiteSpace(full) ? $"Referee {idx}" : full,
                $"{(leagueId > 0 ? s.Resolver!.LeagueName(leagueId) : "—")} · {s.Resolver!.NationName(Parse(rec.Get(nat)))}");
        })
    { }
}

internal sealed class LegacyGenericFormationsSection : GenericTableSection
{
    public LegacyGenericFormationsSection(AppServices s) : base(
        s, "formations", "Formations", "formations",
        d => d.GetFormations(),
        LabelMaps.Formations,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "formationname", "teamid", "attackers", "midfielders", "defenders", "offensiverating" },
            ["Technical"] = new[] { "formationid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("formations")!;
            var rec = s.Session.GetRecord("formations", idx)!;
            int name = Col(t, "formationname"), team = Col(t, "teamid");
            int teamId = Parse(rec.Get(team));
            return (rec.Get(name), teamId > 0 ? s.Resolver!.TeamName(teamId) : "Generic formation");
        })
    { }
}

internal sealed class LegacyGenericKitsSection : GenericTableSection
{
    public LegacyGenericKitsSection(AppServices s) : base(
        s, "kits", "Kits", "teamkits",
        d => d.GetKits(),
        LabelMaps.Kits,
        new Dictionary<string, string[]>
        {
            ["General"] = new[] { "teamtechid", "teamkittypetechid", "year" },
            ["Colours"] = new[] { "teamcolorprimr", "teamcolorprimg", "teamcolorprimb", "teamcolorsecr", "teamcolorsecg", "teamcolorsecb" },
            ["Templates"] = new[] { "jerseytemplateindex", "numberfonttype", "shortstemplateindex", "sockstemplateindex" },
            ["Technical"] = new[] { "teamkitid" },
        },
        idx =>
        {
            var t = s.Session.GetTable("teamkits")!;
            var rec = s.Session.GetRecord("teamkits", idx)!;
            int team = Col(t, "teamtechid"), type = Col(t, "teamkittypetechid"), year = Col(t, "year");
            int teamId = Parse(rec.Get(team));
            return ($"{s.Resolver!.TeamName(teamId)} — {CM26.Application.Services.NameResolverService.KitTypeLabel(Parse(rec.Get(type)))}",
                $"Year {rec.Get(year)}");
        })
    { }
}
