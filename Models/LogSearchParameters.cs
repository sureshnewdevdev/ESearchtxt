using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESearchtxt.Models;

public sealed class LogSearchParameters
{
    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public string? Level { get; set; }

    public string? Application { get; set; }

    public string? EventName { get; set; }

    public string? UserId { get; set; }

    public string? MessageContains { get; set; }

    public bool OnlyExceptions { get; set; }

    public List<SelectListItem> LevelOptions { get; set; } = [];

    public List<SelectListItem> ApplicationOptions { get; set; } = [];

    public List<SelectListItem> EventOptions { get; set; } = [];

    public List<SelectListItem> UserOptions { get; set; } = [];
}
