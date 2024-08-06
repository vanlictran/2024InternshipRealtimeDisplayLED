namespace api_csharp_uplink.DirException;

public class AlreadyCreateException : Exception
{
    public AlreadyCreateException() { }
    public AlreadyCreateException(string message) : base(message) { }
    public AlreadyCreateException(string message, Exception innerException) : base(message, innerException) { }
}