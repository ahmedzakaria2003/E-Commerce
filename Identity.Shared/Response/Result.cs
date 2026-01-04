using Microsoft.AspNetCore.Http;

namespace Identity.Shared.Response;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string Error { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public IEnumerable<string> Errors { get; private set; } = [];
    public int StatusCode { get; private set; }

    protected Result(bool isSuccess, string error, string message, IEnumerable<string> errors, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
        Errors = errors;
        StatusCode = statusCode;
    }

    public static Result Success(string message = "")
        => new(true, string.Empty, message, [], StatusCodes.Status200OK);

    public static Result Failure(string error, int statusCode = StatusCodes.Status400BadRequest)
        => new(false, error, string.Empty, [error], statusCode);

    public static Result Failure(IEnumerable<string> errors, int statusCode = StatusCodes.Status400BadRequest)
        => new(false, string.Join(", ", errors), string.Empty, errors, statusCode);
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool isSuccess, T? value, string error, string message, IEnumerable<string> errors, int statusCode)
        : base(isSuccess, error, message, errors, statusCode)
    {
        Data = value;
    }

    public static Result<T> Success(T value, string message = "")
        => new(true, value, string.Empty, message, [], StatusCodes.Status200OK);

    public static new Result<T> Failure(string error, int statusCode = StatusCodes.Status400BadRequest)
        => new(false, default, error, string.Empty, [error], statusCode);

    public static new Result<T> Failure(IEnumerable<string> errors, int statusCode = StatusCodes.Status400BadRequest)
        => new(false, default, string.Join(", ", errors), string.Empty, errors, statusCode);
}
