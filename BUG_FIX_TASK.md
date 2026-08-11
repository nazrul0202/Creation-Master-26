# CM26 Bug Fix Task — DeepSeek Flash v4 Max Effort

## Context

This is a WinForms (.NET 8) app called "Creation Master 26" — a database editor for EA SPORTS FC 26. The codebase is at `D:\CM 26 Final`. The main app project is `src\CM26.App/`.

I've completed a thorough audit and identified **3 critical bugs** that need fixing. Each bug has a root cause, affected files, and specific fix instructions. Please fix ALL THREE bugs in one pass.

---

## Bug 1: Roster Shows Wrong Team Players (CRITICAL)

### Symptom
When navigating to Teams section and selecting a team (e.g., Manchester United), the Roster tab shows players from a DIFFERENT team. The user must search again to find the correct roster.

### Root Cause
In `src/CM26.App/Sections/TeamsSection.cs`, the `ShowRecord()` method (around line 1692) has these issues:

1. **`_rosterMinifaces` ImageList is NEVER cleared** between team selections. The ImageList at line 27 accumulates images from every team viewed. While `ImageKey`-based lookup should still work, the growing ImageList causes WinForms rendering inconsistencies.

2. **`_pendingRosterMinifaces` HashSet is NEVER cleared** (line 28). Stale async load entries persist across team selections.

3. **`SelectFormationLayout()` is called BEFORE `LoadLineup()`** (lines 1820-1821). `SelectFormationLayout` calls `RenderLineup()` which reads from `_rosterByPlayerId` — but this dictionary still contains the PREVIOUS team's data at that point. Although both calls are synchronous, this creates a window of inconsistent state.

4. **Silent exception swallowing** at line 1872: `catch { /* swallowed */ }` hides ALL errors in roster loading, making bugs invisible.

### Fix Instructions

In `src/CM26.App/Sections/TeamsSection.cs`:

**A.** At the START of `ShowRecord()` (before line 1816 where `_teamPlayers.Items.Clear()` is), add:
```csharp
_rosterMinifaces.Images.Clear();
_pendingRosterMinifaces.Clear();
```

**B.** REORDER lines 1820-1821 so `LoadLineup` runs BEFORE `SelectFormationLayout`:
```csharp
// BEFORE (buggy):
SelectFormationLayout(teamId);   // line 1820 — reads stale _rosterByPlayerId
LoadLineup(teamId, roster);      // line 1821 — populates _rosterByPlayerId

// AFTER (fixed):
LoadLineup(teamId, roster);      // populates _rosterByPlayerId FIRST
SelectFormationLayout(teamId);   // now reads correct data
```

**C.** Replace the silent catch at line 1872 with logging:
```csharp
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"[TeamsSection] Roster load error: {ex}");
}
```

---

## Bug 2: Minifaces Are Blurry (HIGH)

### Symptom
Player face images (minifaces) appear blurry/pixelated throughout the app — in the player info card, roster list, and formation board.

### Root Cause
The app uses `PerMonitorV2` DPI awareness (Program.cs line 270), but the image loading pipeline always pre-scales images to the PictureBox's **logical pixel** dimensions. On any display above 96 DPI (nearly all modern displays), the pre-scaled image has fewer pixels than the physical render area. WinForms then upscales using default Bilinear interpolation, causing blur.

Concrete example: On a 150% DPI display, a 100x100 miniface renders into 150x150 physical pixels — 56% of the needed pixel data is missing.

Additional issues:
- `CreateCircularMiniface()` doesn't set `InterpolationMode` (defaults to low-quality Bilinear)
- `_overviewFace` (116x116) receives a 100x100 image — 16% upscale even at 96 DPI
- `TexturePreviewService.ScaleToFit()` has a comment "Never upscale beyond 2x" but doesn't enforce it — the large face preview upscales 128x128 to 390x390 (3x)

### Fix Instructions

**A.** In `src/CM26.App/Sections/FrostbitePreviewLoader.cs`, line 182-183, account for DPI:
```csharp
private static Image? CreatePreview(AppServices services, string? path, PictureBox viewer, bool linearColor)
{
    float scale = viewer.DeviceDpi / 96f;
    int w = Math.Max(1, (int)(viewer.Width * scale));
    int h = Math.Max(1, (int)(viewer.Height * scale));
    return CreatePreview(services, path, w, h, linearColor);
}
```

**B.** In `src/CM26.App/Sections/TeamsSection.cs`, `LoadPlayerMinifaceAsync()` around line 1446, apply the same DPI scaling:
```csharp
// Find where CreatePreview is called with a size parameter for lineup/roster minifaces
// and multiply the size by DPI scale factor (DeviceDpi / 96f)
```

**C.** In `src/CM26.App/Sections/TeamsSection.cs`, `CreateCircularMiniface()` around line 1451, add interpolation:
```csharp
private static Image CreateCircularMiniface(Image source, int diameter)
{
    var image = new Bitmap(diameter, diameter);
    using var graphics = Graphics.FromImage(image);
    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
    // ... rest of existing code
}
```

**D.** In `src/CM26.App/Sections/TeamsSection.cs`, `LineupMarker.OnPaint()` around line 930, add interpolation:
```csharp
e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
```

**E.** In `src/CM26.Application/Services/TexturePreviewService.cs`, `ScaleToFit()` around line 147, enforce the 2x upscale cap:
```csharp
double ratio = Math.Min((double)maxW / src.Width, (double)maxH / src.Height);
ratio = Math.Min(ratio, 2.0);  // enforce documented 2x cap
```

**F.** In `src/CM26.App/Sections/PlayersSection.cs`, `ShowRecord()` around lines 1241-1244, load `_overviewFace` independently instead of copying the 100x100 preview:
```csharp
// Instead of: _overviewFace.Image = preview == null ? null : new Bitmap(preview);
// Load at the correct size for _overviewFace (116x116 or DPI-scaled)
FrostbitePreviewLoader.CreatePreview(Services, image, _overviewFace, linearColor: false);
```
(You may need to make `CreatePreview` public or add a helper method.)

---

## Bug 3: Sidebar Headings Overlap With Navigation Buttons (MEDIUM)

### Symptom
After toggling the dark/light theme, the sidebar group headings ("World", "Venue", "Team", "Media", "Tools") become transparent, and the navigation button text shows through them.

### Root Cause
In `src/CM26.App/Theming/Theme.cs`, `ApplyControlTree()` at line 429:
```csharp
case Label label:
    if (label.BackColor == SystemColors.Control)
        label.BackColor = Color.Transparent;  // THIS MAKES HEADING LABELS TRANSPARENT
```

This changes ALL Labels with default `BackColor` to transparent — including the sidebar heading Labels that were created without an explicit BackColor. After a theme toggle, the headings become see-through, and the SidebarNavButton text behind them becomes visible.

### Fix Instructions

In `src/CM26.App/Theming/Theme.cs`, `ApplyControlTree()` around line 429, add an exclusion for sidebar headings. The simplest fix: check if the label's parent is a FlowLayoutPanel with a specific tag, OR set explicit BackColor on the heading Labels at creation time.

**Option A (preferred — fix at creation):** In `src/CM26.App/MainForm.cs`, line 186-193, add explicit BackColor to the heading Labels:
```csharp
_sidebarList.Controls.Add(new Label
{
    Text = label,
    AutoSize = true,
    Font = Theme.Label,
    ForeColor = Theme.Muted,
    Margin = new Padding(10, 12, 0, 2),
    BackColor = Theme.Background,  // ADD THIS — prevents ApplyControlTree from making it transparent
});
```

**Option B (fix at source):** In Theme.cs ApplyControlTree, exclude labels inside FlowLayoutPanels:
```csharp
case Label label:
    // Don't make labels transparent if they're inside a FlowLayoutPanel
    // (sidebar headings need opaque backgrounds to avoid overlap with buttons)
    if (label.BackColor == SystemColors.Control && label.Parent is not FlowLayoutPanel)
        label.BackColor = Color.Transparent;
```

---

## Additional Notes

- The project uses `CM26.App.Controls.CardLayout` for card-based UI (static class with shared colors)
- `Theme.cs` is the central design system — palette, typography, spacing
- `SectionBase.cs` is the base class for all editor sections
- `AppServices.cs` is the lightweight DI container passed to all sections
- The app uses embedded resources for icons via `IconService.cs`
- All sections are disposed and rebuilt on theme toggle (ApplyThemeMode in MainForm)
- The database is in-memory (DbTable objects) — no SQL, just row scanning
- Build: `dotnet build src/CM26.App/CM26.App.csproj -c Release`
- Self-test: `CM26_by_Rizco98.exe --release-selftest` (10 checks, no game needed)

## Verification

After fixing all 3 bugs, run:
```bash
dotnet build src/CM26.App/CM26.App.csproj -c Release -nologo
```

Then test manually:
1. Open the app, load a game database
2. Navigate to Teams → select Manchester United → click Roster tab → verify correct players
3. Navigate to a different team → verify roster updates correctly
4. Check player minifaces in Players section — should be sharp, not blurry
5. Toggle dark/light theme → verify sidebar headings remain visible and opaque
