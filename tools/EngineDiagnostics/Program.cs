// CM26.EngineDiagnostics — READ-ONLY diagnostic harness over the protected T3DB engine + bridge.
// Never writes to the database. Calls the native engine via the C++/CLI bridge and CM26.Application
// services WITHOUT WinForms, so raw values can be traced layer-by-layer.
using System.Text;
using CM26.Application.Models;
using CM26.Application.Services;
using CM26.EngineBridge;

var dbFolder = args.Length > 0 ? args[0] : @"D:\CM 26 Final\database";
var cmd = args.Length > 1 ? args[1].ToLowerInvariant() : "--full-integrity-test";
var arg = args.Length > 2 ? args[2] : "";

static int Col(DbTable t, string n)
{
    for (int i = 0; i < t.Columns.Count; i++)
        if (t.Columns[i].Name.Equals(n, StringComparison.OrdinalIgnoreCase)) return i;
    return -1;
}
static string Hex(byte[] b) => string.Join(' ', b.Take(24).Select(x => x.ToString("X2")));
static string Asc(byte[] b) => new string(b.Take(24).Select(x => x >= 32 && x < 127 ? (char)x : '.').ToArray());

try
{
    // ---- load once (proves which files are opened) ----
    Console.WriteLine($"DB folder: {dbFolder}");
    using var session = new DatabaseSession();
    var summary = session.ValidateFolder(dbFolder);
    Console.WriteLine($"Validate: state={summary.State} tables={summary.TableCount}");
    Console.WriteLine($"  meta    = {summary.MetaPath}");
    Console.WriteLine($"  database= {summary.DatabasePath}");
    Console.WriteLine($"  locale  = {summary.LocalePath}");
    if ((int)summary.State != 0) { Console.WriteLine("FAIL: " + summary.Message); return 2; }
    session.Load(dbFolder);
    var resolver = new NameResolverService(session);
    var data = new SectionDataService(session, resolver, new PendingChangesService(session));

    switch (cmd)
    {
        case "--show-loaded-files":
            Console.WriteLine($"LoadedFolder: {session.LoadedFolder}");
            Console.WriteLine($"MetaPath: {session.MetaPath}");
            Console.WriteLine($"DatabasePath: {session.DatabasePath}");
            Console.WriteLine($"LocalePath: {session.LocalePath}");
            foreach (var t in session.Tables) Console.WriteLine($"  table: {t.Name} (locale={t.IsLocale}) rows={t.RowCount} cols={t.Columns.Count}");
            break;
        case "--list-tables":
            foreach (var t in session.Tables)
                Console.WriteLine($"{t.Name,-32} {t.RowCount,8} rows  {t.Columns.Count,3} cols  {(t.IsLocale ? "LOCALE" : "main")}");
            Console.WriteLine($"TOTAL: {session.Tables.Count} tables");
            break;
        case "--describe-table":
            DescribeTable(session, arg);
            break;
        case "--dump-country":
            DumpCountry(session, resolver, arg);
            break;
        case "--dump-player":
            DumpPlayer(session, resolver, arg);
            break;
        case "--trace-country-confederation":
            TraceCountryConfederation(session, resolver, arg);
            break;
        case "--trace-player-name":
            TracePlayerName(session, resolver, arg);
            break;
        case "--compare-bridge":
            CompareBridge(session, arg);
            break;
        case "--cache-switch-test":
            CacheSwitchTest(dbFolder);
            break;
        case "--full-integrity-test":
            FullIntegrity(session, resolver, data);
            break;
        default:
            Console.WriteLine("Unknown command: " + cmd);
            Console.WriteLine("Commands: --show-loaded-files --list-tables --describe-table <table>");
            Console.WriteLine("          --dump-country <id> --dump-player <id> --dump-raw-row <table> <pk>");
            Console.WriteLine("          --trace-country-confederation <id> --trace-player-name <id>");
            Console.WriteLine("          --compare-bridge <table> --cache-switch-test --full-integrity-test");
            return 1;
    }
    return 0;
}
catch (Exception ex) { Console.WriteLine("ERROR: " + ex); return 3; }

static void DescribeTable(DatabaseSession s, string name)
{
    var t = s.GetTable(name);
    if (t == null) { Console.WriteLine($"Table '{name}' not found"); return; }
    Console.WriteLine($"Table: {t.Name}  shortName={t.ShortName}  rows={t.RowCount}  locale={t.IsLocale}");
    Console.WriteLine($"  #  name              kind  depth  range           bitOff  writable");
    for (int i = 0; i < t.Columns.Count; i++)
    {
        var c = t.Columns[i];
        Console.WriteLine($"  {i,2} {c.Name,-18} {c.KindLabel,-12} d={c.Depth,-3} [{c.RangeLow}..{c.RangeHigh}] off={c.Kind} w={c.IsWritable}");
    }
}

static void DumpCountry(DatabaseSession s, NameResolverService r, string idArg)
{
    var t = s.GetTable("nations"); if (t == null) { Console.WriteLine("nations missing"); return; }
    int id = Col(t, "nationid"), name = Col(t, "nationname"), conf = Col(t, "confederation"), iso = Col(t, "isocountrycode");
    if (!int.TryParse(idArg, out var want)) { Console.WriteLine("need numeric country id"); return; }
    for (int row = 0; row < t.RowCount; row++)
    {
        var rec = s.GetRecord("nations", row); if (rec == null) continue;
        if (!int.TryParse(rec.Get(id), out var cid) || cid != want) continue;
        int.TryParse(rec.Get(conf), out var code);
        Console.WriteLine($"nationid={cid}  name='{rec.Get(name)}'  iso='{rec.Get(iso)}'  confederation_raw={code}  resolver_label={r.ConfederationLabel(code)}");
        Console.WriteLine("  all fields:");
        for (int i = 0; i < t.Columns.Count; i++) Console.WriteLine($"    [{i}] {t.Columns[i].Name} = '{rec.Get(i)}'");
        return;
    }
    Console.WriteLine($"country id {want} not found");
}

static void DumpPlayer(DatabaseSession s, NameResolverService r, string idArg)
{
    var t = s.GetTable("players"); if (t == null) { Console.WriteLine("players missing"); return; }
    int pid = Col(t, "playerid"), fn = Col(t, "firstnameid"), ln = Col(t, "lastnameid"), cn = Col(t, "commonnameid");
    if (!int.TryParse(idArg, out var want)) { Console.WriteLine("need numeric player id"); return; }
    for (int row = 0; row < t.RowCount; row++)
    {
        var rec = s.GetRecord("players", row); if (rec == null) continue;
        if (!int.TryParse(rec.Get(pid), out var p) || p != want) continue;
        int f = int.TryParse(rec.Get(fn), out var fv) ? fv : 0;
        int l = int.TryParse(rec.Get(ln), out var lv) ? lv : 0;
        int c = int.TryParse(rec.Get(cn), out var cv) ? cv : 0;
        var parts = r.PlayerNameParts(p, f, l, c);
        Console.WriteLine($"playerid={p}  firstnameid={f}  lastnameid={l}  commonnameid={c}");
        Console.WriteLine($"  first(raw resolver)='{parts.FirstName}'  last='{parts.LastName}'  common='{parts.CommonName}'  knownAs='{parts.KnownAs}'");
        Console.WriteLine($"  display='{r.PlayerDisplayName(p,f,l,c)}'");
        Console.WriteLine("  key fields:");
        foreach (var n in new[] { "playerid","firstnameid","lastnameid","commonnameid","playerjerseynameid","preferredposition1","overallrating","nationality" })
        { int ci = Col(t, n); Console.WriteLine($"    {n,-22} = '{(ci>=0?rec.Get(ci):"<missing>")}'"); }
        return;
    }
    Console.WriteLine($"player id {want} not found");
}

static void TraceCountryConfederation(DatabaseSession s, NameResolverService r, string idArg)
{
    Console.WriteLine("=== LAYER TRACE: country -> confederation ===");
    var t = s.GetTable("nations");
    int id = Col(t!, "nationid"), name = Col(t!, "nationname"), conf = Col(t!, "confederation");
    int.TryParse(idArg, out var want);
    for (int row = 0; row < t!.RowCount; row++)
    {
        var rec = s.GetRecord("nations", row); if (rec == null) continue;
        if (!int.TryParse(rec.Get(id), out var cid) || cid != want) continue;
        int code = int.TryParse(rec.Get(conf), out var cv) ? cv : 0;
        Console.WriteLine($"[Physical DB]     nationid={cid} name='{rec.Get(name)}'");
        Console.WriteLine($"[Native engine]   confederation field raw value = {code}");
        Console.WriteLine($"[Bridge]          GetCell -> '{rec.Get(conf)}' (string)");
        Console.WriteLine($"[NameResolver]    ConfederationLabel({code}) = '{r.ConfederationLabel(code)}'");
        Console.WriteLine($"[UI]              bound to resolver label");
        return;
    }
    Console.WriteLine($"country {want} not found");
}

static void TracePlayerName(DatabaseSession s, NameResolverService r, string idArg)
{
    Console.WriteLine("=== LAYER TRACE: player -> name ===");
    var t = s.GetTable("players");
    int pid = Col(t!, "playerid"), fn = Col(t!, "firstnameid"), ln = Col(t!, "lastnameid"), cn = Col(t!, "commonnameid");
    int.TryParse(idArg, out var want);
    for (int row = 0; row < t!.RowCount; row++)
    {
        var rec = s.GetRecord("players", row); if (rec == null) continue;
        if (!int.TryParse(rec.Get(pid), out var p) || p != want) continue;
        int f = int.TryParse(rec.Get(fn), out var fv) ? fv : 0;
        int l = int.TryParse(rec.Get(ln), out var lv) ? lv : 0;
        int c = int.TryParse(rec.Get(cn), out var cv) ? cv : 0;
        Console.WriteLine($"[Physical DB]     playerid={p}");
        Console.WriteLine($"[Native engine]   firstnameid={f} lastnameid={l} commonnameid={c}");
        // raw playernames bytes for each id
        foreach (var (nid,label) in new[] { (f,"first"), (l,"last"), (c,"common") })
            if (nid > 0) Console.WriteLine($"[playernames]     {label}nameid={nid} -> raw name bytes: {DescribeName(s, nid)}");
        var parts = r.PlayerNameParts(p, f, l, c);
        Console.WriteLine($"[Resolver]        first='{parts.FirstName}' last='{parts.LastName}' common='{parts.CommonName}' knownAs='{parts.KnownAs}'");
        Console.WriteLine($"[UI display]      '{r.PlayerDisplayName(p,f,l,c)}'");
        return;
    }
    Console.WriteLine($"player {want} not found");
}

static string DescribeName(DatabaseSession s, int nameId)
{
    var t = s.GetTable("playernames"); if (t == null) return "playernames missing";
    int idC = Col(t, "nameid"), nameC = Col(t, "name");
    for (int row = 0; row < t.RowCount; row++)
    {
        var rec = s.GetRecord("playernames", row); if (rec == null) continue;
        if (!int.TryParse(rec.Get(idC), out var id) || id != nameId) continue;
        var b = s.GetCellBytes("playernames", row, "name");
        return $"text='{rec.Get(nameC)}' bytes=[{Hex(b)}] asc='{Asc(b)}'";
    }
    return "nameid not found";
}

static void CompareBridge(DatabaseSession s, string table)
{
    var t = s.GetTable(table); if (t == null) { Console.WriteLine($"table '{table}' missing"); return; }
    Console.WriteLine($"=== native engine vs bridge: {t.Name} (first 20 rows) ===");
    int limit = Math.Min(20, t.RowCount);
    for (int row = 0; row < limit; row++)
    {
        var rec = s.GetRecord(table, row); if (rec == null) continue;
        for (int i = 0; i < Math.Min(t.Columns.Count, 6); i++)
        {
            var cellText = rec.Get(i);
            var cellBytes = s.GetCellBytes(table, row, t.Columns[i].Name);
            Console.WriteLine($"  row{row} [{i}] {t.Columns[i].Name,-16} text='{cellText}' bytes=[{Hex(cellBytes)}]");
        }
        Console.WriteLine();
    }
}

static void CacheSwitchTest(string folder)
{
    Console.WriteLine("=== cache-switch test (reload same folder; caches must clear) ===");
    using var s1 = new DatabaseSession(); s1.Load(folder);
    var r1 = new NameResolverService(s1);
    int d1 = r1.PlayerNames.DecodableNameCount;
    Console.WriteLine($"session1 decodable={d1}");
    // simulate switch: reload
    using var s2 = new DatabaseSession(); s2.Load(folder);
    var r2 = new NameResolverService(s2);
    int d2 = r2.PlayerNames.DecodableNameCount;
    Console.WriteLine($"session2 decodable={d2}");
    Console.WriteLine($"caches cleared & rebuilt: {(d1 == d2 ? "PASS" : "FAIL")}");
}

static void FullIntegrity(DatabaseSession s, NameResolverService r, SectionDataService data)
{
    Console.WriteLine("=== FULL INTEGRITY ===");
    Console.WriteLine($"tables: {s.Tables.Count}");
    var players = data.GetPlayers();
    var countries = data.GetCountries();
    var teams = data.GetTeams();
    Console.WriteLine($"players={players.Count} countries={countries.Count} teams={teams.Count}");
    Console.WriteLine($"player names decodable from DB: {r.PlayerNames.DecodableNameCount}  placeholders: {r.PlayerNames.PlaceholderNameCount}");
    int resolved = players.Count(p => !p.Title.StartsWith("Player ", StringComparison.Ordinal));
    int bareNum = players.Count(p => int.TryParse(p.Title, out _));
    Console.WriteLine($"players resolved={resolved} fallback={players.Count - resolved} bareNumeric={bareNum}");
    // confederation distribution
    var nt = s.GetTable("nations");
    int conf = Col(nt!, "confederation");
    var dist = new Dictionary<int,int>();
    for (int row = 0; row < nt!.RowCount; row++){ var rec=s.GetRecord("nations",row); if(rec==null)continue; if(int.TryParse(rec.Get(conf),out var c)) dist[c]=dist.GetValueOrDefault(c)+1; }
    Console.WriteLine("confederation raw-code distribution: " + string.Join(", ", dist.OrderBy(kv=>kv.Key).Select(kv=>$"{kv.Key}={kv.Value}")));
    // 13 ground-truth countries
    var truth = new (string name,string expected)[]{("Afghanistan","AFC"),("Malaysia","AFC"),("Japan","AFC"),("Saudi Arabia","AFC"),("England","UEFA"),("Germany","UEFA"),("Brazil","CONMEBOL"),("Argentina","CONMEBOL"),("Morocco","CAF"),("Nigeria","CAF"),("United States","CONCACAF"),("Mexico","CONCACAF"),("New Zealand","OFC")};
    int name = Col(nt, "nationname"), id = Col(nt, "nationid");
    int pass=0, fail=0;
    foreach(var (n,exp) in truth){
        for(int row=0;row<nt.RowCount;row++){ var rec=s.GetRecord("nations",row); if(rec==null)continue; if(!rec.Get(name).Equals(n,StringComparison.OrdinalIgnoreCase))continue; int.TryParse(rec.Get(conf),out var c); var lbl=r.ConfederationLabel(c); bool ok=lbl==exp; Console.WriteLine($"  [{(ok?"PASS":"FAIL")}] {n,-14} code={c} -> {lbl} (exp {exp})"); if(ok)pass++; else fail++; break; }
    }
    Console.WriteLine($"confederation truth: {pass} PASS, {fail} FAIL");
    Console.WriteLine($"bareNumeric player names (must be 0): {bareNum}");
}
