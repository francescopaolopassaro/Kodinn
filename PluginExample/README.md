# PluginExample — Calculator & Timer

A complete Kodinn plugin that is also useful day to day:

- **`calculate`**, a tool for the agent: language models get arithmetic wrong; this evaluates
  expressions exactly (decimal arithmetic, 28 significant digits: `0.1 + 0.2` is `0.3`).
  Supports `+ - * / % ^`, parentheses, `pi`, `e`, `sqrt`, `abs`, `round(x, digits)`, `floor`,
  `ceil`, `min`, `max`.
- **`/calc (1250 + 80) * 1.22`**, the same in the chat.
- **`/timer 25 review`**, a focus timer with a Kodinn notification when it ends.
- **A dock panel** 🧮 with the history of calculations (yours and the agent's) and the running timers.
- The history is kept in the plugin's own database (`plugin_kodinn.examples.calculator-timer.db`),
  declared in `plugin.json`: Kodinn creates it at install time, encrypted with your PIN, and deletes
  it with the plugin. The panel loads the latest 50 through the `at` index; the database keeps 1000.

It asks only for the capabilities it uses: `Tools`, `Commands`, `Panels`, `Notifications`,
`Database` — it does not read the chats and does not call the model.

## Build and install

```powershell
powershell -ExecutionPolicy Bypass -File pack-plugin.ps1
```

This produces `PluginExample.kodinn-plugin`. In Kodinn open **Settings → Plugins → Install
plugin**, pick the file, review what it asks for and enable it. No restart needed; disabling or
removing it is immediate too.

## Files

| File | What it shows |
|---|---|
| `plugin.json` | the manifest: identity, entry class, capabilities |
| `CalculatorTimerPlugin.cs` | `IKodinnPlugin`: registering a tool, two commands and a panel; storage; notifications; texts in the user's language |
| `CalculatorPanel.razor` | a dock panel as a Blazor component, re-rendering when the plugin's state changes |
| `Calculator.cs` | the expression evaluator (plain C#, no Kodinn types) |
| `pack-plugin.ps1` | packaging, without the assemblies Kodinn already provides |

## Writing your own

1. Copy this project, change `id`, `name`, `assembly` and `entryType` in `plugin.json`.
2. Keep the reference to `Kodinn.Sdk` with `Private=false` / `ExcludeAssets=runtime`: the SDK and
   the Blazor assemblies must be Kodinn's own, never a copy shipped with the plugin.
3. Declare in `capabilities` only what you use: the user sees the list before enabling the plugin,
   and calls outside it throw `PluginCapabilityException`.
