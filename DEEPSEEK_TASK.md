# CM26 Bug Fix + Polish — DeepSeek v4 Flash (Max Effort)

## Project Context

WinForms .NET 8 app at `D:\CM 26 Final`. Main project: `src/CM26.App/`. This is a database editor for EA SPORTS FC 26.

The app has 3 bugs that need fixing. Each fix includes the EXACT file, line numbers, and code before/after. Do NOT change anything else — only the specified lines.

---

## Bug 1: Roster Shows Wrong Team (CRITICAL)

### Problem
When navigating to Teams → select any team → click Roster tab, the player list may show players from a DIFFERENT team.

### Root Cause (2 issues)

**Issue 1A:** `_rosterMinifaces` ImageList (line 27) and `_pendingRosterMinifaces` HashSet (line 28) are NEVER cleared when switching teams. The ImageList accumulates images from every team viewed, causing stale rendering.

**Issue 1B:** `SelectFormationLayout(teamId)` is called BEFORE `LoadLineup(teamId, roster)` at lines 1820-1821. `SelectFormationLayout` calls `RenderLineup()` which reads `_rosterByPlayerId` — but that dictionary still holds the PREVIOUS team's data.

### File: `src/CM26.App/Sections/TeamsSection.cs`

### Fix 1A — Clear stale state at start of ShowRecord

At line 1816, BEFORE `_teamPlayers.Items.Clear();`, add two lines:

```csharp
// ADD THESE TWO LINES at line 1815 (before _teamPlayers.Items.Clear()):
_rosterMinifaces.Images.Clear();
_pendingRosterMinifaces.Clear();
_teamPlayers.Items.Clear();
```

### Fix 1B — Reorder LoadLineup before SelectFormationLayout

Lines 1820-1821 currently read:
```csharp
SelectFormationLayout(teamId);
LoadLineup(teamId, roster);
```

Change to:
```csharp
LoadLineup(teamId, roster);
SelectFormationLayout(teamId);
```

### Fix 1C — Log exceptions instead of swallowing

Line 1872 currently reads:
```csharp
catch { /* Roster/sponsor loading failure must not prevent the record from loading. */ }
```

Change to:
```csharp
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"[TeamsSection] Roster load error: {ex.Message}");
}
```

---

## Bug 2: Minifaces Are Blurry (HIGH)

### Problem
Player face images (minifaces) look blurry/pixelated everywhere — player card, roster list, formation board.

### Root Cause
The app is `PerMonitorV2` DPI-aware (Program.cs line 270), but images are pre-scaled to PictureBox **logical pixel** dimensions. On a 150% DPI display, a 100x100 miniface renders into 150x150 physical pixels — 56% of pixel data is missing, filled by blurry interpolation.

### Fix 2A — DPI-aware scaling in FrostbitePreviewLoader

**File:** `src/CM26.App/Sections/FrostbitePreviewLoader.cs`  
**Line:** 182-183

Current code:
```csharp
private static Image? CreatePreview(AppServices services, string? path, PictureBox viewer, bool linearColor) =>
    CreatePreview(services, path, viewer.Width, viewer.Height, linearColor);
```

Change to:
```csharp
private static Image? CreatePreview(AppServices services, string? path, PictureBox viewer, bool linearColor)
{
    float scale = viewer.DeviceDpi / 96f;
    int w = Math.Max(1, (int)(viewer.Width * scale));
    int h = Math.Max(1, (int)(viewer.Height * scale));
    return CreatePreview(services, path, w, h, linearColor);
}
```

### Fix 2B — High-quality interpolation in CreateCircularMiniface

**File:** `src/CM26.App/Sections/TeamsSection.cs`  
**Lines:** 1451-1464

Current code:
```csharp
private static Image CreateCircularMiniface(Image source, int diameter)
{
    var image = new Bitmap(diameter, diameter);
    using var graphics = Graphics.FromImage(image);
    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    using var path = new System.Drawing.Drawing2D.GraphicsPath();
    path.AddEllipse(1, 1, diameter - 2, diameter - 2);
    graphics.SetClip(path);
    graphics.DrawImage(source, new Rectangle(0, 0, diameter, diameter));
    graphics.ResetClip();
    using var border = new Pen(Color.FromArgb(235, Color.White), 2);
    graphics.DrawEllipse(border, 1, 1, diameter - 3, diameter - 3);
    return image;
}
```

Change to:
```csharp
private static Image CreateCircularMiniface(Image source, int diameter)
{
    var image = new Bitmap(diameter, diameter);
    using var graphics = Graphics.FromImage(image);
    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
    using var path = new System.Drawing.Drawing2D.GraphicsPath();
    path.AddEllipse(1, 1, diameter - 2, diameter - 2);
    graphics.SetClip(path);
    graphics.DrawImage(source, new Rectangle(0, 0, diameter, diameter));
    graphics.ResetClip();
    using var border = new Pen(Color.FromArgb(235, Color.White), 2);
    graphics.DrawEllipse(border, 1, 1, diameter - 3, diameter - 3);
    return image;
}
```

### Fix 2C — High-quality interpolation in LineupMarker.OnPaint

**File:** `src/CM26.App/Sections/TeamsSection.cs`  
**Lines:** 930-942 (inside LineupMarker.OnPaint)

Current code:
```csharp
protected override void OnPaint(PaintEventArgs e)
{
    base.OnPaintBackground(e);
    const int faceSize = 70;
    if (Image != null)
    {
        var width = Math.Min(faceSize, Image.Width);
        var height = Math.Min(faceSize, Image.Height);
        e.Graphics.DrawImage(Image, new Rectangle((Width - width) / 2, 0, width, height));
    }
```

Change to:
```csharp
protected override void OnPaint(PaintEventArgs e)
{
    base.OnPaintBackground(e);
    const int faceSize = 70;
    if (Image != null)
    {
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        var width = Math.Min(faceSize, Image.Width);
        var height = Math.Min(faceSize, Image.Height);
        e.Graphics.DrawImage(Image, new Rectangle((Width - width) / 2, 0, width, height));
    }
```

### Fix 2D — Enforce 2x upscale cap in ScaleToFit

**File:** `src/CM26.Application/Services/TexturePreviewService.cs`  
**Lines:** 147-150

Current code:
```csharp
private static Image ScaleToFit(Image src, int maxW, int maxH)
{
    double ratio = Math.Min((double)maxW / src.Width, (double)maxH / src.Height);
    // Never upscale beyond 2x to keep small icons crisp; always allow downscale.
```

Change to:
```csharp
private static Image ScaleToFit(Image src, int maxW, int maxH)
{
    double ratio = Math.Min((double)maxW / src.Width, (double)maxH / src.Height);
    // Never upscale beyond 2x to keep small icons crisp; always allow downscale.
    ratio = Math.Min(ratio, 2.0);
```

### Fix 2E — Load _overviewFace at correct size instead of copying 100x100

**File:** `src/CM26.App/Sections/PlayersSection.cs`  
**Lines:** 1241-1244

Current code:
```csharp
FrostbitePreviewLoader.LoadLegacyUiAsset(_miniface, Services, image,
    $"data/ui/imgAssets/heads/p{playerId}.dds", (preview, _) =>
{
    if (IsDisposed) { preview?.Dispose(); return; }
    _miniface.Image?.Dispose();
    _miniface.Image = preview;
    _overviewFace.Image?.Dispose();
    _overviewFace.Image = preview == null ? null : new Bitmap(preview);
});
```

Change to:
```csharp
FrostbitePreviewLoader.LoadLegacyUiAsset(_miniface, Services, image,
    $"data/ui/imgAssets/heads/p{playerId}.dds", (preview, _) =>
{
    if (IsDisposed) { preview?.Dispose(); return; }
    _miniface.Image?.Dispose();
    _miniface.Image = preview;
});
FrostbitePreviewLoader.LoadLegacyUiAsset(_overviewFace, Services, image,
    $"data/ui/imgAssets/heads/p{playerId}.dds", (preview, _) =>
{
    if (IsDisposed) { preview?.Dispose(); return; }
    _overviewFace.Image?.Dispose();
    _overviewFace.Image = preview;
});
```

This loads _overviewFace independently at its own size (116x116) instead of stretching the 100x100 _miniface image.

---

## Bug 3: Sidebar Headings Become Transparent After Theme Toggle (MEDIUM)

### Problem
After toggling dark/light theme, sidebar group headings ("World", "Venue", "Team", "Media", "Tools") become transparent, and navigation button text shows through them.

### Root Cause
`Theme.ApplyControlTree()` at line 429 changes ANY Label with `BackColor == SystemColors.Control` to `Color.Transparent`. The sidebar heading Labels have no explicit BackColor set (default is `SystemColors.Control`), so after a theme toggle they become see-through.

### Fix — Set explicit BackColor on heading Labels

**File:** `src/CM26.App/MainForm.cs`  
**Lines:** 186-193

Current code:
```csharp
_sidebarList.Controls.Add(new Label
{
    Text = label,
    AutoSize = true,
    Font = Theme.Label,
    ForeColor = Theme.Muted,
    Margin = new Padding(10, 12, 0, 2),
});
```

Change to:
```csharp
_sidebarList.Controls.Add(new Label
{
    Text = label,
    AutoSize = true,
    Font = Theme.Label,
    ForeColor = Theme.Muted,
    Margin = new Padding(10, 12, 0, 2),
    BackColor = Theme.Background,
});
```

---

## Verification

After applying all fixes, run:
```bash
dotnet build src/CM26.App/CM26.App.csproj -c Release -nologo -v q
```

Must compile with 0 errors. Then test manually:
1. Open app → load database → Teams → select Manchester United → Roster tab → verify correct players
2. Navigate to different team → Roster tab → verify roster updates
3. Players section → check minifaces are sharp (not blurry)
4. Toggle dark/light theme → verify sidebar headings remain visible
