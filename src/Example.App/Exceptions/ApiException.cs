using System.Net;

namespace Example.App.Exceptions;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; private set; }
}
