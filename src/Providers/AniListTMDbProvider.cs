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
using Jellyfin.Plugin.AnilistTMDbFusion.Configuration;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Providers
{
    public class AniListTMDbProvider : IRemoteMetadataProvider<Series, ItemLookupInfo>, IRemoteSearchProvider<ItemLookupInfo>
    {
        private readonly PluginConfiguration _config;

        public AniListTMDbProvider()
        {
            _config = new PluginConfiguration
            {
                //Cambiarlo por tu api_key
                TmdbApiKey = "xxxxxxxxxxxx"
            };
        }

        public string Name => "AniFusion Provider";

        public async Task<MetadataResult<Series>> GetMetadata(ItemLookupInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>
            {
                Item = new Series(),
                HasMetadata = false
            };

            // Título romaji desde AniList
            var romajiTitle = await GetRomajiTitleFromAniList(info.Name ?? "", cancellationToken);
            if (!string.IsNullOrEmpty(romajiTitle))
            {
                result.Item.Name = romajiTitle;
            }

            // Metadatos desde TMDb en español
            var tmdbData = await GetTMDbDetails(info.Name ?? "", cancellationToken);
            if (tmdbData != null)
            {
                result.Item.Overview = tmdbData.overview;
                result.Item.Genres = tmdbData.genres?.Select(g => g.name).Where(n => !string.IsNullOrEmpty(n)).ToArray();

                if (DateTime.TryParse(tmdbData.first_air_date, out var date))
                {
                    result.Item.PremiereDate = date;
                    result.Item.ProductionYear = date.Year;
                }

                result.RemoteImages = new List<(string, ImageType)>
                {
                    ($"https://image.tmdb.org/t/p/w500{tmdbData.poster_path}", ImageType.Primary)
                };


                result.HasMetadata = true;
            }

            return result;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ItemLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<RemoteSearchResult>>(new List<RemoteSearchResult>());
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(default!);
        }

        private async Task<string?> GetRomajiTitleFromAniList(string searchTitle, CancellationToken cancellationToken)
        {
            var query = @"
                query ($search: String) {
                    Media(search: $search, type: ANIME) {
                        title {
                            romaji
                        }
                    }
                }";

            var variables = new { search = searchTitle };
            var payload = new { query, variables };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://graphql.anilist.co", payload, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<AniListResponse>(cancellationToken);
            return json?.data?.Media?.title?.romaji;
        }

        private async Task<TMDbDetails?> GetTMDbDetails(string searchTitle, CancellationToken cancellationToken)
        {
            var apiKey = _config.TmdbApiKey;
            using var client = new HttpClient();

            var searchUrl = $"https://api.themoviedb.org/3/search/tv?api_key={apiKey}&query={Uri.EscapeDataString(searchTitle)}&language=es-ES";
            var searchResponse = await client.GetAsync(searchUrl, cancellationToken);
            if (!searchResponse.IsSuccessStatusCode) return null;

            var searchJson = await searchResponse.Content.ReadFromJsonAsync<TMDbSearchResponse>(cancellationToken);
            var firstResult = searchJson?.results?.FirstOrDefault();
            if (firstResult == null) return null;

            var detailsUrl = $"https://api.themoviedb.org/3/tv/{firstResult.id}?api_key={apiKey}&language=es-ES";
            var detailsResponse = await client.GetAsync(detailsUrl, cancellationToken);
            if (!detailsResponse.IsSuccessStatusCode) return null;

            return await detailsResponse.Content.ReadFromJsonAsync<TMDbDetails>(cancellationToken);
        }

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