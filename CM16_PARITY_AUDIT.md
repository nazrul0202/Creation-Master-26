# CM16 parity audit — FC26 adapters

Updated 2026-07-28.  Compared against the original CM16 form sources in
`D:\FC26 FILE TOOL\cm16 source code\CreationMaster` and the FC26 schema
(`database\fifa_ng_db-meta.XML`).  “Same” means the FC26 equivalent exists,
is shown, and is connected to the FC26 engine; it does **not** mean an absent
FIFA 16 field is invented in FC26.

| CM26 section | CM16 reference | Status | Verified gap / next work |
|---|---|---|---|
| Countries | CountryForm | FC26 fields complete | FC26 has only 8 nation fields; all are now shown. Map/flag stays asset-dependent. |
| Leagues | LeagueForm | FC26 fields complete | All FC26 league fields are exposed, including an editable country picker. Asset panels need an archive writer. |
| Teams | TeamForm | Partial | Roster info and interactive pitch added. FC26's canonical team name is mirrored into the familiar CM16 display slots; link pickers and team traits need completing. |
| Players | PlayerForm | Partial | Database field editor exists; player-name cipher prevents name editing. Face/3D preview depends on local assets. |
| Managers | ManagerForm | Partial | FC26 fields shown; image/3D is asset-dependent. |
| Stadiums | StadiumForm | Partial | Core fields shown; texture/model panes are asset-dependent. |
| Kits | KitForm | Partial | Colour/template fields shown; texture/model panes are asset-dependent. |
| Competitions | CompetitionForm | Partial | Core database editor exists; presentation assets incomplete. |
| Formations | FormationForm | Partial | FC26 formation fields shown; CM16 tactical canvas requires verified FC26 formation-position mapping. |
| Transfers | SquadForm / transfer UI | Partial | Validated link-table transfers exist; full CM16 player contract/loan workspace remains incomplete. |
| Balls | BallForm | Partial | Database properties and local preview supported. |
| Boots | ShoesForm | Partial | Database properties and local preview supported. |
| Referees | RefereeForm | Partial | Core fields shown; 3D preview is asset-dependent. |
| Sponsors / Adboards | No CM16 FC26 equivalent | FC26 extension | FC26-only sponsor and mode-adboard links are shown as fixed CM16-style forms. |
| Audio / Stadium Audio | No CM16 FC26 equivalent | FC26 extension | Nation and stadium audio controls are shown as fixed forms. |
| TV Broadcast / Game Graphics / News Stories | No CM16 FC26 equivalent | FC26 extension | FC26-only database tables are placed in dedicated fixed-layout modules. |
| Database Browser | — | FC26-specific | Browses all FC26 tables; not a CM16 equivalent. |
| Dashboard / Diagnostics / Settings | MainForm | FC26-specific | Application-level functions; no CM16 database-form counterpart. |

## Rules used by the audit

- Preserve the CM16 form geometry and its familiar display slots. Where FC26
  stores a single canonical value instead of CM16's variants, the adapter
  mirrors that canonical FC26 value into the compatible CM16 display slots;
  it never creates a second independent value.
- Never make a control appear editable when the FC26 schema has no field or
  the native writer cannot save it.
- Every relationship editor must use the actual stored ID and stage through
  `PendingChangesService`, not only display a resolved label.
- Asset actions remain unavailable until a verified FC26 archive writer is
  available; a preview is not evidence of an import/export capability.
