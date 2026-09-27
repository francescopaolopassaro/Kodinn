using System.Text.Json;

namespace Kodinn.Sdk;

// ── Toolbar ────────────────────────────────────────────────────────────────

/// <summary>
/// A control in the bar under the chat input, next to the compression and privacy
/// selectors. <see cref="ComponentType"/> is a Blazor component of the plugin; it
/// receives a string parameter "ChatId" (the chat it is shown in) plus
/// <see cref="Parameters"/>.
/// </summary>
public sealed class PluginToolbarItem
{
    public required string Id { get; init; }
    public required Type ComponentType { get; init; }
    public IReadOnlyDictionary<string, object?>? Parameters { get; init; }
}

// ── Hooks ──────────────────────────────────────────────────────────────────

/// <summary>Points of Kodinn's lifecycle a plugin can hook into.</summary>
public enum HookPoint
{
    SkillBeforeExecute, SkillAfterExecute,
    AgentBeforeTurn, AgentAfterTurn,
    ToolBeforeCall, ToolAfterCall,
    ChatBeforeSubmit, ChatAfterSubmit,
    SessionStart, SessionEnd,
    CompactBefore, CompactAfter,
    MemoryBeforeSave, MemoryAfterSave,
    FileSaved, FileCreated,
}

/// <summary>
/// One hook invocation. <see cref="Data"/> is the live data of the event: changing
/// it changes what Kodinn does next (e.g. the text about to be submitted).
/// </summary>
public sealed class HookEvent
{
    public required HookPoint Point { get; init; }
    public string? AgentName { get; init; }
    public string? ToolName { get; init; }
    public string? SkillName { get; init; }
    public string? SessionId { get; init; }
    public string? WorkspacePath { get; init; }
    public required IDictionary<string, object> Data { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

// ── MCP ────────────────────────────────────────────────────────────────────

public interface IKodinnMcp
{
    IReadOnlyList<McpServerInfo> Servers();
    IReadOnlyList<McpToolInfo> Tools(string? server = null);
    Task ConnectAsync(string server, CancellationToken cancellationToken = default);
    void Disconnect(string server);
    /// <summary>Adds or replaces a server in Kodinn's MCP configuration and connects it.</summary>
    Task AddOrUpdateAsync(McpServerSpec server);
    void Remove(string server);
    /// <summary>Calls a tool of a connected server; <paramref name="arguments"/> is a JSON object.</summary>
    Task<string> CallToolAsync(string server, string tool, JsonElement arguments, CancellationToken cancellationToken = default);
}

public sealed record McpServerInfo(string Name, bool Connected, bool Enabled, int ToolCount);
public sealed record McpToolInfo(string Server, string Name, string Description);

/// <summary>A server to add: <see cref="Command"/> for stdio servers, <see cref="Url"/> for HTTP ones.</summary>
public sealed class McpServerSpec
{
    public required string Name { get; init; }
    public string? Command { get; init; }
    public IReadOnlyList<string>? Args { get; init; }
    public string? Url { get; init; }
    public IReadOnlyDictionary<string, string>? Environment { get; init; }
}

// ── Chat ───────────────────────────────────────────────────────────────────

public interface IKodinnChat
{
    /// <summary>The chats open in Kodinn.</summary>
    IReadOnlyList<ChatInfo> OpenChats { get; }
    /// <summary>The chat on screen, if any.</summary>
    string? ActiveChatId { get; }
    IReadOnlyList<ChatMessageInfo> Messages(string chatId);
    /// <summary>Puts text in the chat's input box (the user still decides to send it).</summary>
    Task SetInputAsync(string chatId, string text);
    /// <summary>Sends <paramref name="text"/> as if the user had typed it and pressed Enter.</summary>
    Task SendAsync(string chatId, string text);
    /// <summary>Adds a message of the plugin to the chat (markdown), without calling any model.</summary>
    Task AppendAsync(string chatId, string markdown);
}

public sealed record ChatInfo(string Id, string Title, bool IsProcessing);
public sealed record ChatMessageInfo(string Role, string Content, DateTimeOffset At);
