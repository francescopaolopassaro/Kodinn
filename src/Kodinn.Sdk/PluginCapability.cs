namespace Kodinn.Sdk;

/// <summary>
/// What a plugin may do inside Kodinn. A plugin declares the ones it needs in
/// plugin.json; Kodinn shows them to the user, who grants them when enabling
/// the plugin. Nothing outside the granted set is reachable.
/// </summary>
public enum PluginCapability
{
    /// <summary>Tools the agent can call.</summary>
    Tools,
    /// <summary>Chat commands (/name).</summary>
    Commands,
    /// <summary>Panels in the chat's dock.</summary>
    Panels,
    /// <summary>Controls in the bar under the chat input, next to compression and privacy.</summary>
    Toolbar,
    /// <summary>Reading the messages sent and the answers completed in the chats.</summary>
    ChatEvents,
    /// <summary>Acting on the chats: reading their messages, writing in the input, sending messages,
    /// adding messages of its own.</summary>
    ChatControl,
    /// <summary>Kodinn's lifecycle hooks: before/after an agent turn, a tool call, a chat submit,
    /// a compaction, a file or memory save - with the power to change their data.</summary>
    Hooks,
    /// <summary>The MCP servers: listing, adding, connecting, removing them and calling their tools.</summary>
    Mcp,
    /// <summary>Notifications in the interface.</summary>
    Notifications,
    /// <summary>Private encrypted storage.</summary>
    Storage,
    /// <summary>Requests to the model configured in Kodinn (may cost money with a cloud provider).</summary>
    Model,
    /// <summary>Every internal service of Kodinn (IKodinnHost.Services): the plugin can do anything
    /// Kodinn can. Only for plugins the user fully trusts.</summary>
    FullAccess,
    /// <summary>The plugin's own database (<c>plugin_&lt;id&gt;.db</c>), with the collections declared
    /// under "database" in plugin.json, encrypted with the user's PIN like the rest of Kodinn's data.</summary>
    Database,
}
