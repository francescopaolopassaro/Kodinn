namespace Kodinn.Sdk;

/// <summary>
/// The plugin's only door into Kodinn. Each call is checked against the
/// capabilities the user granted when enabling the plugin
/// (<see cref="PluginCapability"/>): a call outside them throws
/// <see cref="PluginCapabilityException"/>.
/// Registrations return an <see cref="IDisposable"/> that removes them; all
/// of them are removed anyway when the plugin is deactivated.
/// </summary>
public interface IKodinnHost
{
    /// <summary>The plugin as declared in its plugin.json.</summary>
    PluginInfo Plugin { get; }

    /// <summary>Version of the SDK Kodinn implements.</summary>
    Version SdkVersion { get; }

    /// <summary>Language of the Kodinn interface ("it", "en"...), for the plugin's own texts.</summary>
    string Language { get; }

    /// <summary>Capabilities the user granted to this plugin.</summary>
    IReadOnlySet<PluginCapability> Granted { get; }

    /// <summary>Messages written to Kodinn's diagnostic log, prefixed with the plugin id.</summary>
    IPluginLogger Log { get; }

    /// <summary>Private key/value storage of this plugin, encrypted like the rest of Kodinn's data
    /// (<see cref="PluginCapability.Storage"/>).</summary>
    IPluginStorage Storage { get; }

    /// <summary>The plugin's own database, as declared in plugin.json
    /// (<see cref="PluginCapability.Database"/>).</summary>
    IPluginDatabase Database { get; }

    /// <summary>A tool the agent can call while working (<see cref="PluginCapability.Tools"/>).
    /// The user is asked before each call unless the tool says otherwise.</summary>
    IDisposable RegisterTool(PluginTool tool);

    /// <summary>A chat command, typed as /name (<see cref="PluginCapability.Commands"/>).</summary>
    IDisposable RegisterCommand(PluginCommand command);

    /// <summary>A panel in the chat's dock, rendered by a Blazor component of the plugin
    /// (<see cref="PluginCapability.Panels"/>).</summary>
    IDisposable RegisterPanel(PluginPanel panel);

    /// <summary>A control in the bar under the chat input (<see cref="PluginCapability.Toolbar"/>).</summary>
    IDisposable RegisterToolbarItem(PluginToolbarItem item);

    /// <summary>Called for every message sent and every answer completed in any chat
    /// (<see cref="PluginCapability.ChatEvents"/>). Read-only.</summary>
    IDisposable OnChatEvent(Func<ChatEvent, Task> handler);

    /// <summary>A handler for one of Kodinn's lifecycle hooks (<see cref="PluginCapability.Hooks"/>).
    /// Higher priority runs first.</summary>
    IDisposable OnHook(HookPoint point, Func<HookEvent, Task> handler, int priority = 0);

    /// <summary>The chats: read, write in the input, send, append (<see cref="PluginCapability.ChatControl"/>).</summary>
    IKodinnChat Chat { get; }

    /// <summary>Kodinn's MCP servers and their tools (<see cref="PluginCapability.Mcp"/>).</summary>
    IKodinnMcp Mcp { get; }

    /// <summary>Every internal service of Kodinn (<see cref="PluginCapability.FullAccess"/>). The
    /// types are Kodinn's own (Kodinn.Framework.dll, next to the app); nothing here is a stable
    /// contract - it changes with Kodinn's versions.</summary>
    IServiceProvider Services { get; }

    /// <summary>A notification in Kodinn's interface (<see cref="PluginCapability.Notifications"/>).</summary>
    void Notify(string title, string message);

    /// <summary>A one-off request to the model the user configured in Kodinn - cloud provider or
    /// local model - with its usual fallbacks and cost accounting (<see cref="PluginCapability.Model"/>).</summary>
    Task<string> AskModelAsync(string prompt, CancellationToken cancellationToken = default);
}

/// <summary>Plugin identity, from plugin.json.</summary>
public sealed record PluginInfo(string Id, string Name, Version Version, string Author, string Description);

public interface IPluginLogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}

public interface IPluginStorage
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
    Task RemoveAsync(string key);
    Task<IReadOnlyList<string>> KeysAsync();
}

/// <summary>A call outside the capabilities granted to the plugin.</summary>
public sealed class PluginCapabilityException(PluginCapability capability)
    : InvalidOperationException($"The plugin was not granted the '{capability}' capability.")
{
    public PluginCapability Capability { get; } = capability;
}
