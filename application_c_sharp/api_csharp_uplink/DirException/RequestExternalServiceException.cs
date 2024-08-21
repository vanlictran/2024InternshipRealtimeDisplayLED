namespace api_csharp_uplink.DirException;

public class RequestExternalServiceException : Exception
{
    public RequestExternalServiceException() { }
    public RequestExternalServiceException(string message) : base(message) { }
    public RequestExternalServiceException(string message, Exception innerException) : base(message, innerException) { }
}