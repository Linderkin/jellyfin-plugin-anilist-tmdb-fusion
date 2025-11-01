using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Jellyfin.Plugin.AniFusion.Configuration;

namespace Jellyfin.Plugin.AniFusion.Providers
{
    public class AniFusionProvider : IRemoteMetadataProvider<Series, ItemLookupInfo>, IRemoteSearchProvider<ItemLookupInfo>
    {
        private readonly PluginConfiguration _config;

        private static readonly SemaphoreSlim AniListSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim TMDbSemaphore = new SemaphoreSlim(1, 1);

        public AniFusionProvider()
        {
            _config = new PluginConfiguration();
        }

        public string Name => "AniFusion Provider";

        public async Task<MetadataResult<Series>> GetMetadata(ItemLookupInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>
            {
                Item = new Series(),
                HasMetadata = false
            };

            // Obtener título romaji de AniList
            var romajiTitle = await GetRomajiTitleFromAniList(info.Name ?? "", cancellationToken);
            if (!string.IsNullOrEmpty(romajiTitle))
            {
                result.Item.Name = romajiTitle;
            }

            // Obtener metadatos de TMDb
            var tmdbData = await GetTMDbDetails(info.Name ?? "", cancellationToken);
            if (tmdbData != null)
            {
                result.Item.Overview = tmdbData.overview;

                if (_config.IncludeGenres && tmdbData.genres != null)
                {
                    result.Item.Genres = tmdbData.genres
                        .Select(g => g.name)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToArray();
                }

                if (DateTime.TryParse(tmdbData.first_air_date, out var date))
                {
                    result.Item.PremiereDate = date;
                    result.Item.ProductionYear = date.Year;
                }

                if (!string.IsNullOrEmpty(tmdbData.poster_path) && _config.ShowImages)
                {
                    result.RemoteImages = new List<(string Url, ImageType Type)>
                    {
                        ($"https://image.tmdb.org/t/p/w500{tmdbData.poster_path}", ImageType.Primary)
                    };
                }

                result.HasMetadata = true;
            }

            return result;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ItemLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            // Por ahora devolvemos vacío, Jellyfin lo usa solo si hacemos búsqueda
            return Task.FromResult<IEnumerable<RemoteSearchResult>>(new List<RemoteSearchResult>());
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            // No usamos
            return Task.FromResult<HttpResponseMessage>(default!);
        }

        private async Task<string?> GetRomajiTitleFromAniList(string searchTitle, CancellationToken cancellationToken)
        {
            await AniListSemaphore.WaitAsync(cancellationToken);
            try
            {
                var query = @"
                    query ($search: String) {
                        Media(search: $search, type: ANIME) {
                            title { romaji }
                        }
                    }";

                var variables = new { search = searchTitle };
                var payload = new { query, variables };

                using var client = new HttpClient();
                int retries = 3;

                while (retries > 0)
                {
                    var response = await client.PostAsJsonAsync("https://graphql.anilist.co", payload, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadFromJsonAsync<AniListResponse>(cancellationToken);
                        return json?.data?.Media?.title?.romaji;
                    }

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000, cancellationToken);

                    retries--;
                }

                return null;
            }
            finally
            {
                AniListSemaphore.Release();
            }
        }

        private async Task<TMDbDetails?> GetTMDbDetails(string searchTitle, CancellationToken cancellationToken)
        {
            await TMDbSemaphore.WaitAsync(cancellationToken);
            try
            {
                var apiKey = _config.TmdbApiKey;
                var language = _config.Language ?? "es-ES";

                using var client = new HttpClient();
                int retries = 3;

                while (retries > 0)
                {
                    var searchUrl = $"https://api.themoviedb.org/3/search/tv?api_key={apiKey}&query={Uri.EscapeDataString(searchTitle)}&language={language}";
                    var searchResponse = await client.GetAsync(searchUrl, cancellationToken);

                    if (searchResponse.IsSuccessStatusCode)
                    {
                        var searchJson = await searchResponse.Content.ReadFromJsonAsync<TMDbSearchResponse>(cancellationToken);
                        var firstResult = searchJson?.results?.FirstOrDefault();
                        if (firstResult == null) return null;

                        var detailsUrl = $"https://api.themoviedb.org/3/tv/{firstResult.id}?api_key={apiKey}&language={language}";
                        var detailsResponse = await client.GetAsync(detailsUrl, cancellationToken);

                        if (detailsResponse.IsSuccessStatusCode)
                            return await detailsResponse.Content.ReadFromJsonAsync<TMDbDetails>(cancellationToken);
                    }

                    if ((int)searchResponse.StatusCode == 429)
                        await Task.Delay(1000, cancellationToken);

                    retries--;
                }

                return null;
            }
            finally
            {
                TMDbSemaphore.Release();
            }
        }

        // Clases internas para deserialización
        private class AniListResponse
        {
            public AniListData? data { get; set; }
        }

        private class AniListData
        {
            public AniListMedia? Media { get; set; }
        }

        private class AniListMedia
        {
            public AniListTitle? title { get; set; }
        }

        private class AniListTitle
        {
            public string? romaji { get; set; }
        }

        private class TMDbSearchResponse
        {
            public List<TMDbSearchResult>? results { get; set; }
        }

        private class TMDbSearchResult
        {
            public int id { get; set; }
        }

        private class TMDbDetails
        {
            public string? overview { get; set; }
            public string? first_air_date { get; set; }
            public string? poster_path { get; set; }
            public List<TMDbGenre>? genres { get; set; }
        }

        private class TMDbGenre
        {
            public string? name { get; set; }
        }
    }
}
