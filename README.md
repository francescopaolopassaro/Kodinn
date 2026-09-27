# Kodinn

### The AI workspace that works for you — and keeps your data yours.

**This is the public repository of Kodinn, for all users: everything that is open source in
Kodinn is published here.** You find the installers, the SDK for writing plugins, a complete
example plugin and, over time, the other parts of Kodinn we open up.

> 🔒 **Open source, with a closed core.** Kodinn is built on a high level of security. The core
> engine — agent orchestration, roles, permissions and the whole security system — is **not open**
> and is distributed only as a compiled DLL inside the installers. That is a deliberate security
> choice, explained [below](#open-source-with-a-closed-core).

Kodinn is a **desktop AI workspace for Windows and macOS**. An agent that reads and writes code, runs
commands, browses the web, works with Git and checks the quality of what it produces — together
with your mail, notes, documents, databases and APIs, in one application. It works with more than
twenty AI providers **or entirely offline** with a model running on your own computer, and
everything it stores is **encrypted on your machine under your PIN**.

> **Questo è il repository pubblico di Kodinn, per tutti gli utenti: tutto ciò che di Kodinn è
> open source viene pubblicato qui.** Kodinn è lo spazio di lavoro AI per Windows e macOS: un agente
> che scrive codice, usa il terminale, naviga, lavora con Git e verifica la qualità di ciò che
> produce, insieme a posta, note, documenti, database e API — con oltre venti provider AI o
> completamente offline, e con tutti i dati cifrati sul tuo computer sotto il tuo PIN. 🔒 Il nucleo
> di Kodinn (orchestrazione degli agenti, ruoli, permessi e sistema di sicurezza) **non è aperto**
> per ragioni di sicurezza: viene distribuito solo come DLL compilata dentro i setup.

---

## Why Kodinn

| | |
|---|---|
| 🔐 **Your data is encrypted, always** | Database, conversations, credentials and mail are encrypted at rest with a key derived from your PIN. Other agents keep configuration and history in plain files. |
| 🕵️ **Personal data never reaches the model** | *PrivacyGuard* masks names, emails, IBANs, tax codes, keys and dozens of other categories **before** a message leaves your computer, and restores them in the answer. The provider sees placeholders, not your data. |
| 🧠 **Many models, one team** | More than twenty providers, several instances of the same one, and a different model for each agent role — with automatic failover when a provider fails or hits its limits. No lock-in to a single vendor. |
| 💻 **Offline when you want** | Local models run inside Kodinn on your CPU or GPU through **DesireeIA**, our own inference engine. No network, no account, no per-token cost. |
| ✅ **It checks its own work** | A built-in static analysis engine (*QualityGuard*: 27 languages, thousands of rules, Quality Gate) the agent uses on the code it writes. |
| 🗜️ **Context that lasts** | *Caveman* compresses the conversation instead of cutting it, routes each kind of content to the right strategy, aligns with the provider's cache — and refuses to summarise what must stay exact (security advisories, destructive commands). |
| 🖥️ **A real application** | Not a terminal: sessions, chat, Git, diff, browser and panels side by side, in Italian, English and Chinese. |
| 🧩 **Extensible without restarting** | Plugins install, update and uninstall while Kodinn runs, each with its own encrypted database and explicit consent for everything it asks. |

---

## What Kodinn can do

### 🤖 The agent
- **Always has tools** — there is no "just chat" mode: every conversation is with an agent that can act.
- Over **50 tools**: files, search, edit with diff, shell and terminal, build and test, Git, browser,
  web search, memory, tasks, scheduling, worktrees, static analysis, databases, knowledge archive.
- **Asks before risky actions** — every tool has a permission (allow / ask) you control.
- **Undo and redo** of every change, a Git-style panel with the diff of each file.
- **Plan mode**, **sub-agents and teams** that split the work, **skills** (45 bundled) and slash commands.
- **Memory** across sessions, per project.

### 🧠 Providers and orchestration
- **More than 20 providers** preconfigured (cloud and local), multiple instances of each.
- **Per-role routing**: planner, developer, reviewer, tester can each run on a different model.
- **Automatic failover** with a shared health state and a different reaction for each kind of error
  (authentication, limits, timeouts, refusals), plus a watchdog and per-minute rate limiting.
- **Cost and token panels**: what each model costs you, live, from a price list updated automatically.
- 🔒 *The orchestration engine is part of the closed core, for security reasons.*

### 💻 Local AI — DesireeIA
- GGUF models run **inside Kodinn**, on CPU or GPU, with automatic tuning for your model and your PC.
- Fully **offline**: nothing leaves your machine.
- Voice dictation is local too: your voice is transcribed on your computer.

### 🔐 Security and privacy
- **Encryption at rest** of everything Kodinn stores, with a key derived from your PIN; the PIN is
  asked once and never stored in the clear.
- A **sealed credential vault** for API keys, passwords and tokens.
- **PrivacyGuard**: configurable masking of personal data from the chat itself, restored in the answers.
- **Every piece of data in its own encrypted file** — mailboxes, document archive, plugin databases —
  so nothing grows without limit in one place.
- Plugins and agents can only do what you allowed.
- 🔒 *The security engine is not open: publishing it would hand out the map of what an attack has
  to get past.*

### 🛠️ The developer workspace
- **Git and GitHub** built in: status, diff, commit, branches, merge, pull requests.
- **Integrated terminal** and shell with process management.
- **Built-in browser** the agent can drive (the very session you see), with devtools, console and
  network capture, plus an **embedded web server** per chat to preview what it builds.
- **Code editor** with syntax highlighting and language servers.
- **MCP**, both **client and server**, with auto-detection and a dedicated interface.

### 📬 Beyond code
- **EmailFlow** — full mail client (IMAP/SMTP, OAuth), folders, rules, identities, scheduled send
  and undo-send, conversations; the agent can read and draft mail for you.
- **MemoFlow** — tasks, notes with attachments, calendar; mail and files become notes in one move.
- **Document archive** — your own on-board knowledge base (RAG) with a knowledge graph, encrypted,
  that the agent searches when it answers.
- **Database module** — connect to several database engines at once, read-only enforced for the
  agent when you want it.
- **Sinapsi** — a client for REST APIs and protocols.
- **79 connectors** to external services, with credentials prefilled from the vault.

### 🧩 Plugins
Hot-installable extensions with their own tools, commands, panels, controls under the chat, hooks,
MCP servers and **their own encrypted database** — see [Plugins](#plugins) below.

---

## Kodinn compared

Kodinn, Claude Code and OpenCode solve the same problem — an agent that writes code — from different
starting points: Claude Code and OpenCode are born in the terminal, Kodinn is born as a desktop
application with encrypted data and several models orchestrated together. LangChain is in the table
because it is the most cited yardstick, but it is a different category: a library you build with,
where everything Kodinn already has is code somebody must write, maintain and secure.

| | **Kodinn** | Claude Code | OpenCode | LangChain |
|---|---|---|---|---|
| **Interface** | ✅ Full desktop application | Terminal | Terminal | None: it is a library |
| **Data encrypted at rest** | ✅ Database, conversations, credentials, mail — key derived from your PIN | Configuration and history in the clear | Files in the clear | Not provided |
| **Personal-data masking before the model** | ✅ Built in (PrivacyGuard), values restored in the answer | ❌ | ❌ | Build it yourself |
| **Providers** | ✅ 20+ preconfigured, several instances of each | Anthropic models only | Several providers | Many, through integrations |
| **A different model for each agent role** | ✅ With specialisations and priority | ❌ One vendor for everything | Model selection, no per-role routing | Build it yourself |
| **Automatic failover between providers** | ✅ Shared health state, a reaction per error type | ❌ | Left to the user | Build it yourself |
| **Offline local models** | ✅ In-process on CPU and GPU (DesireeIA) | ❌ Needs the remote API | Through an external server | Through external integrations |
| **Built-in static analysis** | ✅ QualityGuard: 27 languages, thousands of rules, Quality Gate | Reviews by the model, no engine | External tooling | ❌ |
| **MCP** | ✅ Client **and** server, dedicated UI, auto-detection | Client | Client | Client, via adapters |
| **Built-in browser driven by the agent** | ✅ Inside the app, the very session you see | Separate browser extension | ❌ | Only tools you program |
| **Voice dictation** | ✅ Offline: your voice never leaves the machine | Remote transcription | ❌ | ❌ |
| **Mail, notes, calendar, knowledge archive (RAG), databases, API client** | ✅ All built in | ❌ | ❌ | Build it yourself |
| **Plugins** | ✅ Hot install, explicit consent, each with its own encrypted database | Plugins and extensions | Community extensions | Integrations you code |
| **Operational telemetry** | ✅ Panels for tokens, cost, per-minute limits, timeouts, permissions | Text commands | Partial | Tracing to wire up |
| **Context compression** | ✅ Caveman: routing per content type, cache alignment, guard over critical content | Summarising compaction | Compaction | Strategies to assemble |
| **Localised interface** | ✅ Italian, English, Chinese | English | English | Not applicable |

**Where the others fit better.** Claude Code and OpenCode run anywhere there is a terminal —
Linux, containers, CI pipelines, SSH — and LangChain is the right base for a pipeline written from
scratch. Kodinn is a graphical application for Windows and macOS. When your data, your costs and the
quality of the result are what matter, Kodinn is built to be the best place to work with AI.

*The Kodinn column is verified against Kodinn's source. The other columns reflect the features known
at the date of this document (September 2026): those products are under active development and may
change.*

> **Il confronto.** Claude Code e OpenCode nascono nel terminale, LangChain è una libreria con cui
> costruire; Kodinn nasce come applicazione desktop con dati cifrati e più modelli orchestrati
> insieme. Kodinn è l'unico dei quattro con cifratura di tutti i dati sotto il PIN, mascheratura dei
> dati personali prima del modello, un modello diverso per ogni ruolo con failover automatico,
> modelli locali offline dentro l'app, analisi statica integrata, MCP client e server, browser e
> dettatura offline, posta/note/archivio/database integrati e interfaccia in italiano. 🔒 Il motore
> che rende possibile tutto questo è il nucleo chiuso di Kodinn, non pubblicato per ragioni di
> sicurezza.

---

## Open source, with a closed core

We want Kodinn to be open source, and everything that can be opened is published here. One part
stays closed: **the core of Kodinn**, `Kodinn.Framework`. It is the engine that orchestrates the
agents, their roles and permissions, and the whole security system — encryption of your data under
the PIN, the credential vault, the checks on what agents and plugins are allowed to do.

Kodinn is built around a high level of security, and that engine is where it lives. Publishing its
source would hand anyone a map of exactly the logic an attack would have to get past, so we chose
to keep it closed:

- `Kodinn.Framework` ships **only as a compiled DLL inside Kodinn's installers**;
- it is **not published on NuGet** and its source is not in this repository;
- everything a plugin needs is in the open [`Kodinn.Sdk`](src/Kodinn.Sdk/README.md), which **is**
  published on NuGet: plugins never compile against the core.

> **Open source, con un nucleo chiuso.** Vogliamo che Kodinn sia open source e tutto ciò che si
> può aprire lo pubblichiamo qui. Resta chiusa una parte: **il nucleo di Kodinn**,
> `Kodinn.Framework`, cioè il motore che orchestra gli agenti, i loro ruoli e permessi, e tutto il
> sistema di sicurezza (cifratura dei dati col PIN, cassaforte delle credenziali, controlli su cosa
> possono fare agenti e plugin). Kodinn si basa su un sistema di sicurezza elevato e quel motore è il
> punto in cui vive: pubblicarne il codice significherebbe consegnare a chiunque la mappa esatta
> della logica che un attacco dovrebbe superare. Per questo abbiamo scelto di tenerlo chiuso: viene
> distribuito **solo come DLL compilata dentro i setup di Kodinn**, **non è pubblicato su NuGet** e il
> suo sorgente non è in questo repository. Tutto ciò che serve ai plugin sta nell'SDK aperto
> `Kodinn.Sdk`, che invece è pubblicato su NuGet.

---

## Download

The installers are in **[`Builds/`](Builds/README.md)** — only installers, no source code:
the Windows setup in `Builds/windows`, the MSIX package in `Builds/windows/msix`, the macOS package
in `Builds/macos`, with the SHA-256 of each file in `Builds/SHA256SUMS.txt`. They are stored with
Git LFS (a setup is about 500 MB); every version is also attached to a
**[GitHub Release](https://github.com/francescopaolopassaro/Kodinn/releases)**.

## Plugins

Kodinn installs plugins **while running**: pick a `.kodinn-plugin` file in *Settings → Plugins*,
read what the plugin asks to do, and enable it. Disabling, updating and removing a plugin need no
restart either.

A plugin can add:

- **tools for the agent** — called while it works, with your approval unless the tool is read-only;
- **chat commands** — typed as `/name`;
- **panels in the chat dock** — its own interface, as Blazor components;
- **controls under the chat** — next to compression and privacy;
- **reactions to chat events** and **control of the chats** — reading, writing, sending;
- **Kodinn's hooks** and **MCP servers** — stepping into how the agent works, adding and using servers;
- **its own database** — declared in `plugin.json`, created at install time in a separate file,
  encrypted with your PIN, removed with the plugin;
- **notifications**, **private encrypted storage** and **requests to the model you configured**;
- **full access**, for the plugins you fully trust.

Every plugin starts **disabled**. Before enabling it you see its author and the exact capabilities
it declares; anything outside them is refused at run time.

🔒 Plugins talk to Kodinn only through the open SDK: the core they run inside stays closed, and a
plugin never sees how Kodinn stores its data — not even the engine behind its own database.

| Folder | Contents |
|---|---|
| [`src/Kodinn.Sdk`](src/Kodinn.Sdk/README.md) | the SDK: the only types a plugin compiles against (open source, on NuGet) |
| [`PluginExample`](PluginExample/README.md) | *Calculator & Timer*: exact arithmetic for the agent and for you, focus timers, a dock panel |
| [`Builds`](Builds/README.md) | Kodinn's installers (Windows setup, MSIX, macOS) and their checksums |

To build the example you need the [.NET 10 SDK](https://dotnet.microsoft.com/download):

```powershell
cd PluginExample
powershell -ExecutionPolicy Bypass -File pack-plugin.ps1
```

## Support

Questions, bug reports and plugin ideas: [Issues](https://github.com/francescopaolopassaro/Kodinn/issues).

## License

See [LICENSE](LICENSE).
