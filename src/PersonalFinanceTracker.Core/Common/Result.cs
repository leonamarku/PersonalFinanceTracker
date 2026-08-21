namespace PersonalFinanceTracker.Core.Common;

/// <summary>Generic result wrapper used by all service methods.</summary>
public class Result<T>
{
    public bool    IsSuccess { get; private set; }
    public T?      Data      { get; private set; }
    public string? Error     { get; private set; }
    public int     StatusCode { get; private set; }

    private Result() { }

    public static Result<T> Success(T data, int statusCode = 200)
        => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

    public static Result<T> Failure(string error, int statusCode = 400)
        => new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}
