# Project conventions

## GitHub sync
- **Always push to GitHub after every update.** Whenever the public repo (`GitHub_CM26\Creation-Master-26`, origin https://github.com/nazrul0202/Creation-Master-26.git) has new commits, run `git push origin main` — never leave changes un-pushed.
- Commit author identity for the public repo: `Rizco98 <rizco98@users.noreply.github.com>` (use `-c user.name=... -c user.email=...` since no local git identity is configured).
- Preferred file endings: CRLF on checkout (LF warnings on commit are expected and harmless).

## Build & test
- Release config: `Release\assemble_packages.ps1` assembles Full/Lite portable packages.
- Public release version is a fixed MAJOR.MINOR.PATCH (e.g. v1.0.18); keep `CM26.App.csproj` + Assembly/File/Informational versions in sync.