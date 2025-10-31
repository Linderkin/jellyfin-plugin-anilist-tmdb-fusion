using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string TmdbApiKey { get; set; } = "";
    }
}
