namespace Kodinn.Sdk;

/// <summary>
/// Entry point of a plugin. Kodinn creates one instance (public parameterless
/// constructor) when the user enables the plugin, calls
/// <see cref="ActivateAsync"/>, and <see cref="DeactivateAsync"/> when the
/// plugin is disabled, removed or updated - while the app keeps running.
/// Everything registered through the host is removed automatically on
/// deactivation; the plugin only releases what it created itself.
/// </summary>
public interface IKodinnPlugin
{
    Task ActivateAsync(IKodinnHost host, CancellationToken cancellationToken);

    Task DeactivateAsync();
}
