# CM26 â€” Complete Fix Task (Screenshot Audit 2026-08-02)

## Context

CM26 (Creation Master 26) is a WinForms C# database editor for EA Sports FC 26.
Project root: `<repo>`
Main project: `src\CM26.App\CM26.App.csproj`

A screenshot audit of the running application identified 3 issues to fix.

---

## PROTECTED FILES â€” NEVER MODIFY

- `src/database_engine.h`
- `src/database_engine.cpp`
- `tests/engine_smoke.cpp`
- `database/fifa_ng_db.db`
- `database/eng_us.DB`

---

## ISSUE 1: Team Logo Inheritance Bug (PDRM FC shows Bayern Munich crest)

### Evidence
When creating a new team via LeaguesSection (e.g. "PDRM FC" in Malaysia Super League),
the new team's league-list tile shows Bayern Munich's crest instead of a blank/default logo.

### Code Location
`src\CM26.App\Sections\LeaguesSection.cs` â€” `CreateAndLinkTeam()` method (~line 540).

### How team logos are displayed
Team crests are loaded by team ID:
1. `Services.Assets.GetTeamLogo(teamId)` â†’ looks for cached file
2. Fallback: `Services.FrostbiteAssets.ExportLegacyAsset($"data/ui/imgAssets/crest/light/l{teamId}.dds")`
3. Fallback: `SearchAssets` for `logo_*_color` or `crest_{teamId}_` with `ResType == 0x6BDE20BA`
4. Final: `MissingCrest()` draws a gray `?` placeholder

The crest path is `data/ui/imgAssets/crest/light/l{teamId}.dds` â€” based on the TEAM ID, not a field in the record.

### What CreateAndLinkTeam does now
1. `DuplicateRow("teams", 0)` â€” copies template row 0 (a historic club like Bayern Munich)
2. Overrides specific fields: teamid, teamname, countryid, leagueid, assetid, presassetone, presassettwo, captainid, penaltytakerid, freekicktakerid, left/right corner kick takers, stadiumid, managerid, kitids, formationid, prestige, ratings, clubworth, ballid
3. Links team to league via leagueteamlinks

### Root Cause Analysis
The template row 0 is a real team (e.g. Bayern Munich). When we DuplicateRow, ALL fields are copied. We then override most fields, but the logo display is based on the TEAM ID resolving to a DDS file. If `l{newTeamId}.dds` doesn't exist, it should show a placeholder.

However, the screenshot shows Bayern Munich's crest for PDRM FC. This means either:
- The team ID assigned to PDRM FC collides with or points to Bayern Munich's crest file
- OR the league team list is showing the wrong team's logo (display bug in the team picker)

### Task
1. Read `LeaguesSection.cs` fully â€” find how the league's team picker loads and displays team crests (the `PopulateTeamPicker` method and related logo loading code around lines 600-700)
2. Identify why PDRM FC shows Bayern Munich's crest
3. Fix the bug so newly created teams show either their own crest (if it exists) or a blank/placeholder
4. Apply the same fix to `CountriesSection.CreateNationalTeam()` (same issue may exist there)

### Similar code in CountriesSection
`src\CM26.App\Sections\CountriesSection.cs` â€” `CreateNationalTeam()` method (~line 192) uses `CreateRecordFromTemplate("teams", "teamid", ...)` with templateRow: 0. Same template inheritance issue.

---

## ISSUE 2: Compdata Inside Frostbite CAS Archives

### Evidence
The "Open from Game Folder" button fails because compdata TXT files are NOT loose files on disk.
They are packed inside Frostbite CAS archives at:
- `dlc/FootballCompEng/data/compdata/careermode_closedbeta/` â€” 13 TXT files
- `dlc/FootballCompEng/data/compdata/schedules/` â€” 25 TXT files

Verified via FIFA Editor Tool Legacy Explorer:
```
dlc/FootballCompEng/data/compdata/
â”œâ”€â”€ careermode_closedbeta/
â”‚   â”œâ”€â”€ activeteams.txt
â”‚   â”œâ”€â”€ advancement.txt
â”‚   â”œâ”€â”€ compids.txt
â”‚   â”œâ”€â”€ compobj.txt
â”‚   â”œâ”€â”€ dataver.txt
â”‚   â”œâ”€â”€ debug_database.txt
â”‚   â”œâ”€â”€ initteams.txt
â”‚   â”œâ”€â”€ objectives.txt
â”‚   â”œâ”€â”€ schedule.txt
â”‚   â”œâ”€â”€ settings.txt
â”‚   â”œâ”€â”€ standings.txt
â”‚   â”œâ”€â”€ tasks.txt
â”‚   â””â”€â”€ weather.txt
â”œâ”€â”€ schedules/
â”‚   â”œâ”€â”€ C17_S1_2025.txt
â”‚   â”œâ”€â”€ C19_S1_2025.txt
â”‚   â”œâ”€â”€ C20_S1_2025.txt
â”‚   â”œâ”€â”€ C31_S1_2025.txt
â”‚   â”œâ”€â”€ C32_S1_2025.txt
â”‚   â”œâ”€â”€ C39_S1_0.txt
â”‚   â”œâ”€â”€ C53_S1_2025.txt
â”‚   â”œâ”€â”€ C54_S1_2025.txt
â”‚   â”œâ”€â”€ C60_S1_2025.txt
â”‚   â”œâ”€â”€ C61_S1_2025.txt
â”‚   â”œâ”€â”€ C66_S1_2025.txt
â”‚   â”œâ”€â”€ C68_S1_2025.txt
â”‚   â”œâ”€â”€ C152_S1_0.txt
â”‚   â”œâ”€â”€ C153_S1_0.txt
â”‚   â”œâ”€â”€ C308_S1_2025.txt
â”‚   â”œâ”€â”€ C2076_S1_2025.txt
â”‚   â”œâ”€â”€ C2215_S1_2025.txt
â”‚   â”œâ”€â”€ C2216_S1_2025.txt
â”‚   â”œâ”€â”€ C2218_S1_2025.txt
â”‚   â””â”€â”€ C2222_S1_2025.txt
â”œâ”€â”€ Finance/
â”œâ”€â”€ Negotiation/
â”œâ”€â”€ Objectives/
â””â”€â”€ OpponentAnalysis/
```

### Current Code
`src\CM26.App\Sections\ClassicUtilitySections.cs` â€” `OpenCompdataFromGameFolder()` (~line 169):
```csharp
private void OpenCompdataFromGameFolder()
{
    var gameFolder = SettingsService.FC26GameFolder;
    // ...checks...
    var compdataPath = Path.Combine(gameFolder, "dlc", "FootballCompEng", "data", "compdata");
    if (!Directory.Exists(compdataPath))
    {
        MessageBox.Show(this, $"Compdata folder not found at:\n{compdataPath}", ...);
        return;
    }
    _compdata.OpenFromGameFolder(compdataPath);
    // ...
}
```

This reads from loose files. But compdata is inside CAS archives, not loose files.

### Available API
`FrostbiteAssetSession` has these methods:
- `SearchAssets(string query, string? assetType = null, int maximum = 100)` â€” search for assets
- `ExportLegacyAsset(string legacyPath)` â€” export a legacy UI asset by path
- `ExtractAsset(string name, string assetType)` â€” extract any named asset by type

The legacy asset path for compdata TXT files would be something like:
- `dlc/FootballCompEng/data/compdata/careermode_closedbeta/activeteams.txt`
- `dlc/FootballCompEng/data/compdata/schedules/C17_S1_2025.txt`

### Task
1. Read `CompdataWorkbookService.cs` fully â€” understand `OpenFromGameFolder` and `ReadTxtSheet` methods
2. Read `FrostbiteAssetSession.cs` â€” understand `ExportLegacyAsset` and `SearchAssets` methods
3. Modify `OpenCompdataFromGameFolder()` in `ClassicUtilitySections.cs` to:
   a. First try loose files (current behavior)
   b. If loose files not found, try extracting from CAS archives using `FrostbiteAssetSession`
   c. Use `Services.FrostbiteAssets.ExportLegacyAsset(path)` to extract each TXT file
   d. Write extracted content to a temp directory, then read with existing CSV parser
   e. Show honest status: how many sheets loaded, from CAS or loose files
4. If CAS extraction also fails, show clear error message explaining compdata is inside CAS archives

### Key: CompdataWorkbookService
The `OpenFromGameFolder(compdataPath)` method stores the path and reads subdirectories as sheet names.
`ReadTxtSheet(sheetName)` reads `*.txt` files from subdirectories.
We need to either:
- Extract TXT files to a temp dir and point OpenFromGameFolder there, OR
- Add a new method that reads directly from extracted CAS content

---

## ISSUE 3: National Team Detail View

### Evidence
Countries section can "Create National Team" but there's no way to view/edit the national team's details, roster, formation, etc. like club teams.

### Current Code
`src\CM26.App\Sections\CountriesSection.cs`:
- "Create National Team" button at line ~80 calls `CreateNationalTeam()` at line ~192
- After creation, shows message box with team ID but NO navigation to Teams section

### Available Navigation API
In `AppServices.cs`:
```csharp
public event Action<string>? NavigationRequested;        // key -> section
public event Action<string, int>? RecordNavigationRequested; // key, recordIndex -> section
public void RequestRecordNavigation(string key, int recordIndex)
{
    RecordNavigationRequested?.Invoke(key, recordIndex);
}
```

`MainForm.cs` handles these events:
```csharp
_services.NavigationRequested += NavigateTo;
_services.RecordNavigationRequested += NavigateToRecord;
```

`NavigateToRecord` calls `NavigateTo(key)` then `section.GoToRecord(recordIndex)`.

### Task
1. Read `CountriesSection.cs` fully â€” find the "Create National Team" button and `CreateNationalTeam()` method
2. After successful national team creation, add a button or modify the message box to include "Open Team" option
3. When clicked, navigate to Teams section: `Services.RequestRecordNavigation("teams", newTeamRecordIndex)`
4. The Teams section already handles any team record â€” it will display the national team's details, roster, formation, etc.
5. Also add an "Open Team" button for EXISTING national teams (if a country already has a linked national team, show a button to navigate to it)

### Implementation Approach
After `CreateNationalTeam()` succeeds:
```csharp
var result = MessageBox.Show(this, 
    $"{values[0]} was created for {nationName} with Team ID {teamId}.\n\nOpen the new team in Teams section?",
    "Create National Team", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
if (result == DialogResult.Yes)
{
    // Find the record index for the new team
    var teamsTable = Services.Session.GetTable("teams");
    // Navigate to teams section with the new team
    Services.RequestRecordNavigation("teams", /* record index */);
}
```

For existing national teams: add an "Open National Team" button in the country details area that finds the linked national team and navigates to it.

---

## BUILD & TEST

After all fixes, run these commands to verify:

```bash
# Build
dotnet build src\CM26.App\CM26.App.csproj

# Tests (all must exit 0)
dotnet run --project src\CM26.App\CM26.App.csproj -- --name-tests
dotnet run --project src\CM26.App\CM26.App.csproj -- --smoke
dotnet run --project src\CM26.App\CM26.App.csproj -- --nav-test
dotnet run --project src\CM26.App\CM26.App.csproj -- --layout-test
```

---

## CODING RULES

- NEVER modify protected files
- Do NOT add comments unless asked
- Use `Theme.Body` (Segoe UI 9f) for fonts â€” NOT "Microsoft Sans Serif"
- Use `Theme.Panel` for panel backgrounds
- Use `Theme.Accent` for primary buttons
- Follow existing code patterns in each file
- Keep changes minimal â€” fix the bug, don't refactor
- All WinForms controls must be created on the UI thread
