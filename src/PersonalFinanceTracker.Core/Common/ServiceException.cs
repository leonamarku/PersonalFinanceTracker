namespace PersonalFinanceTracker.Core.Common;

/// <summary>Domain-level exception thrown by services when business rules are violated.</summary>
public class ServiceException : Exception
{
    public int StatusCode { get; }

    public ServiceException(string message, int statusCode = 400)
        : base(message) => StatusCode = statusCode;
}
