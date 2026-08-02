using CM26.Application.Models;

namespace CM26.Application.Services;

/// <summary>
/// Builds resolved record lists and field editors for each section, using the session
/// and name resolver. All UI sections consume this rather than touching tables directly.
/// </summary>
public sealed class SectionDataService
{
    private readonly DatabaseSession _session;
    private readonly NameResolverService _resolver;
    private readonly PendingChangesService _pending;

    public SectionDataService(DatabaseSession session, NameResolverService resolver, PendingChangesService pending)
    {
        _session = session;
        _resolver = resolver;
        _pending = pending;
    }

    // ---------- generic helpers ----------

    private DbTable? Table(string name) => _session.GetTable(name);

    private static int Col(DbTable t, string name)
    {
        for (int i = 0; i < t.Columns.Count; i++)
            if (t.Columns[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    private static int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

    private string Cell(string table, int row, string field) => _session.GetCell(table, row, field);

    // ---------- Countries ----------

    public IReadOnlyList<RecordListItem> GetCountries()
    {
        var t = Table("nations"); if (t == null) return Array.Empty<RecordListItem>();
        int name = Col(t, "nationname"), conf = Col(t, "confederation"), iso = Col(t, "isocountrycode");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("nations", r); if (rec == null) continue;
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = rec.Get(name),
                Subtitle = _resolver.ConfederationLabel(ParseInt(rec.Get(conf))),
                Detail = rec.Get(iso).ToUpperInvariant(),
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- Leagues ----------

    public IReadOnlyList<RecordListItem> GetLeagues()
    {
        var t = Table("leagues"); if (t == null) return Array.Empty<RecordListItem>();
        int name = Col(t, "leaguename"), country = Col(t, "countryid"), level = Col(t, "level");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("leagues", r); if (rec == null) continue;
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = rec.Get(name),
                Subtitle = _resolver.NationName(ParseInt(rec.Get(country))),
                Detail = $"Level {rec.Get(level)}",
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    /// <summary>Returns the actual club names registered in a league's FC26 link rows.</summary>
    public IReadOnlyList<string> GetLeagueTeams(int leagueId)
    {
        var links = Table("leagueteamlinks");
        if (links == null || leagueId <= 0) return Array.Empty<string>();
        var league = Col(links, "leagueid");
        var team = Col(links, "teamid");
        var names = new List<string>();
        for (var row = 0; row < links.RowCount; row++)
        {
            var rec = _session.GetRecord("leagueteamlinks", row); if (rec == null) continue;
            if (ParseInt(rec.Get(league)) != leagueId) continue;
            names.Add(_resolver.TeamName(ParseInt(rec.Get(team))));
        }
        return names.Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ---------- Teams ----------

    public IReadOnlyList<RecordListItem> GetTeams()
    {
        var t = Table("teams"); if (t == null) return Array.Empty<RecordListItem>();
        int id = Col(t, "teamid"), name = Col(t, "teamname"), ovr = Col(t, "overallrating");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("teams", r); if (rec == null) continue;
            int teamId = ParseInt(rec.Get(id));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = rec.Get(name),
                Subtitle = _resolver.TeamLeagueName(teamId),
                Detail = $"OVR {rec.Get(ovr)}",
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- Players ----------

    public IReadOnlyList<RecordListItem> GetPlayers()
    {
        var t = Table("players"); if (t == null) return Array.Empty<RecordListItem>();
        int id = Col(t, "playerid"), fn = Col(t, "firstnameid"), ln = Col(t, "lastnameid"), cn = Col(t, "commonnameid");
        int ovr = Col(t, "overallrating"), pos = Col(t, "preferredposition1");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("players", r); if (rec == null) continue;
            int playerId = ParseInt(rec.Get(id));
            var parts = _resolver.PlayerNameParts(playerId, ParseInt(rec.Get(fn)), ParseInt(rec.Get(ln)), ParseInt(rec.Get(cn)));
            var club = _resolver.PlayerClubName(playerId);
            var posLabel = NameResolverService.PositionLabel(ParseInt(rec.Get(pos)));
            // Title = full display name; Detail = position · OVR · ID; search covers all name parts + club + id.
            var searchBlob = string.Join(' ', new[]
            {
                parts.FirstName, parts.LastName, parts.CommonName, parts.KnownAs, club, playerId.ToString()
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = parts.KnownAs ?? $"Player {playerId}",
                Subtitle = club,
                Detail = $"{posLabel} · OVR {rec.Get(ovr)} · {playerId}",
                SearchText = searchBlob,
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    /// <summary>
    /// Resolves a club roster from the FC26 player table and the teamplayerlinks mapping.
    /// Player ownership deliberately comes from the link table: teams does not contain a roster.
    /// </summary>
    public IReadOnlyList<TeamRosterItem> GetTeamRoster(int teamId)
    {
        var players = Table("players");
        if (players == null || teamId <= 0) return Array.Empty<TeamRosterItem>();

        int id = Col(players, "playerid"), fn = Col(players, "firstnameid"), ln = Col(players, "lastnameid");
        int cn = Col(players, "commonnameid"), pos = Col(players, "preferredposition1"), ovr = Col(players, "overallrating");
        int contractUntil = Col(players, "contractvaliduntil"), joiningDate = Col(players, "playerjointeamdate");
        var links = Table("teamplayerlinks");
        var linkByPlayerId = new Dictionary<int, DbRecord>();
        if (links != null)
        {
            var linkPlayerId = Col(links, "playerid");
            var linkTeamId = Col(links, "teamid");
            for (var row = 0; row < links.RowCount; row++)
            {
                var link = _session.GetRecord("teamplayerlinks", row); if (link == null) continue;
                if (ParseInt(link.Get(linkTeamId)) == teamId)
                    linkByPlayerId[ParseInt(link.Get(linkPlayerId))] = link;
            }
        }
        var loanByPlayerId = new Dictionary<int, (int fromTeamId, string endDate)>();
        var loans = Table("playerloans");
        if (loans != null)
        {
            var loanPlayerId = Col(loans, "playerid");
            var loanFromTeam = Col(loans, "teamidloanedfrom");
            var loanEnd = Col(loans, "loandateend");
            for (var row = 0; row < loans.RowCount; row++)
            {
                var loan = _session.GetRecord("playerloans", row); if (loan == null) continue;
                var playerId = ParseInt(loan.Get(loanPlayerId));
                if (playerId > 0)
                    loanByPlayerId[playerId] = (ParseInt(loan.Get(loanFromTeam)), loan.Get(loanEnd));
            }
        }
        var roster = new List<TeamRosterItem>();
        for (var row = 0; row < players.RowCount; row++)
        {
            var rec = _session.GetRecord("players", row); if (rec == null) continue;
            var playerId = ParseInt(rec.Get(id));
            if (_resolver.PlayerTeamId(playerId) != teamId) continue;
            var fnId = ParseInt(rec.Get(fn)); var lnId = ParseInt(rec.Get(ln)); var cnId = ParseInt(rec.Get(cn));
            var parts = _resolver.PlayerNameParts(playerId, fnId, lnId, cnId);
            var resolved = parts.HasAnyName;
            linkByPlayerId.TryGetValue(playerId, out var link);
            loanByPlayerId.TryGetValue(playerId, out var loanInfo);
            string LinkValue(string field) => link == null || links == null ? string.Empty : link.Get(Col(links, field));
            roster.Add(new TeamRosterItem
            {
                PlayerId = playerId,
                JerseyNumber = _resolver.PlayerJersey(playerId) ?? 0,
                // Verified real name, or the documented "Player {id}" fallback — never a raw key.
                Name = parts.KnownAs ?? $"Player {playerId}",
                Resolved = resolved,
                Position = NameResolverService.PositionLabel(ParseInt(rec.Get(pos))),
                Overall = rec.Get(ovr),
                LeagueAppearances = ParseInt(LinkValue("leagueappearances")),
                LeagueGoals = ParseInt(LinkValue("leaguegoals")),
                YellowCards = ParseInt(LinkValue("yellows")),
                RedCards = ParseInt(LinkValue("reds")),
                Form = LinkValue("form"),
                Injury = LinkValue("injury"),
                IsTopScorer = ParseInt(LinkValue("istopscorer")) != 0,
                LoanFrom = loanInfo.fromTeamId > 0 ? _resolver.TeamName(loanInfo.fromTeamId) : string.Empty,
                LoanEndDate = loanInfo.endDate ?? string.Empty,
                ContractValidUntil = contractUntil >= 0 ? rec.Get(contractUntil) : string.Empty,
                JoiningDate = joiningDate >= 0 ? rec.Get(joiningDate) : string.Empty,
            });
        }
        return roster.OrderBy(x => x.JerseyNumber <= 0 ? int.MaxValue : x.JerseyNumber)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ---------- Managers ----------

    public IReadOnlyList<RecordListItem> GetManagers()
    {
        var t = Table("manager"); if (t == null) return Array.Empty<RecordListItem>();
        int fn = Col(t, "firstname"), sn = Col(t, "surname"), team = Col(t, "teamid"), nat = Col(t, "nationality");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("manager", r); if (rec == null) continue;
            var full = $"{rec.Get(fn)} {rec.Get(sn)}".Trim();
            int teamId = ParseInt(rec.Get(team));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = string.IsNullOrWhiteSpace(full) ? $"Manager {r}" : full,
                Subtitle = teamId > 0 ? _resolver.TeamName(teamId) : "—",
                Detail = _resolver.NationName(ParseInt(rec.Get(nat))),
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- Stadiums ----------

    public IReadOnlyList<RecordListItem> GetStadiums()
    {
        var t = Table("stadiums"); if (t == null) return Array.Empty<RecordListItem>();
        int name = Col(t, "name"), cap = Col(t, "capacity"), home = Col(t, "hometeamid"), country = Col(t, "countrycode");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("stadiums", r); if (rec == null) continue;
            int homeId = ParseInt(rec.Get(home));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = rec.Get(name),
                Subtitle = homeId > 0 ? _resolver.TeamName(homeId) : _resolver.NationName(ParseInt(rec.Get(country))),
                Detail = $"Capacity {rec.Get(cap)}",
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- Referees ----------

    public IReadOnlyList<RecordListItem> GetReferees()
    {
        var t = Table("referee"); if (t == null) return Array.Empty<RecordListItem>();
        int fn = Col(t, "firstname"), sn = Col(t, "surname"), nat = Col(t, "nationalitycode"), league = Col(t, "leagueid");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("referee", r); if (rec == null) continue;
            var full = $"{rec.Get(fn)} {rec.Get(sn)}".Trim();
            int leagueId = ParseInt(rec.Get(league));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = string.IsNullOrWhiteSpace(full) ? $"Referee {r}" : full,
                Subtitle = leagueId > 0 ? _resolver.LeagueName(leagueId) : "—",
                Detail = _resolver.NationName(ParseInt(rec.Get(nat))),
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- Formations ----------

    public IReadOnlyList<RecordListItem> GetFormations()
    {
        var t = Table("formations"); if (t == null) return Array.Empty<RecordListItem>();
        int name = Col(t, "formationname"), team = Col(t, "teamid");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("formations", r); if (rec == null) continue;
            int teamId = ParseInt(rec.Get(team));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = rec.Get(name),
                Subtitle = teamId > 0 ? _resolver.TeamName(teamId) : "Generic",
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- Kits ----------

    public IReadOnlyList<RecordListItem> GetKits()
    {
        var t = Table("teamkits"); if (t == null) return Array.Empty<RecordListItem>();
        int team = Col(t, "teamtechid"), type = Col(t, "teamkittypetechid"), year = Col(t, "year");
        var list = new List<RecordListItem>(t.RowCount);
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = _session.GetRecord("teamkits", r); if (rec == null) continue;
            int teamId = ParseInt(rec.Get(team));
            var typeLabel = NameResolverService.KitTypeLabel(ParseInt(rec.Get(type)));
            list.Add(new RecordListItem
            {
                RecordIndex = r,
                Title = $"{_resolver.TeamName(teamId)} — {typeLabel}",
                Subtitle = _resolver.TeamLeagueName(teamId),
                Detail = $"Year {rec.Get(year)}",
            });
        }
        return list.OrderBy(x => x.Title).ToList();
    }

    // ---------- generic field editor for any record ----------

    /// <summary>Build editable field models for a record, applying optional label maps and modified state.</summary>
    public IReadOnlyList<FieldValue> GetFields(string tableName, int recordIndex,
        IReadOnlyDictionary<string, string>? labelMap = null,
        Func<string, string, string>? valueFormatter = null)
    {
        var t = Table(tableName);
        var rec = _session.GetRecord(tableName, recordIndex);
        if (t == null || rec == null) return Array.Empty<FieldValue>();

        var fields = new List<FieldValue>(t.Columns.Count);
        for (int i = 0; i < t.Columns.Count; i++)
        {
            var col = t.Columns[i];
            var raw = rec.Get(i);
            var display = valueFormatter?.Invoke(col.Name, raw) ?? raw;
            var label = labelMap != null && labelMap.TryGetValue(col.Name, out var friendly) ? friendly : SplitCamel(col.Name);
            fields.Add(new FieldValue
            {
                FieldName = col.Name,
                Label = label,
                Value = display,
                RawValue = raw,
                IsWritable = col.IsWritable,
                KindLabel = col.KindLabel,
                RangeLow = col.IsInteger ? col.RangeLow : null,
                RangeHigh = col.IsInteger ? col.RangeHigh : null,
                Modified = _pending.IsFieldModified(tableName, recordIndex, col.Name),
            });
        }
        return fields;
    }

    private static string SplitCamel(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new System.Text.StringBuilder(s.Length + 8);
        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(s[i - 1])) sb.Append(' ');
            sb.Append(c);
        }
        var result = sb.ToString();
        return char.ToUpper(result[0], System.Globalization.CultureInfo.InvariantCulture) + result[1..];
    }
}
