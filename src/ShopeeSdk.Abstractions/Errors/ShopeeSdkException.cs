namespace Loscardos.ShopeeSdk.Abstractions.Errors;

public class ShopeeSdkException : Exception
{
    public ShopeeSdkException(string message) : base(message)
    {
    }

    public ShopeeSdkException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
