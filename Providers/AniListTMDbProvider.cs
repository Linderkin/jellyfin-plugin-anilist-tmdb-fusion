using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Jellyfin.Plugin.AnilistTMDbFusion.Configuration;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Providers
{
    public class AniListTMDbProvider : IRemoteMetadataProvider<Series, ItemLookupInfo>, IRemoteSearchProvider<ItemLookupInfo>
    {
        private readonly PluginConfiguration _config;

        public AniListTMDbProvider(PluginConfiguration config)
        {
            _config = config;
        }

        // Devuelve los metadatos de la serie
        public async Task<MetadataResult<Series>> GetMetadata(ItemLookupInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>
            {
                Item = new Series
                {
                    Name = "Título Romaji (placeholder)"
                },
                HasMetadata = true
            };

            // Aquí puedes integrar AniList y TMDb usando _config.TmdbApiKey, etc.

            return await Task.FromResult(result);
        }

        // Búsqueda remota de contenido
        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ItemLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<RemoteSearchResult>>(new List<RemoteSearchResult>());
        }
        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(null);
        }
        // Nombre del proveedor
        public string Name => "AniList + TMDb Fusion Provider";
    }
}