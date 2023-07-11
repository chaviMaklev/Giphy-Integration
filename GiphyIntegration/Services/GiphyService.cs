using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace GiphyIntegration.Services
{
    public class GiphyService : IGiphyService
    {
        
        private readonly IMemoryCache _cache;
        private readonly RestClient _restClient;
        private readonly IConfiguration _config;
        private string _giphyApiKey;


        public GiphyService(IMemoryCache cache, IConfiguration config)
        {
            _cache = cache;
            _config = config;
            _giphyApiKey = _config.GetValue<string>("Params:giphy.api_key");
            _restClient = new RestClient(_config.GetValue<string>("Params:giphy.url"));
        }
        public async Task<List<string>> GetTrendsGifs()
        {
            const string cacheKey = "TrendsGifs";

            if (_cache.TryGetValue(cacheKey, out List<string> cachedGifs))
            {
                return cachedGifs;
            }

            var request = new RestRequest("trending", Method.Get);
            request.AddParameter("api_key", _giphyApiKey);

            var response = await _restClient.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var gifs = GetUrlsFromResponse(response.Content);

                SaveToCache(cacheKey, gifs);

                return gifs;
            }

            throw new Exception("Failed to fetch trending GIFs.");
        }


        public async Task<List<string>> SearchGifs(string searchValue)
        {
            var cacheKey = $"Search_{searchValue}";

            if (_cache.TryGetValue(cacheKey, out List<string> cachedGifs))
            {
                return cachedGifs;
            }

            var request = new RestRequest("search", Method.Get);
            request.AddParameter("api_key", _giphyApiKey);
            request.AddParameter("q", searchValue);

            var response = await _restClient.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var gifs = GetUrlsFromResponse(response.Content);

                //In the case of a search by value, we must decide when to reset the cache in order to see updated values on the one hand and to prevent unnecessary requests on the other.
                //Currently the cache resets once a day.
                SaveToCache(cacheKey, gifs);
                return gifs;
            }

            throw new Exception("Failed to search for GIFs.");
        }

        private void SaveToCache(string cacheKey, List<string> gifs)
        {
            if (gifs != null && gifs.Count > 0)
            {
                var todayEndOfDay = DateTime.Today.AddDays(1).AddTicks(-1);
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = todayEndOfDay - DateTime.Now
                };

                _cache.Set(cacheKey, gifs, cacheOptions);
            }
        }

        private List<string> GetUrlsFromResponse(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;
                var dataArray = root.GetProperty("data").EnumerateArray();
                List<string> urls = new();
                foreach (var gif in dataArray)
                {
                    string url = gif.GetProperty("url").GetString();
                    urls.Add(url);
                }
                return urls;
            }
            return null;
        }
    }
}

