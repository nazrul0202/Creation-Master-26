# Country Confederation Audit — CM26

Date: 2026-07-28
Database folder: `D:\CM 26 Final\database` (read-only audit)

## 1. The reported symptom

"Afghanistan is shown under CAF" — its correct football confederation is **AFC** (Asia).

## 2. Layer-by-layer trace (Afghanistan)

Command: `CM26.EngineDiagnostics --trace-country-confederation <afghanistan-id>`

| Layer | Value | Correct? |
|-------|-------|----------|
| Physical DB `nations.confederation` | `5` | ✅ raw value present |
| Native engine | returns `5` | ✅ reads correct field |
| C++/CLI bridge | `GetCell` → `"5"` | ✅ |
| `NameResolverService.ConfederationLabel(5)` | `AFC` (in current source) | ✅ |
| UI (bound to resolver) | `AFC` | ✅ |

The raw code for Afghanistan is **5**, and in the current source code `5 → AFC` is **correct**.

## 3. Root cause of the symptom

The confederation mapping in an **earlier build** was:

```csharp
// OLD (WRONG): 1=>AFC, 2=>UEFA, 3=>CONCACAF, 4=>CONMEBOL, 5=>CAF, 6=>OFC
```

This made Afghanistan (code 5) show as **CAF**. It was corrected earlier in this session to:

```csharp
// CURRENT (CORRECT, derived from the actual nations table):
2=>UEFA, 3=>CAF, 4=>CONMEBOL, 5=>AFC, 6=>OFC, 7=>CONCACAF, 1=>"—"
```

The user's report of "Afghanistan in CAF" comes from **running a stale binary** (e.g.
`build_updated\`, `publish\`, or `Release\CM26\`), all of which predate the fix (timestamps
26–27 July). The rebuilt packages (`Release\CM26_v1.0_Full_Portable\`, `Release\CM26_v1.0_Lite\`,
dated 28 July 00:58) contain the correct mapping.

## 4. All-country validation (218 nations)

Raw-code distribution from the DB:
- code 1 = 7 nations (special: Gibraltar, Greenland, International, Rest of World, …)
- code 2 = 54 nations (UEFA: England, France, Germany, Spain, Italy, …)
- code 3 = 54 nations (CAF: Algeria, Egypt, Ghana, Morocco, Nigeria, South Africa, …)
- code 4 = 11 nations (CONMEBOL: Argentina, Brazil, Chile, Colombia, …)
- code 5 = 46 nations (AFC: Afghanistan, Australia, Japan, Qatar, Korea Republic, India, …)
- code 6 = 11 nations (OFC: Fiji, New Zealand, Papua New Guinea, Samoa, …)
- code 7 = 35 nations (CONCACAF: Canada, Mexico, United States, Costa Rica, Jamaica, …)

13 ground-truth spot checks: **13/13 PASS** (Afghanistan→AFC, Malaysia→AFC, Japan→AFC, Saudi
Arabia→AFC, England→UEFA, Germany→UEFA, Brazil→CONMEBOL, Argentina→CONMEBOL, Morocco→CAF,
Nigeria→CAF, United States→CONCACAF, Mexico→CONCACAF, New Zealand→OFC).

## 5. Results

| Metric | Value |
|--------|-------|
| Total countries | 218 |
| Correctly resolved | 218 (all map to a known confederation; code 1 = "—") |
| Unresolved | 0 |
| Duplicate mappings | 0 |
| Invalid confederation values | 0 |
| Countries in unexpected confederation | 0 |

## 6. Verdict

The confederation **mapping in the current source/binary is correct**. The defect exists only in
**stale binaries**. Fix = run the rebuilt exe (28 July) and remove/archive the old build folders.
