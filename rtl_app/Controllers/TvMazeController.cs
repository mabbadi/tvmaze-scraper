using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace rtl_app.Controllers;
[ApiController]
[Route("[controller]")]
public class TvMazeController : ControllerBase
{
    private readonly ITvMazeStorage tvMazeStorage;
    private readonly IConsumer iConsumer;
    private readonly ApiKey apiKey;
    public TvMazeController(ITvMazeStorage tvMazeStorage, IConsumer iConsumer, IOptions<ApiKey> apiKey)
    {
        this.tvMazeStorage = tvMazeStorage;
        this.iConsumer = iConsumer;
        this.apiKey = apiKey.Value;
    }



    [HttpGet("search/shows", Name = "SearchShows")]
    public async Task<IActionResult> Search([FromQuery] int? page = 1, [FromQuery] string? q = "")
    {
        return Ok(await tvMazeStorage.Search(q, 2, page.HasValue ? page.Value : 1));
    }


    [HttpPost("process-all-data", Name = "ProcessAllData")]
    public async Task<IActionResult> ProcessAllData([FromQuery] string key)
    {
        if (key != apiKey.Key) return Unauthorized();
        await iConsumer.LoadUrls(true);
        return Ok();
    }


}