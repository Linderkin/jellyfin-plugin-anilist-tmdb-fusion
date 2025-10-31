using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AnilistTMDbFusion.Configuration;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Providers
{
    public class AniListTMDbProvider : IRemoteMetadataProvider<Series, SeriesInfo>
    {
        public string Name => "AniList + TMDb Fusion";

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series> { HasMetadata = false };
            var plugin = Plugin.Instance!;
            var apiKey = plugin.Configuration?.TmdbApiKey;

            if (string.IsNullOrEmpty(apiKey))
                return result;

            // Obtener metadatos de TMDb
            var tmdb = await TMDbClient.SearchAsync(apiKey, info.Name);
            if (tmdb == null) return result;

            var series = new Series
            {
                Overview = tmdb.Value.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
            };

            if (tmdb.Value.TryGetProperty("first_air_date", out var dateProp) && DateTime.TryParse(dateProp.GetString(), out var dt))
            {
                series.PremiereDate = dt;
            }

            // Obtener título romaji de AniList
            var romaji = await AniListClient.GetRomajiTitleAsync(info.Name);
            if (!string.IsNullOrEmpty(romaji))
            {
                series.Name = romaji;
            }
            else if (tmdb.Value.TryGetProperty("name", out var nameProp))
            {
                series.Name = nameProp.GetString();
            }

            result.Item = series;
            result.HasMetadata = true;
            return result;
        }
    }
}
