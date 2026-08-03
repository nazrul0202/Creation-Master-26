# Player Name Source Audit â€” CM26

Date: 2026-07-28
Scope: trace every player-name value from the FC26 database to the compiled UI, and determine
whether a genuine, readable local name source exists.

This audit was performed read-only. No database, locale, or protected engine file was modified.

---

## 1. The exact current source of every player-name value

| UI value | Source field(s) | Resolved via | Result today |
|----------|-----------------|--------------|--------------|
| Player display name | `players.firstnameid`, `players.lastnameid`, `players.commonnameid` | `playernames.nameid â†’ name` (ciphered in DB) **+ local readable export** | **Real name** when export present, else `Player {id}` |
| Player Info â–¸ First Name / Surname / Common Name | same IDs | `PlayerNameService.NameById(nameId)` | Real name, or `Unavailable` / `Not set` |
| Player Info â–¸ header name | same IDs | `PlayerNameParts.KnownAs` | Real name or `Player {id}` |
| Team roster â–¸ Display Name | same IDs + `teamplayerlinks` | `PlayerNameParts` | Real name or `Player {id}` |
| Captain / corner / penalty / free-kick takers | `teams.captainid`, `â€¦takerid` | `players.playerid â†’ name IDs â†’ PlayerNameService` | Real name or `Player {id}` |
| Search by name | all of the above | `RecordListItem.SearchText` | Works for real names and IDs |

The numeric values under **Surname / First Name** in the earlier build were **not** a database fault
and **not** a localization-cipher fault. They were a presentation bug (see Â§4).

---

## 2. The pipeline (traced end to end)

```
players.firstnameid / lastnameid / commonnameid      (integers â†’ playernames.nameid)
        â†“
playernames table  (nameid | commentaryid | name)
        â†“  name column in fifa_ng_db.db is EA-CIPHERED (0xC4 placeholder fill)
LOCAL READABLE EXPORT  (playernames.txt / playernames.xlsx)  â† the genuine source
        â†“
ExternalNameSource  (read-only adapter)   â† NEW
        â†“
PlayerNameService  (indexed cache, built once per session)
        â†“
NameResolverService
        â†“
SectionDataService â†’ Player / Team presentation models
        â†“
Record selector Â· Player Info Â· Team roster Â· Formation Â· Captain/set-piece UI
```

---

## 3. Why numeric values appeared as surnames (root cause)

`TeamsSection.ShowRecord` built the roster by **splitting the display string on a space**:

```csharp
var nameParts = player.Name.Split(' ', 2, ...);   // "Player 10264" â†’ ["Player","10264"]
â€¦ nameParts[1] â€¦   // â†’ "10264"  placed in the SURNAME column
â€¦ nameParts[0] â€¦   // â†’ "Player" placed in the FIRST NAME column
```

Because the documented fallback string is `Player {playerId}`, splitting it produced exactly the
reported symptom: the **numeric player id under "Surname"** and the word **"Player" under
"First Name"**. This was purely a UI presentation defect in the roster list, not an engine or
localization problem. It has been corrected (see `PLAYER_NAME_BINDING_FIX_REPORT.md`).

A second, related defect: the Player Info tab bound `firstnameid`/`lastnameid`/`commonnameid`
directly under the labels "First Name"/"Last Name", so a raw localization **ID** could be shown in a
name control. Those controls now resolve the ID to a name (or `Unavailable`), and the raw IDs are
shown only in the Technical tab.

---

## 4. Was a real local name source found? â€” YES

Two genuine, readable, **local** sources were found and **verified against the live database**.
No internet fetch, no hard-coded names, no cipher guessing.

| Source | Path | Format | Entries | Verified |
|--------|------|--------|---------|----------|
| **FC26 Modern Database Studio export (preferred)** | `<FC26 tools>\asset\fifa_ng_db\playernames.txt` | tab-separated `nameidâ‰commentaryidâ‰name`, UTF-8 | 41,190 | âœ… resolves **100%** of players |
| Extracted sample spreadsheet | `<repo>\sample db extracted\fifa_ng_Db\playernames.xlsx` | xlsx, `nameid | commentaryid | name` | 41,190 | âœ… resolves **100%** of players |

### Verification (executed, not asserted)
- The live `fifa_ng_db.db` `playernames.name` column was confirmed to be ciphered placeholder:
  `nameid=1 â†’ bytes [C4 C4 C4 6B 50 C4 C4]`, undecodable (matches `LOCALE_DECODER_REPORT.md`).
- Cross-checking **every** player in the live DB against the export:
  `players.firstnameid/lastnameid/commonnameid â†’ playernames.txt` resolves **20,268 / 20,268 (100.0%)**.
- Spot checks return correct real names: `Joe Cole`, `AndrÃ©s Iniesta` (`Iniesta LujÃ¡n`),
  `Alan Shearer`, `Roy Keane`, `Paul Scholes`, `David Beckham`, `Oliver Kahn`, `Javier Zanetti`,
  `Cristiano Ronaldo`, `Martin Ã˜degaard`, `E. MbappÃ©` (UTF-8 `C3 A9` = Ã©). Encoding is clean UTF-8.
- `editedplayernames` is empty (header only) in this DB, so no in-game rename overrides exist.

**Conclusion â€” source category: (1) Present but not loaded.** A genuine readable name source existed
on disk the whole time; the application simply never read it. It is now loaded read-only.

---

## 5. Other local sources audited (and why they are not the name source)

| Candidate | Finding |
|-----------|---------|
| `eng_us.DB` `LanguageStrings1/2` | Real English UI text, but **not** player names (player names live in `playernames`, which is ciphered). |
| `career_firstnames.txt` / `career_lastnames.txt` | Name pools for generated/regen players â€” **not** keyed by `playernames.nameid`; cannot resolve a specific real player. Not used. |
| `commentarynames` / `playernamemap.txt` | Commentary-name IDs â†’ playerid map; commentary audio names, not display surnames. Not the display-name source. |
| `editedplayernames` | Empty (no user renames). |
| `dcplayernames` | Empty in this DB. |
| FC26 install folders on `D:` | Many mod/asset packs (faces, kits, etc.); none is a cleaner `nameidâ†’name` map than the studio export. |

No external extractor, runtime memory dump, or EA key is required â€” the source is a static local file.

---

## 6. Resolution statistics (live DB, this machine)

| Metric | Value |
|--------|-------|
| Players in DB | 20,268 |
| `playernames` rows (ciphered in DB) | 41,190 |
| Readable names loaded from local export | 41,189 |
| Players resolving to a real name | **20,268 (100%)** with source present |
| Fallback `Player {id}` count | 0 with source Â· 20,268 with source absent |
| Player-list build time | ~1.1 s (indexed cache; no per-player scans) |

---

## 7. Remaining blocker

None for the machines that have the export. The only situation in which names cannot be shown is
when **no** readable `playernames` export is reachable. In that case the app honestly shows
`Player {id}` (and `Unavailable` in the individual name fields) and never fabricates a name. The
user can point Settings â–¸ â€œPlayer-name source folderâ€ at any folder containing `playernames.txt`
or `playernames.xlsx` (the app also bundles the verified sample export for offline use).
