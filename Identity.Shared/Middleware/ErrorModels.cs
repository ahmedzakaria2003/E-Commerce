using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Identity.Shared.Middleware;

public record ErrorResponse
{
    public int StatusCode { get; init; }
    public ErrorBody Body { get; init; } = new();
}

public record ErrorBody
{
    public string Type { get; init; } = "about:blank";

    public string Title { get; init; } = string.Empty;

    public int Status { get; init; }

    public string Detail { get; init; } = string.Empty;
    public string? Instance { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<ValidationError>? Errors { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public string TraceId { get; init; } = Activity.Current?.Id ?? Guid.NewGuid().ToString();
}

public record ValidationError
{
    public string Field { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AttemptedValue { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; init; }
}

public class BusinessLogicException : Exception
{
    public string? ErrorCode { get; }

    public BusinessLogicException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessLogicException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
