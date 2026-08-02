using CM26.Application.Services;

// Managed end-to-end smoke test against the REAL FC26 database (read-only; no writes).
var dbFolder = args.Length > 0 ? args[0] : @"D:\CM 26 Final\database";
Console.WriteLine($"Folder: {dbFolder}");

using var session = new DatabaseSession();
var summary = session.ValidateFolder(dbFolder);
Console.WriteLine($"Validate: state={summary.State} tables={summary.TableCount}");
if ((int)summary.State != 0) { Console.WriteLine("FAIL: " + summary.Message); return 2; }

session.Load(dbFolder);
Console.WriteLine($"Loaded. main tables+locale tables = {session.Tables.Count}");

var resolver = new NameResolverService(session);
var pending = new PendingChangesService(session);
var data = new SectionDataService(session, resolver, pending);

var players = data.GetPlayers();
Console.WriteLine($"Players: {players.Count}");
foreach (var p in players.Take(8))
    Console.WriteLine($"  #{p.RecordIndex}  {p.Title}  | {p.Subtitle} | {p.Detail}");

var teams = data.GetTeams();
Console.WriteLine($"Teams: {teams.Count}");
foreach (var t in teams.Take(8))
    Console.WriteLine($"  #{t.RecordIndex}  {t.Title}  | {t.Subtitle} | {t.Detail}");

var nations = data.GetCountries();
Console.WriteLine($"Nations: {nations.Count}  first: {(nations.Count>0?nations[0].Title:"-")}");

var stadiums = data.GetStadiums();
Console.WriteLine($"Stadiums: {stadiums.Count}  first: {(stadiums.Count>0?stadiums[0].Title:"-")}");

var leagues = data.GetLeagues();
Console.WriteLine($"Leagues: {leagues.Count}  first: {(leagues.Count>0?leagues[0].Title:"-")}");

var managers = data.GetManagers();
Console.WriteLine($"Managers: {managers.Count}");
foreach (var m in managers.Take(5)) Console.WriteLine($"   {m.Title} | {m.Subtitle} | {m.Detail}");

var referees = data.GetReferees();
Console.WriteLine($"Referees: {referees.Count}  first: {(referees.Count>0?referees[0].Title:"-")}");

var kits = data.GetKits();
Console.WriteLine($"Kits: {kits.Count}  first: {(kits.Count>0?kits[0].Title:"-")}");

var formations = data.GetFormations();
Console.WriteLine($"Formations: {formations.Count}  first: {(formations.Count>0?formations[0].Title:"-")}");

Console.WriteLine("SMOKE OK");
return 0;
