using GiphyIntegration.Models;
using GiphyIntegration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GiphyIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiphyController : Controller
    {
        private readonly IGiphyService _giphyService;

        public GiphyController(IGiphyService giphyService)
        {
            _giphyService = giphyService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingGifs()
        {
            try
            {
                var gifs = await _giphyService.GetTrendsGifs();
                return Ok(gifs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGifs([FromQuery] string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
            {
                return BadRequest("Search term is required.");
            }

            try
            {
                var gifs = await _giphyService.SearchGifs(searchValue);
                return Ok(gifs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
