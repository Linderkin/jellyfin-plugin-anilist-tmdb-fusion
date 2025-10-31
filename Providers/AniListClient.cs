using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Providers
{
    public static class AniListClient
    {
        private static readonly HttpClient _http = new();

        public static async Task<string?> GetRomajiTitleAsync(string query)
        {
            try
            {
                const string endpoint = "https://graphql.anilist.co";
                var requestBody = new
                {
                    query = @"query ($search: String) { Media(search: $search, type: ANIME) { title { romaji } } }",
                    variables = new { search = query }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode) return null;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("Media", out var media) &&
                    media.TryGetProperty("title", out var title) &&
                    title.TryGetProperty("romaji", out var romajiProp))
                {
                    return romajiProp.GetString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
