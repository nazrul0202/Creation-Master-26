# FC26 database deep scan — updated for v1.0.145

Source inspected: the installed FC26 direct-session squads database dated 2026-08-22. Values can change with a later Title Update or squad file.

## Complete Team-section inventory

The inspected snapshot contains 808 teams and 110 columns in `teams`. The
Team section is spread across these linked tables rather than one record:

| Table | Rows | Team data supplied |
| --- | ---: | --- |
| `teams` | 808 | identity, colours, prestige, finances, ratings, honours, tactics, traits, atmosphere/pitch flags and set-piece player IDs |
| `leagueteamlinks` | 808 | league membership, last-season/current positions, champion flag, form/statistics and objective bounds |
| `teamplayerlinks` | 21,622 | player, club, shirt number, tactical position and competition statistics |
| `formations` | 837 | team-specific and 29 generic tactical layouts |
| `default_mentalities` | 4,045 | five default mentality variants per supported team, tactics and XI slots |
| `defaultteamdata` | 808 | default layout/depth and related team defaults |
| `teamkits` | 3,781 | team kit links and kit appearance data |
| `teamstadiumlinks` | 814 | home stadium assignment |
| `teamnationlinks` | 145 | national-team country assignment |
| `manager` | 808 | manager identity, team assignment, appearance and traits |
| `stadiums` | 180 | stadium model, capacity/environment and presentation values |
| `teamballs` | 149 | ball assets referenced by teams/competitions |
| `career_managerpref` | 1 | Career-save budget/wage preferences; not a per-team squads table |

### Static club information available in `teams`

Verified editable profile fields include `clubworth` (stored in thousands),
`domesticprestige`, `internationalprestige`, `profitability`, `popularity`,
`youthdevelopment`, `foundationyear`, `teamstadiumcapacity`, `overallrating`,
`attackrating`, `midfieldrating`, `defenserating`, `leaguetitles`,
`domesticcups`, `uefa_cl_wins`, `uefa_el_wins`, `uefa_uecl_wins` and
`uefa_consecutive_wins`. v1.0.145 exposes these high-value fields through the
structured CM16-style Club Details tab; the snapshot change plan writes them
back to their native columns.

Additional available categories are team colours, rival, ball, city/location,
kit/stadium presentation, crowd/tifo/banner flags, pitch/net styles, set-piece
takers, opponent thresholds, matchday ratings, build-up style, defensive depth
and the three opponent-context trait masks. Asset/presentation flags remain in
the snapshot until a safe editor and visual validation exist; they are not
silently reinterpreted as old CM16 fields.

Manchester City (`teamid=10`) in the inspected database has Club Worth `4564360` (about 4.56B after scaling), popularity `9` (Very High), youth development `3` (Low), profitability `8` (High), founded `1880`, 10 league titles, 7 domestic cups and 1 Champions League. A screenshot from another squad/Title Update can therefore legitimately show a different Club Worth.

`factory_teams` is not a per-team budget source: it contains one World XI template row. The base squads database has no populated per-team Career budget rows. `career_managerpref` is a zeroed template and `career_managerhistory`/`career_managerinfo` are empty before a Career save exists.

The live Career save does populate `career_managerpref.transferbudget` and
`startofseasontransferbudget`. The active manager's club is identified by
`career_users.clubteamid`. Creation Master 26 v1.0.144 edits those two Career
values directly from the Team page while keeping static `teams.clubworth`
separate, and creates a timestamped copy of the complete save before writing.

`leagueteamlinks.objective` is zero for all 808 teams in this snapshot, so the
actual Career board objective is not present in the base squads DB. However,
`highestpossible` is non-zero for 131 teams and `highestprobable` is non-zero
for 299 teams. v1.0.144 therefore treats each field independently: unavailable
zero values are labelled and disabled, while populated Highest/Probable values
remain visible and editable. Interpreting every zero as the old CM16 enum value
“Win League Title” is incorrect.

## Roster-link integrity

There are 21,622 `teamplayerlinks` covering 801 of the 808 team IDs; seven
database teams intentionally have no linked players. The scanned player table
contains 20,268 unique player IDs, and the tested Al Fateh links all resolve to
real player rows.

The v1.0.143 roster regression was in the bridge, not the database: after
creating a linked `TeamPlayer`, its numeric `teamid` and `playerid` were passed
through the generic reflection mapper and overwrote the linked Team/Player
objects with null. v1.0.144 keeps foreign-key conversion in the save-plan path
only. Real-database smoke coverage now verifies that every loaded roster link
has a live player object and specifically renders Al Fateh's roster list.

## Formations and tactics

The authoritative generic formation set is the 29 rows where `formations.teamid=-1`:

`4-1-3-2`, `4-1-4-1`, `4-2-3-1 Narrow`, `4-2-3-1 Wide`, `4-2-4`, `4-3-1-2`, `4-3-2-1`, `4-3-3 Flat`, `4-3-3 Holding`, `4-3-3 Defend`, `4-3-3 Attack`, `4-2-2-2`, `4-1-2-1-2 Wide`, `4-1-2-1-2 Narrow`, `4-4-2 Flat`, `4-4-2 Holding`, `4-4-1-1 Midfield`, `4-5-1 Flat`, `4-5-1 Attack`, `3-1-4-2`, `3-4-1-2`, `3-4-2-1`, `3-4-3 Flat`, `3-5-2`, `5-2-1-2`, `5-2-3`, `5-3-2 Holding`, `5-4-1 Flat`, `4-2-1-3`.

The short `formationname` column is not unique. FC26 variants must be resolved through `formationid`; a team's row points back to the generic layout through `relativeformationid`.

Verified FC26 team tactic values remain Build-Up Style (`Short Passing`, `Balanced`, `Counter`), Defensive Approach (`Deep`, `Balanced`, `High`, `Aggressive`) and exact line height 1–100. Team traits are stored in the opponent-context masks `trait1vweak`, `trait1vequal` and `trait1vstrong`.

## Player information

Static `players` data includes detailed technical, mental, physical and goalkeeper attributes, preferred positions, five familiar-role IDs, traits and PlayStyle/PlayStyle+ masks. These are appropriate for the squads editor.

Energy, sharpness, morale and the live role/focus currently selected in Team Management are runtime Career/match state and are not columns in the base squads `players` table. CM26 labels that boundary instead of inventing default values.
