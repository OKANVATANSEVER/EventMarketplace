using System.Text.Json.Serialization;

namespace EventMarketplace.API.Responses;

public sealed class ErrorDto
{
    [JsonInclude]
    public List<string> Errors { get; set; } = [];

    [JsonInclude]
    public bool IsShow { get; set; }

    public ErrorDto()
    {
    }

    public ErrorDto(string error, bool isShow)
    {
        Errors = [error];
        IsShow = isShow;
    }

    public ErrorDto(List<string> errors, bool isShow)
    {
        Errors = errors;
        IsShow = isShow;
    }
}
