using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AnilistTMDbFusion.Providers
{
    public static class TMDbClient
    {
        private static readonly HttpClient _http = new();

        public static async Task<JsonElement?> SearchAsync(string apiKey, string query)
        {
            try
            {
                var url = $"https://api.themoviedb.org/3/search/tv?api_key={apiKey}&language=es-ES&query={Uri.EscapeDataString(query)}";
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (doc.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    return results[0];
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
