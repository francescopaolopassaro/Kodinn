using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kodinn.Sdk;

/// <summary>
/// plugin.json, at the root of a plugin package:
/// <code>
/// {
///   "id": "com.example.hello",
///   "name": "Hello",
///   "version": "1.0.0",
///   "author": "Example",
///   "description": "What the plugin does, shown to the user before enabling it.",
///   "assembly": "PluginExample.dll",
///   "entryType": "PluginExample.HelloPlugin",
///   "sdkVersion": "1.0.0",
///   "capabilities": [ "Tools", "Commands", "Panels", "Database" ],
///   "database": {
///     "collections": [
///       { "name": "notes", "indexes": [ "tag", "createdAt" ] }
///     ]
///   }
/// }
/// </code>
/// </summary>
public sealed class PluginManifest
{
    public const string FileName = "plugin.json";

    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>The plugin's own assembly, relative to the package root.</summary>
    public string Assembly { get; set; } = "";
    /// <summary>Full name of the class implementing <see cref="IKodinnPlugin"/>.</summary>
    public string EntryType { get; set; } = "";
    /// <summary>The SDK version the plugin was built against.</summary>
    public string SdkVersion { get; set; } = "1.0.0";
    public List<PluginCapability> Capabilities { get; set; } = [];
    /// <summary>The plugin's own database, created when the plugin is installed. Optional; requires
    /// the <see cref="PluginCapability.Database"/> capability.</summary>
    public PluginDatabaseSpec? Database { get; set; }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static PluginManifest Parse(string json) =>
        JsonSerializer.Deserialize<PluginManifest>(json, JsonOptions)
        ?? throw new InvalidDataException("plugin.json is empty.");

    /// <summary>The problems that make the manifest unusable; empty when it is valid.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Id) || Id.Any(c => !(char.IsLetterOrDigit(c) || c is '.' or '-' or '_')))
            errors.Add("id: letters, digits, '.', '-' and '_' only");
        if (string.IsNullOrWhiteSpace(Name)) errors.Add("name is missing");
        if (!System.Version.TryParse(Version, out _)) errors.Add("version is not a version number");
        if (!System.Version.TryParse(SdkVersion, out _)) errors.Add("sdkVersion is not a version number");
        if (string.IsNullOrWhiteSpace(Assembly) || Assembly.Contains("..") || Path.IsPathRooted(Assembly))
            errors.Add("assembly must be a file inside the package");
        if (string.IsNullOrWhiteSpace(EntryType)) errors.Add("entryType is missing");
        bool wantsDb = Capabilities.Contains(PluginCapability.Database);
        if (Database != null && !wantsDb) errors.Add("database is declared but the Database capability is not");
        if (Database == null && wantsDb) errors.Add("the Database capability needs a \"database\" section");
        if (Database != null) errors.AddRange(Database.Validate());
        return errors;
    }

    public PluginInfo ToInfo() =>
        new(Id, Name, System.Version.TryParse(Version, out var v) ? v : new System.Version(0, 0), Author, Description);
}
