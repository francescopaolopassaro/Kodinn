namespace Kodinn.Sdk;

/// <summary>
/// A tool the agent can call. <see cref="Name"/> must be unique in Kodinn
/// (lowercase, digits and underscores); the description is what the model
/// reads to decide when to use it - write it for the model.
/// </summary>
public sealed class PluginTool
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<PluginToolParameter> Parameters { get; init; } = [];

    /// <summary>Ask the user before each call (default). Set to false only for tools that
    /// cannot change anything (read-only lookups).</summary>
    public bool RequiresApproval { get; init; } = true;

    /// <summary>Runs the tool. The arguments are the ones the model passed, by parameter name;
    /// the returned text is what the model reads back.</summary>
    public required Func<IReadOnlyDictionary<string, object?>, CancellationToken, Task<string>> ExecuteAsync { get; init; }
}

/// <summary>A parameter of a <see cref="PluginTool"/>.</summary>
/// <param name="Name">Name the model uses for the argument.</param>
/// <param name="Type">"string", "integer", "number" or "boolean".</param>
/// <param name="Description">What the model should pass.</param>
/// <param name="Required">Whether the model must always pass it.</param>
public sealed record PluginToolParameter(string Name, string Type, string Description, bool Required = false);

/// <summary>
/// A chat command: typing /<see cref="Name"/> followed by optional arguments
/// runs it, and the returned markdown is shown as the answer in the chat.
/// </summary>
public sealed class PluginCommand
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Func<string, CancellationToken, Task<string>> ExecuteAsync { get; init; }
}

/// <summary>
/// A panel in the chat's dock: a button in the dock rail opens
/// <see cref="ComponentType"/>, a Blazor component of the plugin
/// (it must implement IComponent - any .razor component does). Parameters
/// are passed to the component as [Parameter] properties.
/// </summary>
public sealed class PluginPanel
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    /// <summary>One or two characters (an emoji) shown in the dock rail.</summary>
    public string Icon { get; init; } = "🧩";
    public required Type ComponentType { get; init; }
    public IReadOnlyDictionary<string, object?>? Parameters { get; init; }
}

public enum ChatEventKind
{
    /// <summary>The user sent a message.</summary>
    MessageSent,
    /// <summary>An answer finished.</summary>
    AnswerCompleted,
}

/// <summary>Something that happened in a chat.</summary>
public sealed record ChatEvent(ChatEventKind Kind, string ChatId, string Text, DateTimeOffset At);
