namespace POS.API.Models;

public class OperationResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static OperationResult Ok() => new() { Success = true, StatusCode = 200 };
    public static OperationResult Fail(string errorMessage, int statusCode = 400) => new() { Success = false, ErrorMessage = errorMessage, StatusCode = statusCode };
}

public class OperationResult<T> : OperationResult
{
    public T? Payload { get; init; }

    public static OperationResult<T> Ok(T payload) => new() { Success = true, StatusCode = 200, Payload = payload };
    public new static OperationResult<T> Fail(string errorMessage, int statusCode = 400) => new() { Success = false, ErrorMessage = errorMessage, StatusCode = statusCode };
}
