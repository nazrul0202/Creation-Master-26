# HANDOVER — CM26 Studio WPF → GPT 5.6 Luna (medium effort)

Ditulis: 15/8/2026. Baca AGENTS.md dulu (peraturan wajib ada di bawah).

## 1. Projek & Peraturan (AGENTS.md ringkas)
- Workspace: `D:\CM 26 Final`. Public repo: `D:\CM 26 Final\GitHub_CM26\Creation-Master-26` (origin https://github.com/nazrul0202/Creation-Master-26.git, branch `main`).
- **WAJIB**: selepas setiap update → copy tracked files live → repo, commit identity `Rizco98 <rizco98@users.noreply.github.com>` (`-c user.name=... -c user.email=...`), push `git push origin main`. Jangan tinggal un-pushed. (CRLF/LF warnings OK.)
- Release packages HANYA di `D:\CM 26 Final\Release`. Versi public: 1.0.109 (jangan ubah tanpa arahan; sync `CM26.App.csproj` + Assembly/File/Informational versions).
- Solution: `D:\CM 26 Final\CM26.slnx` (bukan .sln). Engine native: `CM26.EngineBridge.dll` (outcome.Success/outcome.Message).
- Build check: `dotnet build src\CM26.WPF\CM26.WPF.csproj -c Debug` (0 error) → `& src\CM26.WPF\bin\Debug\net8.0-windows\CM26.Studio.exe --ui-smoke` (expect EXIT=0 "SMOKE OK") → `dotnet test CM26.slnx` (27 PASS). Lint warnings CS8602 sedia ada (4) — jangan tambah error baharu.
- User komunikasi: Bahasa Melayu/Indonesia.

## 2. Matlamat aktif
User mahu Studio (`CM26.Studio.exe`, WPF) **100% exact macam Creation Master 16** (bukan FC26!): shell, menu, toolbar, status bar, theme, dan setiap section form (group/tab/label/field).

## 3. Status sekarang (selesai & verified)
- Git: `919ea96` pushed ke GitHub, working tree CLEAN. (19 files: MainWindow + 9 views parity pass.)
- Build WPF 0 err; smoke EXIT=0; 27 tests PASS.
- Selesai:
  - Menu bar = CM16 exact (File: Open-FIFA16/Open-lan.db/Open-All/Open-Recent/Save/Close/Exit; Tools 22 visible item; Patch; Online Update submenu 21/20/19/18/16; Help). Ikon menu 16×16 CM16. MenuPatch/MenuUpdateDB disabled sebelum open.
  - Startup no auto-load (CM16): toolbar+workspace disabled, hanya Open + Regenerate aktif, status "Ready".
  - Theme: SkyBlueBrush #87CEEB, panel LightSkyBlue, base #F0F0F0 (SystemColors.Control).
  - Toolbar = 20 butang CM16 (52×52, ikon 48×48), RadioButton `GroupName="Sec"` Tag="section-key"; Sponsor/Tv/ImportGraphics/dashboard `Visibility="Collapsed"` (dashboard ikut CM16 = tiada).
  - Window: Title "Creation Master 26", 1384×961, Min 200×199.
  - Status bar = ProgressBar (hidden, visible masa open) + StatusBarText "Ready" (DbStatusText/PendingCountText DIBUANG).
  - Right panel (Width 24 collapsed) + Bottom panel (Height 25 collapsed) dengan button show/hide + StripLabel "Empty" (CM16 toolStripBottom/Right).
  - Section views diselaraskan ikut CM16: Players (Info/Skills/Face — Face Modelling nested Face Type/Hair Model and Color/Head Model, Skills + Traits+Virtual Pro), Team (Generic: Logos/Name/Stadium/Manager/Info/Last Year Performance/Location/Team Traits/Kit Links; Roster: Defense/BuildUp/Chance Creation/Formation/Set Pieces + roster list; Adboards; Flags; Rev. Mod. Extensions), Referee (Identity+nested Shoes, Leagues, Face Modelling nested), Kit (Positions/Jersey/Name/3D Model), League (Teams/Names and Other Information + objectives/Tuning/Switch), Manager (splitContainer + 3D Model), Country (Country/Map(Audio)/National Team tabs), Audio (+Patching, Sound Split and Merge), Newspaper ("Newspapers"/"News Sponsor"), Formation (+Instructions), Stadium & Tournament sudah padan.

## 4. TUGAS AKTIF (in progress — sambung di sini)
**Bina PickUpControl WPF** — komponen signature CM16 (combo + toolbar search/create/delete/clone), ganti layout "Find: + ListView" di semua view.

Fakta dari `D:\CM16 FULL DECOMPILE\FifaControls\PickUpControl.cs` (788 baris, sudah dianalisis):
- ToolStrip 25px, GripStyle.Hidden, 18 items urutan:
  `separatorBegin, combo(200×25, DropDownHeight 256, MaxDropDownItems 16), buttonRefresh, separatorSearch, buttonCaseSensitive(CheckOnClick), textSearch(150×25), buttonSearchExactly, buttonSearchStart, buttonSearchContain, separatorButtons, buttonNew, buttonRemove, buttonClone, buttonWizard, separatorFilter, labelFilter("Filter"), comboFilterBy(120×25), comboFilterValue(160×25)`
- Semua button DisplayStyle=Image, 23×22, ikon 16×16. ToolTipText TIDAK diset (kosong) — hanya Text internal (Refresh/Case sensitive/Create/Remove/Clone/Wizard) yang boleh jadi tooltip.
- Ikon tersedia di `src\CM26.WPF\Assets\Cm16\`:
  `FifaControls_PickUpControl__buttonRefresh_Image.png`, `__buttonCaseSensitive_Image.png`, `__buttonSearchExactly_Image.png`, `__buttonSearchStart_Image.png`, `__buttonSearchContain_Image.png`, `__buttonNew_Image.png`, `__buttonRemove_Image.png`, `__buttonClone_Image.png`, `__buttonWizard_Image.png`, `__toolStrip_BackgroundImage.png`
- Callbacks (delegate PickUpCallback(object sender, object obj)): SelectObject, CreateObject, DeleteObject, CloneObject, RefreshObject, FilterChanged, WizardObject.
- Flags: MainSelectionEnabled, FilterEnabled, SearchEnabled, CreateButtonEnabled, RemoveButtonEnabled, CloneButtonEnabled, RefreshButtonEnabled, WizardButtonEnabled.
- Search(): mode Exact/Starting/Containing, case-sensitive toggle, wrap-around (currentSearchIndex++), Enter pada textSearch = Search().
- Create: CreateObject → combo.Items.Add + Select. Delete: DeleteObject → jika null, remove + select next. Clone: CloneObject → add + select.
- Guna dalam CM16: PlayerForm/TeamForm/LeagueForm/KitForm/BallForm/ShoesForm/GlovesForm/StadiumForm/FormationForm/ManagerForm/RefereeForm/CompetitionForm/NewspapersForm (contoh: `pickUpControl.SelectObject = SelectFormation; pickUpControl.CreateObject = CreateFormation; ...`).

Rancangan cadangan (medium effort):
1. Buat `src\CM26.WPF\Controls\PickUpControl.xaml(.cs)` — UserControl: horizontal strip (ComboBox + ikon buttons + TextBox cari + Filter label/combos). Style ikut ClassicTheme (StripButton style sedia ada).
2. Properties: ObjectList (IReadOnlyList<RecordListItem> atau string[]), SelectObject/CreateObject/DeleteObject/CloneObject/RefreshObject (Action/Func), flags visibility. Terjemah logik Search/Delete/Clone dari CM16.
3. Integrasi pertama di PlayersView (ganti Border "Find:" + ListView atas editor), selepas itu TeamView/LeagueView/RefereeView/KitView/StadiumView/FormationView/Ball/Shoes/Gloves/Manager/Tournament/Newspaper. Simpan senarai penuh (ListView) jika perlu? — CM16 TIDAK ada listview; combo sahaja. Untuk exact, ganti terus; CountText boleh kekal di status atau dibuang.
4. Build + smoke + tests; commit & push (lihat §1). Version kekal 1.0.109.

Catatan penting:
- Smoke.cs (`src\CM26.WPF\Smoke.cs`) navigate semua RadioButton GroupName=="Sec" — jangan pecahkan. Ia panggil `session.TryOpenGame` terus selepas window.Show().
- ViewModel/Session API: `_vm.Session.Sections.GetItems(table)`, `.GetPlayers()`, `.GetTeams()`, `.GetFields(table, recordIndex, LabelMaps.X)` → IReadOnlyList<FieldValue> (FieldName/RawValue/Value), `.Pending.Stage(table, idx, field, value)` → EditOutcome, `.Database.GetTable/GetCell`. LabelMaps di `src\CM26.Application\Services\LabelMaps.cs`.
- FieldRow control sedia ada: `Controls\FieldRow.xaml` — DataTemplate "FieldTemplate" setiap view.
- Table keys: players, teams, teamkits, leagues, referee, manager, nations, formations, stadiums, competitions/tournaments, balls (teamballs/dlcballs), shoes (playerboots), goalkeepergloves, sponsors (adsponserid).
- CM16 decompile `D:\CM16 FULL DECOMPILE\CreationMaster\*.cs` + `FifaControls\*.cs` — rujukan utama (no resx; panelMain plain LightSkyBlue = betul).
- Steam DRM: steam_appid.txt=3405690, `steam://run/3405690//-dataPath <folder>`; settings `%LOCALAPPDATA%\Creation Master 26\settings.ini`; log cm26.log. (Untuk feature, bukan UI.)

## 5. Susulan berpotensi (selepas PickUpControl)
- Field-level parity setiap form CM16 (bandingkan setiap control CM16 vs FieldRow kita — banyak group CM16 guna combo/checkbox khusus, contoh Traits = 33 checkbox bitmask trait1/trait2, Hair combos, Head ethnic combos).
- About box wording CM16; Menu Tools actions (Expand Database, Align Language DB, Free Agents, dll — stub/placeholder sahaja sekarang).
- 3D viewers / 2D viewers (placeholder sekarang).

## 6. Jika ragu
Tanya user dulu (Bahasa Melayu). Jangan ubah versi, jangan buat release package tanpa arahan, jangan amend/force-push.