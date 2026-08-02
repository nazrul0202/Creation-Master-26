# Installation — Creation Master 26 1.0.18

Use the Full Portable package on Windows 10/11 x64, or install Microsoft .NET 8
Desktop Runtime x64 for the Lite package.

Close FC26, run `CM26_by_Rizco98.exe`, then choose **File > Open FC26**. On the
first open CM26 creates the complete immutable original backup at
`<FC26>\CmModData\Data` and `<FC26>\CmModData\Patch`. Do not modify that folder;
**File > Restore Original FC26 Data** depends on it.

Every Save commits the validated database/legacy replacements directly into
that installation's live `Data` and `Patch`, then re-extracts and reloads the
saved database from the archives to verify the result.

## CM26 Scraper

The CM26 Scraper ships inside the package under `Tools\CM26 Scraper\` and is
opened from the **Transfers > Data Sync** page. Its squad output
(`Scraped teams\squad_*.xlsx`) is detected automatically; when the scraper
closes, Data Sync refreshes and previews the newest output ready for import.
If you already have a copy of the scraper next to CM26, at a drive root, or in
a `FC26 FILE TOOL` folder, it is used instead. A specific folder can be set in
**Settings > CM26 Scraper folder**.
