namespace api_csharp_uplink.DirException;

public class DbException(string messageError) : Exception(messageError);