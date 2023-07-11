using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiphyIntegration.Services
{
    public interface IGiphyService
    {
        Task<List<string>> GetTrendsGifs();
        Task<List<string>> SearchGifs(string searchValue);
    }
}
