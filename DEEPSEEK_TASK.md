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

## Bug 4: Kits Section — Stale Texture Applied to Wrong Kit (HIGH)

### Problem
In the Kits section, if the user clicks "Load Texture" on kit A, then quickly selects kit B before the load finishes, kit A's texture gets applied to kit B's preview.

### Root Cause
The button-triggered load at line 927 calls `LoadFrostbitePreviewAsync()` with the DEFAULT token (no cancellation). Only the AUTO-load path (triggered by `OnRecordShown`) passes `_previewCancellation.Token`. So a button-triggered in-flight load survives record changes and applies stale data.

### File: `src/CM26.App/Sections/ClassicEntitySections.cs`

### Fix — Pass the shared cancellation token from the button handler

Line 927 currently reads:
```csharp
_loadTexture.Click += async (_, _) => await LoadFrostbitePreviewAsync();
```

Change to:
```csharp
_loadTexture.Click += async (_, _) => await LoadFrostbitePreviewAsync(_previewCancellation?.Token ?? default);
```

This makes the button-triggered load respect the same per-record cancellation that `OnRecordShown` (lines 1052-1058) sets up, so a stale texture can never apply to a different kit.

---

## Bug 5: Leagues Section — Club Count Shows Previous League (HIGH)

### Problem
When switching between leagues, the club count label under the new league's header shows the PREVIOUS league's club count briefly (or permanently if the new league has clubs).

### Root Cause
Line 292 reads `_teams.Items.Count` BEFORE `_teams.Items.Clear()` at line 294. The label is set from the previous league's list, then the list is cleared and repopulated.

### File: `src/CM26.App/Sections/LeaguesSection.cs`

### Fix — Move the club count update after repopulation

Lines 292-298 currently read:
```csharp
_leagueClubsLabel.Text = _teams.Items.Count > 0 ? _teams.Items.Count.ToString() : "—";

_teams.Items.Clear();
_leagueId = int.TryParse(record.Get(Col(table, "leagueid")), out var id) ? id : 0;
PopulateTeamLinks();
PopulateTeamPicker();
if (_teams.Items.Count == 0) _teams.Items.Add("No teams linked in leagueteamlinks");
```

Change to:
```csharp
_teams.Items.Clear();
_leagueId = int.TryParse(record.Get(Col(table, "leagueid")), out var id) ? id : 0;
PopulateTeamLinks();
PopulateTeamPicker();
if (_teams.Items.Count == 0) _teams.Items.Add("No teams linked in leagueteamlinks");
_leagueClubsLabel.Text = _teams.Items.Count > 0 ? _teams.Items.Count.ToString() : "—";
```

---

## Bug 6: Leagues Section — ImageList & Pending HashSet Never Cleared (MEDIUM)

### Problem
`_teamImages` ImageList (line 26) and `_pendingTeamCrests` HashSet (line 27) are never cleared when switching leagues. Causes:
1. Unbounded memory growth — crest images accumulate for every league visited
2. Failed crest loads (task returns null) never retry — the placeholder persists forever because the teamId stays in the pending set

### File: `src/CM26.App/Sections/LeaguesSection.cs`

### Fix 6A — Clear both at the start of ShowRecord

At line 292 (the `_leagueClubsLabel` line, before `_teams.Items.Clear()`), add:
```csharp
_teamImages.Images.Clear();
_pendingTeamCrests.Clear();
_teams.Items.Clear();
```

Replace lines 292-294:
```csharp
_leagueClubsLabel.Text = _teams.Items.Count > 0 ? _teams.Items.Count.ToString() : "—";

_teams.Items.Clear();
```

with:
```csharp
_teamImages.Images.Clear();
_pendingTeamCrests.Clear();
_teams.Items.Clear();
```

### Fix 6B — Remove playerId from pending set even on null result

In `QueueFc26TeamCrest` (lines 823-854), the continuation currently returns early when `task.Result == null`:
```csharp
}).ContinueWith(task =>
{
    if (IsDisposed || task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
```

Change to:
```csharp
}).ContinueWith(task =>
{
    if (IsDisposed) return;
    if (task.Status != TaskStatus.RanToCompletion || task.Result == null)
    {
        _pendingTeamCrests.Remove(teamId);
        return;
    }
```

This allows a failed crest load to be retried on the next visit instead of being suppressed forever.

---

## Bug 7: Players Section — 3D Face Search Reads Stale Player (MEDIUM)

### Problem
If the user clicks "3D Face Viewer" on player A, then selects player B while the search is running, the viewer may launch with player B's head model under player A's flow.

### Root Cause
Line 926: `var detected = await Task.Run(FindExtractedFaceFolder);` — `FindExtractedFaceFolder` reads the `_currentHeadAssetId`/`_currentPlayerId` FIELDS at execution time, not capture time. If the user switches players mid-search, the fields already contain the NEW player's IDs.

### File: `src/CM26.App/Sections/PlayersSection.cs`

### Fix — Capture IDs before the await and pass them as parameters

Lines 916-926 currently read:
```csharp
var exported = await Task.Run(() => Services.FrostbiteAssets.ExportMeshForQuery(
    new[] { $"head_{_currentHeadAssetId}", $"head_{_currentPlayerId}" }));
if (IsDisposed) return;
if (!string.IsNullOrWhiteSpace(exported))
{
    _facePreviewCaption.Text = "3D head mesh exported · opening viewer…";
    Launch3DViewer(executable, exported);
    return;
}
_facePreviewCaption.Text = "Searching for the selected player's extracted 3D face…";
var detected = await Task.Run(FindExtractedFaceFolder);
if (IsDisposed) return;
if (!string.IsNullOrWhiteSpace(detected))
{
    _facePreviewCaption.Text = $"3D face assets found · {detected}";
    Launch3DViewer(executable, detected);
    return;
}
```

Change to:
```csharp
var headAssetId = _currentHeadAssetId;
var playerId = _currentPlayerId;
var exported = await Task.Run(() => Services.FrostbiteAssets.ExportMeshForQuery(
    new[] { $"head_{headAssetId}", $"head_{playerId}" }));
if (IsDisposed || playerId != _currentPlayerId) return;
if (!string.IsNullOrWhiteSpace(exported))
{
    _facePreviewCaption.Text = "3D head mesh exported · opening viewer…";
    Launch3DViewer(executable, exported);
    return;
}
_facePreviewCaption.Text = "Searching for the selected player's extracted 3D face…";
var detected = await Task.Run(() => FindExtractedFaceFolder(headAssetId, playerId));
if (IsDisposed || playerId != _currentPlayerId) return;
if (!string.IsNullOrWhiteSpace(detected))
{
    _facePreviewCaption.Text = $"3D face assets found · {detected}";
    Launch3DViewer(executable, detected);
    return;
}
```

Also update the `FindExtractedFaceFolder` method signature to accept the IDs as parameters instead of reading fields:
```csharp
// Find the current signature (reads _currentHeadAssetId / _currentPlayerId internally)
// and change it to:
private string FindExtractedFaceFolder(int headAssetId, int playerId)
```

---

## Bug 8: Audio / NewWave Bank — Stale Bank Inspection (MEDIUM)

### Problem
In the Audio section, if the user clicks "Inspect" on bank A, then selects bank B while parsing runs, the results apply to bank B's details panel but contain bank A's data.

### Root Cause
Line 386: `var bank = await Task.Run(() => Services.FrostbiteAssets.InspectNewWaveBank(_selectedBank));` — `_selectedBank` is read at execution time inside the Task.Run, not captured before.

### File: `src/CM26.App/Sections/Fc26ExtensionSections.cs`

### Fix — Capture the bank name before the await

Lines 386-404 currently read:
```csharp
var bank = await Task.Run(() =>
    Services.FrostbiteAssets.InspectNewWaveBank(_selectedBank));
if (bank == null)
    throw new InvalidDataException("The selected RES is not a supported NewWave bank.");
_extractedPath = bank.ExtractedPath;
_dataSets.Items.Clear();
```

Change to:
```csharp
var bankName = _selectedBank;
var bank = await Task.Run(() =>
    Services.FrostbiteAssets.InspectNewWaveBank(bankName));
if (bank == null)
    throw new InvalidDataException("The selected RES is not a supported NewWave bank.");
if (bankName != _selectedBank) return;  // user switched banks mid-inspection
_extractedPath = bank.ExtractedPath;
_dataSets.Items.Clear();
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
5. Kits section → click Load Texture on kit A → quickly select kit B → verify kit B's texture loads, not kit A's
6. Leagues section → switch between leagues → verify club count shows the CURRENT league's clubs
7. Players section → open 3D Face Viewer on player A → quickly select player B → verify the viewer opens for player B only
8. Audio section → inspect bank A → quickly select bank B → verify bank B's details are shown
