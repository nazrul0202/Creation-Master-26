# Installation — Creation Master 26 1.0.135

Use the Full Portable package on Windows 10/11 x64, or install Microsoft .NET 8
Desktop Runtime x64 for the Lite package.

CM26 is distributed unsigned, so Windows SmartScreen may show an "unknown
publisher" warning on first run. Choose **More info > Run anyway** if you trust
the download. Verify your download against `SHA256SUMS_v1.0.135.txt` if
you obtained it from anywhere other than the official releases page.

The built-in CM26 3D viewer remains included. For optional F3D FBX viewing,
install F3D normally, place `f3d.exe` in `Tools\F3D`, or set `CM26_F3D_PATH`
to the full executable path. CM26 also searches the system `PATH`.

On first run, an **End User License Agreement** is shown. You must accept it to
continue. The full terms are in the `LICENSE` and `EULA.md` files inside the
package.

Close FC26, run `CM26_by_Rizco98.exe`, then choose **File > Open FC26**. On the
first open CM26 creates the complete immutable original backup at
`<FC26>\CmModData\Data` and `<FC26>\CmModData\Patch`. Do not modify that folder;
**File > Restore Original FC26 Data** depends on it.

> Keep your own separate backup of the original game installation as well.
> CM26's `CmModData` cannot recover a backup that has been deleted or altered.

Every Save commits the validated database/legacy replacements directly into
that installation's live `Data` and `Patch`, then re-extracts and reloads the
saved database from the archives to verify the result.

## No EA game content is included

Neither package contains game data: no database tables, schema files, audio,
textures, meshes or name lists. CM26 works only on the files already present in
your own installed copy of the game.

## CM26 Scraper (optional)

The CM26 Scraper is **a separate download and is not included** in this package,
because its data set contains game database content this project does not
redistribute.

Data Sync (**Transfers > Data Sync**) works fully once you have your own copy:

1. Download the CM26 Scraper.
2. Either place its folder next to `CM26_by_Rizco98.exe` named `CM26 Scraper`,
   or click **Set folder...** on the Data Sync page (also available as
   **Settings > CM26 Scraper folder**) and select the folder containing
   `CM26 Scraper.exe`. A copy at a drive root or inside a drive-root
   `FC26 FILE TOOL` folder is also detected automatically.
3. Run a squad scrape. Its output (`Scraped teams\squad_*.xlsx`) is detected
   automatically — when the scraper closes, Data Sync refreshes and previews the
   newest output ready for import.

Without the scraper, every other section of CM26 works normally, including the
Transfermarkt URL squad preview and CSV export on the same page.
