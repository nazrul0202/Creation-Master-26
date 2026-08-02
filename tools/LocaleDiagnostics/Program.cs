// CM26.LocaleDiagnostics — READ-ONLY locale diagnostic harness.
// Never writes to the original database files. Output goes to LocaleDiagnosticsOutput\ only.
using System.Text;
using CM26.Application.Services;

var dbFolder = @"D:\CM 26 Final\database";
var cmd = args.Length > 0 ? args[0].ToLowerInvariant() : "--full-player-name-test";
var arg = args.Length > 1 ? args[1] : "";

var outDir = @"D:\CM 26 Final\LocaleDiagnosticsOutput";
Directory.CreateDirectory(outDir);

static int Col(CM26.Application.Models.DbTable t, string n)
{
    for (int i = 0; i < t.Columns.Count; i++)
        if (t.Columns[i].Name.Equals(n, StringComparison.OrdinalIgnoreCase)) return i;
    return -1;
}
static string Hex(byte[] b, int max = 48) => string.Join(' ', b.Take(max).Select(x => x.ToString("X2")));
static string Asc(byte[] b, int max = 48) => new string(b.Take(max).Select(x => x >= 32 && x < 127 ? (char)x : '.').ToArray());

try
{
    using var session = new DatabaseSession();
    session.Load(dbFolder);
    var resolver = new NameResolverService(session);

    switch (cmd)
    {
        case "--decrypt-locale":
            Console.WriteLine($"eng_us.DB: {session.LocalePath}");
            Console.WriteLine($"  size: {new FileInfo(session.LocalePath!).Length}");
            Console.WriteLine("  AES-256-CBC decrypt: already performed by engine (key/IV match supplied values)");
            break;
        case "--inspect-header":
            InspectHeader(session);
            break;
        case "--list-language-tables":
            ListLanguageTables(session);
            break;
        case "--export-language-table":
            ExportLanguageTable(session, arg, outDir);
            break;
        case "--trace-string-id":
            TraceStringId(session, arg);
            break;
        case "--trace-player-name":
            TracePlayerName(session, resolver, arg);
            break;
        case "--full-player-name-test":
            FullPlayerNameTest(session, resolver, outDir);
            break;
        default:
            Console.WriteLine("Commands: --decrypt-locale --inspect-header --list-language-tables");
            Console.WriteLine("          --export-language-table <name> --trace-string-id <id>");
            Console.WriteLine("          --trace-player-name <pid> --full-player-name-test");
            return 1;
    }
    return 0;
}
catch (Exception ex) { Console.WriteLine("ERROR: " + ex); return 3; }

static void InspectHeader(DatabaseSession s)
{
    Console.WriteLine("=== eng_us.DB header inspection ===");
    var enc = File.ReadAllBytes(s.LocalePath!);
    Console.WriteLine($"encrypted size: {enc.Length}");
    Console.WriteLine($"encrypted header: {Hex(enc, 16)}");
    // The engine decrypts the whole file. eng_us_decrypted.db should match.
    var decPath = Path.Combine(Path.GetDirectoryName(s.LocalePath!)!, "eng_us_decrypted.db");
    if (File.Exists(decPath))
    {
        var dec = File.ReadAllBytes(decPath);
        Console.WriteLine($"decrypted1 size: {dec.Length}");
        Console.WriteLine($"decrypted1 header: {Hex(dec, 16)}");
        Console.WriteLine($"decrypted1 is T3DB: {dec[0]==0x44 && dec[1]==0x42 && dec[2]==0x00 && dec[3]==0x08}");
    }
}

static void ListLanguageTables(DatabaseSession s)
{
    foreach (var t in s.Tables.Where(x => x.IsLocale))
    {
        Console.WriteLine($"\n=== {t.Name} ===");
        Console.WriteLine($"  rows: {t.RowCount}  cols: {t.Columns.Count}");
        for (int i = 0; i < t.Columns.Count; i++)
            Console.WriteLine($"  [{i}] {t.Columns[i].Name,-16} kind={t.Columns[i].KindLabel} depth={t.Columns[i].Depth}");
    }
}

static void ExportLanguageTable(DatabaseSession s, string tableName, string outDir)
{
    var t = s.Tables.FirstOrDefault(x => x.IsLocale && x.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
    if (t == null) { Console.WriteLine($"table {tableName} not found"); return; }
    int sid = Col(t, "stringid"), txt = Col(t, "sourcetext"), hash = Col(t, "hashid");
    var sb = new StringBuilder();
    sb.AppendLine($"# {tableName}: {t.RowCount} rows");
    sb.AppendLine($"# columns: stringid={sid} sourcetext={txt} hashid={hash}");
    int limit = Math.Min(t.RowCount, 50);
    for (int r = 0; r < limit; r++)
    {
        var rec = s.GetRecord(tableName, r);
        if (rec == null) continue;
        var sidB = sid >= 0 ? s.GetCellBytes(tableName, r, "stringid") : Array.Empty<byte>();
        var txtB = txt >= 0 ? s.GetCellBytes(tableName, r, "sourcetext") : Array.Empty<byte>();
        sb.AppendLine($"row{r}: stringid_hex=[{Hex(sidB)}] stringid_asc='{Asc(sidB)}' sourcetext_hex=[{Hex(txtB)}] sourcetext_asc='{Asc(txtB)}'");
    }
    var path = Path.Combine(outDir, $"{tableName}_dump.txt");
    File.WriteAllText(path, sb.ToString());
    Console.WriteLine($"Wrote {limit} rows to {path}");

    // Byte-frequency analysis on sourcetext across a sample
    var freq = new Dictionary<byte, int>();
    var rng = new Random(3);
    for (int i = 0; i < 500; i++)
    {
        var b = s.GetCellBytes(tableName, rng.Next(t.RowCount), "sourcetext");
        foreach (var x in b) freq[x] = freq.GetValueOrDefault(x) + 1;
    }
    Console.WriteLine($"distinct byte values in sourcetext (500-row sample): {freq.Count}");
    Console.WriteLine($"top 15: {string.Join(", ", freq.OrderByDescending(kv => kv.Value).Take(15).Select(kv => $"0x{kv.Key:X2}='{(kv.Key>=32&&kv.Key<127?(char)kv.Key:'.')}'({kv.Value})"))}");
    // Is this a 44-symbol cipher alphabet or real text?
    Console.WriteLine($"alphabet size: {(freq.Count < 60 ? "CIPHERED (small alphabet)" : "real text (large alphabet)")}");
}

static void TraceStringId(DatabaseSession s, string idStr)
{
    foreach (var tn in new[] { "LanguageStrings1", "LanguageStrings2" })
    {
        var t = s.Tables.FirstOrDefault(x => x.IsLocale && x.Name.Equals(tn, StringComparison.OrdinalIgnoreCase));
        if (t == null) continue;
        int sid = Col(t, "stringid"), txt = Col(t, "sourcetext"), hash = Col(t, "hashid");
        for (int r = 0; r < t.RowCount; r++)
        {
            var rec = s.GetRecord(tn, r); if (rec == null) continue;
            if (rec.Get(sid) == idStr || rec.Get(hash) == idStr)
            {
                var sidB = s.GetCellBytes(tn, r, "stringid");
                var txtB = s.GetCellBytes(tn, r, "sourcetext");
                Console.WriteLine($"FOUND in {tn} row {r}:");
                Console.WriteLine($"  stringid: '{rec.Get(sid)}' hex=[{Hex(sidB)}]");
                Console.WriteLine($"  hashid:   '{rec.Get(hash)}'");
                Console.WriteLine($"  sourcetext: '{rec.Get(txt)}' hex=[{Hex(txtB)}] asc='{Asc(txtB)}'");
                return;
            }
        }
    }
    Console.WriteLine($"string/hash id '{idStr}' not found in any language table");
}

static void TracePlayerName(DatabaseSession s, NameResolverService r, string idStr)
{
    if (!int.TryParse(idStr, out var pid)) { Console.WriteLine("need numeric player id"); return; }
    var t = s.GetTable("players");
    int pidC = Col(t!, "playerid"), fn = Col(t!, "firstnameid"), ln = Col(t!, "lastnameid"), cn = Col(t!, "commonnameid");
    for (int row = 0; row < t!.RowCount; row++)
    {
        var rec = s.GetRecord("players", row); if (rec == null) continue;
        if (!int.TryParse(rec.Get(pidC), out var p) || p != pid) continue;
        int f = int.TryParse(rec.Get(fn), out var fv) ? fv : 0;
        int l = int.TryParse(rec.Get(ln), out var lv) ? lv : 0;
        int c = int.TryParse(rec.Get(cn), out var cv) ? cv : 0;
        Console.WriteLine($"=== Player {pid} ===");
        Console.WriteLine($"  firstnameid={f}  lastnameid={l}  commonnameid={c}");
        // playernames raw bytes
        foreach (var (nid, label) in new[] { (f, "first"), (l, "last"), (c, "common") })
            if (nid > 0) Console.WriteLine($"  {label}nameid={nid}: {DescribeName(s, nid)}");
        // resolver
        var parts = r.PlayerNameParts(pid, f, l, c);
        Console.WriteLine($"  resolver: first='{parts.FirstName}' last='{parts.LastName}' common='{parts.CommonName}' knownAs='{parts.KnownAs}'");
        Console.WriteLine($"  display: '{r.PlayerDisplayName(pid, f, l, c)}'");
        return;
    }
    Console.WriteLine($"player {pid} not found");
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
        return $"text='{rec.Get(nameC)}' hex=[{Hex(b)}] asc='{Asc(b)}'";
    }
    return "nameid not found";
}

static void FullPlayerNameTest(DatabaseSession s, NameResolverService r, string outDir)
{
    var data = new SectionDataService(s, r, new PendingChangesService(s));
    var players = data.GetPlayers();
    int resolved = players.Count(p => !p.Title.StartsWith("Player ", StringComparison.Ordinal));
    int fallback = players.Count - resolved;
    int bareNum = players.Count(p => int.TryParse(p.Title, out _));
    Console.WriteLine($"Total players: {players.Count}");
    Console.WriteLine($"Resolved names: {resolved}");
    Console.WriteLine($"Fallback (Player {{id}}): {fallback}");
    Console.WriteLine($"Bare numeric: {bareNum}");
    Console.WriteLine($"Decodable (from DB): {r.PlayerNames.DecodableNameCount}");
    Console.WriteLine($"Placeholder: {r.PlayerNames.PlaceholderNameCount}");
    Console.WriteLine($"Locale strings indexed: {r.PlayerNames.Source.LocaleStringCount}");

    // sample
    var sb = new StringBuilder();
    foreach (var p in players.Take(20))
        sb.AppendLine($"  #{p.RecordIndex}  {p.Title}  | {p.Detail}");
    File.WriteAllText(Path.Combine(outDir, "player_name_sample.txt"), sb.ToString());
    Console.WriteLine($"Sample written to {outDir}\\player_name_sample.txt");
}
