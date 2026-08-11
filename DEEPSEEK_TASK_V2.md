# DeepSeek V4 Flash Max — Bug Fix Task v1.0.97

## Context
WinForms app `CM26` (C#/.NET 8). All source in `src/CM26.App/`. 
Build: `dotnet build src/CM26.App/CM26.App.csproj -c Release`
Self-test: `Release\Creation_Master_26_v1.0.96_Full_Portable\CM26_by_Rizco98.exe --release-selftest`

## BUG 1: Sidebar category labels overlap buttons (ALL sections)
**File:** `src/CM26.App/MainForm.cs` lines 186-194

**Problem:** Category labels ("World", "Venue", "Team", "Media", "Tools") use `AutoSize = true` in a `FlowLayoutPanel`. The `FlowLayoutPanel` doesn't properly account for AutoSize label heights, causing the label text to overlap the `SidebarNavButton` below it.

**Root cause:** `AutoSize = true` on Labels inside `FlowLayoutPanel` with `FlowDirection.TopDown` has known WinForms measurement issues.

**Fix:** Change category labels from `AutoSize = true` to fixed `Height = 20`:

```csharp
// BEFORE (line 186-194):
_sidebarList.Controls.Add(new Label
{
    Text = label,
    AutoSize = true,
    Font = Theme.Label,
    ForeColor = Theme.Muted,
    Margin = new Padding(10, 12, 0, 2),
    BackColor = Theme.Background,
});

// AFTER:
_sidebarList.Controls.Add(new Label
{
    Text = label,
    AutoSize = false,
    Height = 20,
    Width = _sidebarList.ClientSize.Width - 16,
    Font = Theme.Label,
    ForeColor = Theme.Muted,
    Margin = new Padding(10, 12, 0, 2),
    BackColor = Theme.Background,
    TextAlign = ContentAlignment.MiddleLeft,
});
```

Also add to `ResizeSidebarButtons()` (line 318-323) to resize category labels too:
```csharp
private void ResizeSidebarButtons()
{
    var width = Math.Max(120, _sidebarList.ClientSize.Width - 16);
    foreach (var ctrl in _sidebarList.Controls)
    {
        if (ctrl is SidebarNavButton button)
            button.Width = width;
        else if (ctrl is Label label && !label.AutoSize)
            label.Width = width;
    }
}
```

---

## BUG 2: Leagues rating bars (OVR/ATT/MID/DEF) empty/invisible
**File:** `src/CM26.App/Sections/LeaguesSection.cs` lines 321-325

**Problem:** `SetRatingBar` sets `bar.Width = 1` when value is null/empty/0. The bars appear as 1px (nearly invisible).

**Fix:** Show a minimum visible width and display "—" when no data:
```csharp
// BEFORE (line 321-325):
private static void SetRatingBar(Panel bar, string? value, int max)
{
    if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var num) || num <= 0) { bar.Width = 1; return; }
    bar.Width = Math.Max(1, Math.Min(bar.Parent?.Width - 2 ?? 160, (int)((double)num / max * 160)));
}

// AFTER:
private static void SetRatingBar(Panel bar, string? value, int max)
{
    if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var num) || num <= 0)
    {
        bar.Width = Math.Max(8, (bar.Parent?.Width - 2 ?? 160) / 10);
        bar.BackColor = Color.FromArgb(60, bar.BackColor);
        return;
    }
    bar.BackColor = bar.Tag is Color c ? c : bar.BackColor;
    bar.Width = Math.Max(8, Math.Min(bar.Parent?.Width - 2 ?? 160, (int)((double)num / max * 160)));
}
```

Also in `AddRatingBar` (line 333), store the accent color in `barFill.Tag`:
```csharp
barFill.Location = Point.Empty;
barFill.Size = new Size(1, 14);
barFill.BackColor = accent;
barFill.Tag = accent;  // ADD THIS LINE
```

---

## BUG 3: Leagues "Find club to add" overlaps "Clubs" title
**File:** `src/CM26.App/Sections/LeaguesSection.cs` lines 143-166

**Problem:** The second ToolStrip (`teamActions` at Y=52) contains "Find club to add" label that overlaps with the "Clubs" group card title.

**Fix:** Move the Clubs card lower OR move the toolbars inside the card. Simplest fix — increase the Clubs card Y position and adjust internal layout:

Find the line that creates the teamsCard (line 143):
```csharp
// BEFORE:
var teamsCard = CardLayout.CreateGroup(canvas, "Clubs", CardLayout.Fc26Green, 12, 548, 1340, 340);

// AFTER — move card down to avoid overlap with toolbars:
var teamsCard = CardLayout.CreateGroup(canvas, "Clubs", CardLayout.Fc26Green, 12, 580, 1340, 340);
```

And move the toolbars and ListView inside the card instead of on canvas:
```csharp
// Move teamTools, teamActions, and _teams inside teamsCard instead of canvas
teamTools.Location = new Point(4, 26);
teamActions.Location = new Point(4, 52);
_teams.Location = new Point(4, 78);
_teams.Size = new Size(1328, 252);
teamsCard.Controls.Add(teamTools);
teamsCard.Controls.Add(teamActions);
teamsCard.Controls.Add(_teams);
```

---

## BUG 4: Teams Roster — Formation board shows only GK before Refresh
**File:** `src/CM26.App/Sections/TeamsSection.cs` lines 1820-1828

**Problem:** After `LoadLineup` + `SelectFormationLayout`, only GK position shows on the formation pitch. Other players appear only after manual Refresh.

**Root cause:** `LoadLineup` reads `default_teamsheets` but `SelectFormationLayout` may run before lineup data is fully populated, or the formation lookup uses stale team ID.

**Fix:** Ensure `LoadLineup` completes before `SelectFormationLayout`, and force a formation repaint:
```csharp
// BEFORE (lines 1826-1827):
LoadLineup(teamId, roster);
SelectFormationLayout(teamId);

// AFTER:
LoadLineup(teamId, roster);
SelectFormationLayout(teamId);
// Force formation board to repaint with all slots
foreach (var slot in _lineupSlots)
{
    if (slot.PlayerId > 0)
    {
        slot.Label.Visible = true;
        slot.Label.Invalidate();
    }
}
_formationBoard?.Invalidate(true);
```

Also check `LoadLineup` method (lines 1114-1158) — ensure it populates ALL lineup slots, not just the first match. The issue might be that `default_teamsheets` has multiple rows per team (different formations) and only the first is loaded.

Add a guard in `LoadLineup` after line 1130:
```csharp
// After finding the teamsheet row, break to avoid overwriting with later rows:
_activeTeamSheetRow = row;
foreach (var slot in _lineupSlots)
    slot.PlayerId = Parse(record.Get(Col(sheets, slot.PlayerField)));
break;  // ADD THIS — take first matching row only
```

---

## BUG 5: Teams Overview — Stadium/crest image empty
**File:** `src/CM26.App/Sections/TeamsSection.cs` lines 1799-1805

**Problem:** Stadium image doesn't load. `Services.Assets.GetStadium(crestTeamId)` may return null or wrong path.

**Fix:** Add fallback and logging:
```csharp
// BEFORE (lines 1799-1805):
try
{
    var stadiumPath = Services.Assets.GetStadium(crestTeamId);
    LoadKitPreview(_teamStadiumImg, stadiumPath);
}
catch { _teamStadiumImg.Image = null; }

// AFTER:
try
{
    var stadiumPath = Services.Assets.GetStadium(crestTeamId);
    if (!string.IsNullOrWhiteSpace(stadiumPath) && System.IO.File.Exists(stadiumPath))
        LoadKitPreview(_teamStadiumImg, stadiumPath);
    else
        _teamStadiumImg.Image = null;
}
catch { _teamStadiumImg.Image = null; }
```

Also check that `crestTeamId` is correctly parsed from the record. Add debug output:
```csharp
System.Diagnostics.Debug.WriteLine($"[TeamsSection] Stadium load: teamId={crestTeamId}, path={Services.Assets.GetStadium(crestTeamId)}");
```

---

## BUG 6: Countries — sidebar "World" overlaps "Countries" button
Same as BUG 1 — the FlowLayoutPanel AutoSize label issue. Fix in MainForm.cs.

---

## BUG 7: Dashboard — "Report" overlaps "Dashboard"
**File:** `src/CM 26 Final\src\CM26.App\Sections\DashboardSection.cs`

**Problem:** The Dashboard section has a "Report" label or element that overlaps with the sidebar "Dashboard" button. Check if DashboardSection creates controls that bleed into the sidebar area.

**Fix:** Ensure DashboardSection content is properly contained within its panel bounds. Check for any controls with `Dock = DockStyle.Left` or absolute positioning that extends beyond the section panel.

---

## Verification
After all fixes:
1. `dotnet build src/CM26.App/CM26.App.csproj -c Release` — must be 0 errors
2. Run self-test: `Release\Creation_Master_26_v1.0.96_Full_Portable\CM26_by_Rizco98.exe --release-selftest`
3. Visually verify:
   - Sidebar: No text overlap between category labels and buttons
   - Leagues: OVR/ATT/MID/DEF bars show colored fill (not empty)
   - Leagues: "Find club to add" doesn't overlap "Clubs" title
   - Teams Roster: Formation board shows all 11 players on first load
   - Teams Overview: Stadium section shows image (or graceful empty state)
   - Countries: No sidebar overlap
   - Dashboard: No sidebar overlap

## Git
```bash
git add -A
git commit -m "Fix sidebar overlap, rating bars, formation stale-state, stadium loading"
git push origin main
```

Author: `Rizco98 <rizco98@users.noreply.github.com>`
