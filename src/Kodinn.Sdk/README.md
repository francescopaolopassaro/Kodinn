# Kodinn.Sdk

The contract between [Kodinn](https://github.com/francescopaolopassaro/Kodinn) and its plugins.

A plugin is a .NET class library that implements `IKodinnPlugin` and ships a
`plugin.json`. Kodinn installs it while running, shows the user what the
plugin declares it will do, and activates it only after the user agrees.

Through `IKodinnHost` a plugin can:

| Capability | What it gives |
|---|---|
| `Tools` | tools the agent calls while working (`RegisterTool`) |
| `Commands` | chat commands typed as `/name` (`RegisterCommand`) |
| `Panels` | panels in the chat dock, rendered by the plugin's Blazor components (`RegisterPanel`) |
| `Toolbar` | controls in the bar under the chat input, next to compression and privacy (`RegisterToolbarItem`) |
| `ChatEvents` | messages sent and answers completed in the chats (`OnChatEvent`) |
| `ChatControl` | the open chats: read their messages, write in the input, send, add messages (`Chat`) |
| `Hooks` | Kodinn's lifecycle hooks (agent turns, tool calls, chat submit, compaction, file and memory saves), with the power to change their data (`OnHook`) |
| `Mcp` | the MCP servers: list, add, connect, remove, call their tools (`Mcp`) |
| `Notifications` | notifications in the interface (`Notify`) |
| `Storage` | a private key/value store, encrypted like the rest of Kodinn's data (`Storage`) |
| `Database` | the plugin's own database with the collections declared in plugin.json (`Database`) |
| `Model` | requests to the model the user configured (`AskModelAsync`) |
| `FullAccess` | every internal service of Kodinn (`Services`): only for plugins the user fully trusts |

## The plugin's database

A plugin that needs structured data declares it in `plugin.json`:

```json
"capabilities": [ "Database" ],
"database": {
  "collections": [
    { "name": "notes", "indexes": [ "tag", "createdAt" ] }
  ]
}
```

When the plugin is installed Kodinn creates its database, `plugin_<id>.db`: a file of its own,
never inside Kodinn's database, encrypted with the user's PIN, deleted when the plugin is removed.
The plugin works with entities, never with the storage engine:

```csharp
public sealed class Note : PluginEntity
{
    public string Text { get; set; } = "";
    public string Tag { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;   // UTC: indexes sort as text
}

var notes = host.Database.Collection<Note>("notes");
string id = await notes.InsertAsync(new Note { Text = "Call Anna", Tag = "work" });
var work  = await notes.FindAsync("tag", "work");                          // indexed fields only
var last  = await notes.RangeAsync("createdAt", null, null, descending: true, limit: 20);
```

Only the declared collections exist, and only the declared indexes can be searched; entities are
stored as JSON (camelCase), up to 1 MB each.

See `PluginExample` in the repository for a complete plugin.
