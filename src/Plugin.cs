using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Common.Plugins;
using Jellyfin.Plugin.AnilistTMDbFusion.Configuration;

namespace Jellyfin.Plugin.AnilistTMDbFusion
{
    public class Plugin : BasePlugin, IHasWebPages, IHasPluginConfiguration
    {
        public static Plugin? Instance { get; private set; }

        private PluginConfiguration _configuration = new PluginConfiguration();

        public Plugin()
        {
            Instance = this;
            SetId(new Guid("8d7a3a6d-1b23-4c77-9b8a-a12d3f4e9e7d"));
            SetAttributes("AniFusion", "Usa el título romaji de AniList y los metadatos de TMDb en español.", new Version(1, 0, 0, 0));
        }

        public override string Name => "AniFusion";

        public BasePluginConfiguration Configuration => _configuration;

        public Type ConfigurationType => typeof(PluginConfiguration);

        public void UpdateConfiguration(BasePluginConfiguration config)
        {
            _configuration = (PluginConfiguration)config;
        }
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "AniFusion",
                    EmbeddedResourcePath = "Jellyfin.Plugin.AnilistTMDbFusion.Configuration.PluginConfigurationPage.html"
                }
            };
        }
    }
}