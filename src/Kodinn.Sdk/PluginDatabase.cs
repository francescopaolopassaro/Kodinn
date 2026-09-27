using System.Text.RegularExpressions;

namespace Kodinn.Sdk;

/// <summary>
/// The "database" section of plugin.json: the collections of the plugin's own database and,
/// for each one, the fields that can be searched (<see cref="IPluginCollection{T}.FindAsync"/>,
/// <see cref="IPluginCollection{T}.RangeAsync"/>).
/// Kodinn creates the database when the plugin is installed, in a file of its own
/// (never inside Kodinn's database), and deletes it when the plugin is removed.
/// </summary>
public sealed class PluginDatabaseSpec
{
    public const int MaxCollections = 32;
    public const int MaxIndexesPerCollection = 16;

    public List<PluginCollectionSpec> Collections { get; set; } = [];

    public PluginCollectionSpec? Find(string name) =>
        Collections.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal));

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (Collections.Count == 0) errors.Add("database: declare at least one collection");
        if (Collections.Count > MaxCollections) errors.Add($"database: at most {MaxCollections} collections");
        foreach (var c in Collections)
        {
            if (!PluginCollectionSpec.IsValidName(c.Name))
                errors.Add($"database: \"{c.Name}\" is not a valid collection name (a-z, 0-9, _; starts with a letter; max 64)");
            if (c.Indexes.Count > MaxIndexesPerCollection)
                errors.Add($"database.{c.Name}: at most {MaxIndexesPerCollection} indexes");
            foreach (var i in c.Indexes.Where(i => !PluginCollectionSpec.IsValidField(i)))
                errors.Add($"database.{c.Name}: \"{i}\" is not a valid field name");
            if (c.Indexes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != c.Indexes.Count)
                errors.Add($"database.{c.Name}: an index is declared twice");
        }
        if (Collections.Select(c => c.Name).Distinct(StringComparer.Ordinal).Count() != Collections.Count)
            errors.Add("database: a collection is declared twice");
        return errors;
    }
}

/// <summary>One collection of the plugin's database.</summary>
public sealed class PluginCollectionSpec
{
    private static readonly Regex NameRule = new("^[a-z][a-z0-9_]{0,63}$", RegexOptions.CultureInvariant);
    private static readonly Regex FieldRule = new("^[A-Za-z_][A-Za-z0-9_]{0,63}$", RegexOptions.CultureInvariant);

    public string Name { get; set; } = "";
    /// <summary>The entity properties that can be searched, by their JSON name (camelCase:
    /// a C# property <c>CreatedAt</c> is <c>createdAt</c>). Top-level properties only.</summary>
    public List<string> Indexes { get; set; } = [];

    public static bool IsValidName(string? name) => name != null && NameRule.IsMatch(name);
    public static bool IsValidField(string? name) => name != null && FieldRule.IsMatch(name);
}

/// <summary>
/// Base class for what a plugin keeps in its database. Add your own properties; they are stored
/// as JSON (camelCase), so any type System.Text.Json can serialize works.
/// <code>
/// public sealed class Note : PluginEntity
/// {
///     public string Text { get; set; } = "";
///     public string Tag { get; set; } = "";
///     public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
/// }
/// </code>
/// </summary>
public abstract class PluginEntity
{
    /// <summary>Unique in its collection. Left empty, Kodinn assigns one on insert.</summary>
    public string Id { get; set; } = "";
}

/// <summary>
/// The plugin's own database (<see cref="PluginCapability.Database"/>). Only the collections declared
/// in plugin.json exist; the storage behind them belongs to Kodinn and is not reachable.
/// </summary>
public interface IPluginDatabase
{
    /// <summary>The collections declared in plugin.json.</summary>
    IReadOnlyList<string> Collections { get; }

    /// <summary>A declared collection, holding entities of type <typeparamref name="T"/>.</summary>
    /// <exception cref="ArgumentException">The collection is not declared in plugin.json.</exception>
    IPluginCollection<T> Collection<T>(string name) where T : PluginEntity;
}

/// <summary>A collection of the plugin's database.</summary>
public interface IPluginCollection<T> where T : PluginEntity
{
    string Name { get; }

    Task<T?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity; assigns <see cref="PluginEntity.Id"/> when empty and returns it.</summary>
    /// <exception cref="InvalidOperationException">An entity with that id already exists.</exception>
    Task<string> InsertAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing entity; false when there is none with that id.</summary>
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Inserts or replaces; returns the id.</summary>
    Task<string> UpsertAsync(T entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Deletes every entity of the collection; returns how many.</summary>
    Task<int> DeleteAllAsync(CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>The entities ordered by id, a page at a time.</summary>
    Task<IReadOnlyList<T>> AllAsync(int skip = 0, int limit = 1000, CancellationToken cancellationToken = default);

    /// <summary>The entities whose indexed <paramref name="field"/> equals <paramref name="value"/>.</summary>
    /// <exception cref="ArgumentException">The field is not among the collection's indexes.</exception>
    Task<IReadOnlyList<T>> FindAsync(string field, object? value, int limit = 1000, CancellationToken cancellationToken = default);

    /// <summary>The entities whose indexed <paramref name="field"/> is between <paramref name="from"/> and
    /// <paramref name="to"/> (both included; null = unbounded), ordered by that field.</summary>
    /// <exception cref="ArgumentException">The field is not among the collection's indexes.</exception>
    Task<IReadOnlyList<T>> RangeAsync(string field, object? from, object? to, bool descending = false,
        int limit = 1000, CancellationToken cancellationToken = default);
}
