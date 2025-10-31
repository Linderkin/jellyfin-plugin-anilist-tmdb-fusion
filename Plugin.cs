using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AnilistTMDbFusion
{
    public class Plugin : BasePlugin<Configuration.PluginConfiguration>, IHasWebPages
    {
        public static Plugin? Instance { get; private set; }

        public override string Name => "AniList + TMDb Fusion";
        public override string Description => "Usa el título romaji de AniList y los metadatos de TMDb en español.";

        public Plugin(IApplicationPaths appPaths, IXmlSerializer xmlSerializer)
            : base(appPaths, xmlSerializer)
        {
            Instance = this;
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "AnilistTMDbFusionConfig",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.PluginConfigurationPage.html"
                }
            };
        }
    }
}
