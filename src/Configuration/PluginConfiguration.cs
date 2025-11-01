using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string TmdbApiKey { get; set; } = string.Empty;
        public string AniListApiKey { get; set; } = string.Empty;
        public bool UseRomajiTitle { get; set; } = true;
    }
}