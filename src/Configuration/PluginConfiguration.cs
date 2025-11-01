using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AniFusion.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string TmdbApiKey { get; set; } = "";
        public string Language { get; set; } = "es-ES";
        public bool ShowImages { get; set; } = true;
        public bool IncludeGenres { get; set; } = true;
    }
}
