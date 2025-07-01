using Jellyfin.Plugin.TubeArchivistSponsorblock.TubeArchivist;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TubeArchivistSponsorblock.Providers;

/// <inheritdoc />
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IMediaSegmentProvider, SponsorblockMediaSegmentProvider>();
        serviceCollection.AddSingleton<TubeArchivistApi>();
    }
}
