using ESearchtxt.Models;
using ESearchtxt.Services;
using Microsoft.AspNetCore.Mvc;

namespace ESearchtxt.Controllers;

public sealed class LogsController(ILogParserService logParserService) : Controller
{
    [HttpGet]
    public IActionResult Index([FromQuery] LogSearchParameters search)
    {
        var hydratedSearch = logParserService.BuildPromptOptions(search);
        var allEntries = logParserService.ReadAllEntries();
        var results = logParserService.Search(hydratedSearch);

        var model = new LogsIndexViewModel
        {
            Search = hydratedSearch,
            Results = results,
            TotalEntries = allEntries.Count,
            FilteredEntries = results.Count
        };

        return View(model);
    }
}
