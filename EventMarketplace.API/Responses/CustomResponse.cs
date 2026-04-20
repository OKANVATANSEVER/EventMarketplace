using System.Text.Json.Serialization;

namespace EventMarketplace.API.Responses;

public sealed class CustomResponse<T>
{
    [JsonInclude]
    public T? Data { get; set; }

    [JsonInclude]
    public int StatusCode { get; set; }

    [JsonInclude]
    public ErrorDto? Error { get; set; }

    [JsonInclude]
    public bool IsSuccessful { get; set; }

    public static CustomResponse<T> Success(T data, int statusCode)
    {
        return new CustomResponse<T>
        {
            Data = data,
            StatusCode = statusCode,
            IsSuccessful = true
        };
    }

    public static CustomResponse<T> Success(int statusCode)
    {
        return new CustomResponse<T>
        {
            Data = default,
            StatusCode = statusCode,
            IsSuccessful = true
        };
    }

    public static CustomResponse<T> Fail(ErrorDto errorDto, int statusCode)
    {
        return new CustomResponse<T>
        {
            Error = errorDto,
            StatusCode = statusCode,
            IsSuccessful = false
        };
    }

    public static CustomResponse<T> Fail(string errorMessage, int statusCode, bool isShow)
    {
        var errorDto = new ErrorDto(errorMessage, isShow);
        return new CustomResponse<T>
        {
            Error = errorDto,
            StatusCode = statusCode,
            IsSuccessful = false
        };
    }
}
