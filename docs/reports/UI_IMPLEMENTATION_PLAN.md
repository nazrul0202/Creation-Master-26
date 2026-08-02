# CM26 UI Implementation Plan

Architecture (practical, not over-engineered):

```
CM26.EngineBridge (C++/CLI, net8.0-windows, x64)
   └─ compiles protected src/database_engine.cpp UNCHANGED
   └─ exposes managed façade: EngineSession, TableInfo, ColumnInfo, RowData, EditOutcome

CM26.Application (C# class lib, net8.0-windows)
   └─ IDatabaseSession / DatabaseSession      (open/load state, owns EngineSession)
   └─ CatalogService                           (tables, record lists, search, paging)
   └─ NameResolverService                      (IDs -> resolved names; positions/roles/labels)
   └─ PendingChangesService                    (staging, undo/redo, modified-field tracking)
   └─ ValidationService                        (pre-save validation aggregation)
   └─ SaveService                              (backup + engine saveT3dbCopy + reload verify)
   └─ Section services: PlayerService, TeamService, LeagueService, CountryService,
      ManagerService, StadiumService, KitService, BallService, BootService,
      RefereeService, FormationService, CompetitionService, TransferService

CM26.App (C# WinForms, net8.0-windows, x64) -> CM26_by_Rizco98.exe
   └─ Program.cs (startup, global exception handling, logging, DI-light service locator)
   └─ MainForm (sidebar nav, toolbar, workspace, status bar, keyboard shortcuts)
   └─ Controls/  (SearchBar, RecordListPanel, EditorHeader, ValidationSummary,
                  EmptyState, LoadingState, ErrorState, ImagePreview, PendingChangesPanel)
   └─ Sections/  one UserControl per section, all built on shared controls
   └─ Theming/   (Theme palette, ControlFactory, fonts/spacing constants)
   └─ Assets/Icons (embedded section PNGs + drawn fallback)
```

Design system (consistent everywhere):
- Dark palette: background `#1E222A`, panel `#262B34`, raised `#2E3440`, border `#3A4150`,
  text `#E6E9EF`, muted `#9AA3B2`, accent `#3B82F6`, accent-hover `#2F6FD6`, danger `#E5534B`,
  success `#3FB950`, warning `#D29922`.
- Typography: Segoe UI 9f body, 9f semi-bold labels, 12f section title, 16f header record title.
- Spacing unit 8px (4/8/12/16/24). Standard control height 26px; toolbar 40px; sidebar 216px.
- High-DPI: `Application.SetHighDpiMode(PerMonitorV2)`, AutoScaleMode.Dpi, no fixed overlap.

Data loading workflow:
1. `Ctrl+O`/Open → pick folder (or the 3 files via folder auto-detect).
2. Engine `loadFolder` validates → progress shown → `readT3db` main + locale on a background thread.
3. Cancellation via CancellationToken; UI never blocks (async/await + Invoke marshalling).
4. Clear errors for missing/unsupported files.

Save safety:
1. `Ctrl+S`/Save → ValidationService aggregates staged edits → summary dialog (target files confirmed).
2. SaveService: timestamped backup next to source (where supported) → engine `saveT3dbCopy` →
   reload-verify → precise success/failure. Never silent. Never bypasses the engine.

Keyboard: Ctrl+O open, Ctrl+S save, Ctrl+Z undo, Ctrl+F focus search, F5 refresh, Esc close/clear.

Section order (one at a time): Countries, Leagues, Teams, Players, Managers, Stadiums, Kits,
Competitions, Formations, Transfers, Balls, Boots, Referees, Database Browser, Diagnostics,
Dashboard, Settings.

Each section: browser (search/sort/filter/count/prev-next/refresh/clear) + editor (header with
resolved relationships, General/Details/Relationships/Technical tabs, validation, modified marks)
+ action bar (Apply, Revert, Undo, Save, Validate, Open related, Refresh). Unsupported edits are
shown disabled with a tooltip explanation (no fake behaviour).
