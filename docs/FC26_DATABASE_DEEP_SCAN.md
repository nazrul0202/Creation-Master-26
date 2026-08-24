# FC26 database deep scan — updated for v1.0.142

Source inspected: the installed FC26 direct-session squads database dated 2026-08-22. Values can change with a later Title Update or squad file.

## Team information

Verified static `teams` fields include `clubworth` (stored in thousands), `popularity`, `youthdevelopment`, `profitability`, `foundationyear`, `leaguetitles`, `domesticcups` and `uefa_cl_wins`.

Manchester City (`teamid=10`) in the inspected database has Club Worth `4564360` (about 4.56B after scaling), popularity `9` (Very High), youth development `3` (Low), profitability `8` (High), founded `1880`, 10 league titles, 7 domestic cups and 1 Champions League. A screenshot from another squad/Title Update can therefore legitimately show a different Club Worth.

`factory_teams` is not a per-team budget source: it contains one World XI template row. The base squads database has no populated per-team Career budget rows. `career_managerpref` is a zeroed template and `career_managerhistory`/`career_managerinfo` are empty before a Career save exists.

The live Career save does populate `career_managerpref.transferbudget` and
`startofseasontransferbudget`. The active manager's club is identified by
`career_users.clubteamid`. Creation Master 26 v1.0.142 edits those two Career
values directly from the Team page while keeping static `teams.clubworth`
separate, and creates a timestamped copy of the complete save before writing.

`leagueteamlinks.objective`, `highestpossible` and `highestprobable` are zero placeholders in the inspected static database. Career mode generates the visible board objectives; interpreting zero as the old CM16 enum value “Win League Title” is incorrect.

## Formations and tactics

The authoritative generic formation set is the 29 rows where `formations.teamid=-1`:

`4-1-3-2`, `4-1-4-1`, `4-2-3-1 Narrow`, `4-2-3-1 Wide`, `4-2-4`, `4-3-1-2`, `4-3-2-1`, `4-3-3 Flat`, `4-3-3 Holding`, `4-3-3 Defend`, `4-3-3 Attack`, `4-2-2-2`, `4-1-2-1-2 Wide`, `4-1-2-1-2 Narrow`, `4-4-2 Flat`, `4-4-2 Holding`, `4-4-1-1 Midfield`, `4-5-1 Flat`, `4-5-1 Attack`, `3-1-4-2`, `3-4-1-2`, `3-4-2-1`, `3-4-3 Flat`, `3-5-2`, `5-2-1-2`, `5-2-3`, `5-3-2 Holding`, `5-4-1 Flat`, `4-2-1-3`.

The short `formationname` column is not unique. FC26 variants must be resolved through `formationid`; a team's row points back to the generic layout through `relativeformationid`.

Verified FC26 team tactic values remain Build-Up Style (`Short Passing`, `Balanced`, `Counter`), Defensive Approach (`Deep`, `Balanced`, `High`, `Aggressive`) and exact line height 1–100. Team traits are stored in the opponent-context masks `trait1vweak`, `trait1vequal` and `trait1vstrong`.

## Player information

Static `players` data includes detailed technical, mental, physical and goalkeeper attributes, preferred positions, five familiar-role IDs, traits and PlayStyle/PlayStyle+ masks. These are appropriate for the squads editor.

Energy, sharpness, morale and the live role/focus currently selected in Team Management are runtime Career/match state and are not columns in the base squads `players` table. CM26 labels that boundary instead of inventing default values.
