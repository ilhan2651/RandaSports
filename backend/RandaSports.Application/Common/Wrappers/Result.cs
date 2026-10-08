using System.Net;
using System.Text.Json.Serialization;

namespace RandaSports.Application.Common.Wrappers;

public class Result
{
    public List<string> Messages { get; set; } = [];
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public string? UrlAsCreated { get; set; }
    public string? ErrorCode { get; set; }

    [JsonIgnore]
    public bool IsFail => !Success;

    [JsonIgnore]
    public string Message => string.Join(" ", Messages);

    public static Result Ok(string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Messages = message is not null ? [message] : []
    };

    public static Result Created(string urlAsCreated, string? message = null) => new()
    {
        Success = true,
        StatusCode = HttpStatusCode.Created,
        UrlAsCreated = urlAsCreated,
        Messages = message is not null ? [message] : []
    };

    public static Result Fail(List<string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Messages = errors
    };

    public static Result Fail(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => Fail([error], statusCode);

    public static Result Fail(string error, string errorCode, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        Success = false,
        StatusCode = statusCode,
        ErrorCode = errorCode,
        Messages = [error]
    };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        StatusCode = HttpStatusCode.OK,
        Messages = message is not null ? [message] : []
    };

    public static Result<T> Created(T data, string urlAsCreated, string? message = null) => new()
    {
        Success = true,
        Data = data,
        UrlAsCreated = urlAsCreated,
        StatusCode = HttpStatusCode.Created,
        Messages = message is not null ? [message] : []
    };

    public static new Result<T> Fail(List<string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Messages = errors
    };

    public static new Result<T> Fail(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => Fail([error], statusCode);
}
