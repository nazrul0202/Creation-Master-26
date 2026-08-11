# Codex for Open Source — Application Kit

Ready-to-use materials for the **OpenAI Codex for Open Source** program
(6 months of ChatGPT Pro + Codex, conditional Codex Security, and API credits).

- **Application form:** https://openai.com/form/codex-for-oss/
- **Program info:** https://developers.openai.com/codex/open-source
- **Status:** reviewed on a rolling basis; applicants notified by email.
- **No rigid star threshold** — metrics (stars, downloads, ecosystem importance) are
  evaluation factors, not gates. Projects with a strong ecosystem story should apply anyway.

---

## 1. Repo fact sheet (fill-in ready)

| Field | Value |
|-------|-------|
| Repository URL | `https://github.com/nazrul0202/Creation-Master-26` |
| GitHub username | `nazrul0202` |
| Role | Primary maintainer |
| License | MIT |
| Language | C# (WinForms / .NET 8) + C++/CLI engine bridge |
| Releases | Versioned packages (Full Portable + Lite + SHA256SUMS) in every release |
| Monthly downloads | Fill from the GitHub Releases "Download" counts (see Releases page) |
| Stars | Fill current count (this doc is updated on each release) |
| Documentation | README, INSTALLATION.md, KNOWN_LIMITATIONS.md, RELEASE_NOTES.md, docs/BUILDING.md, 30+ research reports in docs/reports/ |

---

## 2. Application text (English, ~500 chars for the "why" field)

> Creation Master 26 is an MIT-licensed Windows editor for EA SPORTS FC 26 career mode
> that reads and writes the Frostbite game database and legacy asset archives directly.
> Built from scratch in C#/.NET 8 with a C++/CLI parser bridge, it ships validated,
> transactional saves with automatic rollback and backup — no EA content is redistributed.
> It is the first open-source FC26 database editor and is actively maintained with
> frequent release packages, full documentation, and 30+ published reverse-engineering
> reports that any modding project can reuse.

(~480 chars — fits the 500-char limit. Trim if needed.)

**Optional "how you plan to use the benefits" text (for the API credits field):**

> API credits will be used for Codex-driven release automation and PR review on the
> CM26 repos, plus Codex Security scanning of the native C++/CLI parser before each
> release packaging run.

---

## 3. AI analysis prompt (the one from the viral tweet, filled in for this repo)

Paste the following into ChatGPT/Codex together with your repo link:

> 我正在申请 OpenAI 的 Codex for Open Source 项目。
>
> 请基于我提供的 GitHub 仓库，对这个项目进行分析，并帮我填写申请表。
>
> 仓库地址：https://github.com/nazrul0202/Creation-Master-26
>
> 我的 GitHub 用户名：nazrul0202
>
> 我的角色：主要维护者 (primary maintainer)
>
> 请输出：1) 一段英文的项目简介（不超过 500 字符，用于申请表）；2) 项目对
> 开源生态的重要性论述；3) 我申请 API credits 的合理用途；4) 如果星星数
> 不多，如何用"生态重要性"角度弥补。

English version of the same prompt:

> I am applying to OpenAI's Codex for Open Source program.
> Based on my GitHub repository below, analyze the project and help me fill out
> the application form.
>
> Repo: https://github.com/nazrul0202/Creation-Master-26
> GitHub username: nazrul0202
> Role: primary maintainer
>
> Output: 1) an English project description under 500 characters for the form;
> 2) an argument for why this project matters to the open-source ecosystem;
> 3) a credible plan for using API credits; 4) how to argue ecosystem importance
> if the star count is still low.

---

## 4. Checklist before submitting

- [ ] GitHub profile visibility is **public**
- [ ] Repository is **public** and the default branch is `main`
- [ ] Repository has a README, license (MIT), release packages and release notes
- [ ] Stars/forks counters: note current numbers from the repo page
- [ ] Monthly downloads: read the download counter on the latest Release asset
- [ ] OpenAI account email matches the one used for the form
- [ ] (API credits only) OpenAI Organisation ID from platform.openai.com → Settings → Organisation → General
- [ ] Fill the form at https://openai.com/form/codex-for-oss/ and submit

---

## 5. Promotion plan (how to gain the 100–200 stars the tweet references)

The tweet's own recipe: build a quality project, then promote it where open-source
users gather. Suggested targets for Creation Master 26:

1. **Release a GitHub Release** with the v1.0.96 packages (already assembled) and notes.
2. **Post the project** on:
   - r/FIFAmodding, r/EAFC, r/FIFA (Reddit communities around career mode modding)
   - discord servers for FC/FIFA modding (FIFA Editor Tool, Frosty Mod Manager users)
   - linuxdo / v2ex (per the tweet's suggestion — international dev forums)
   - Hacker News "Show HN", lobste.rs, r/csharp
3. **Content**: short GIF/video of the Teams profile + Player card; a README badge row
   already shows stars/forks/version so the repo looks alive.
4. **Issue templates** (`bug report` / `feature request`) and a CONTRIBUTING.md make the
   repo look maintained — a strong signal for the program review.

**Golden rule from the program terms:** the strongest signal is *evidence of active
maintenance* (frequent releases, PR review, issue triage). CM26 ships a new release
with every change — keep doing that after applying too.
