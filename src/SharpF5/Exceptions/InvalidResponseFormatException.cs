namespace SharpF5.Exceptions
{
    public class InvalidResponseFormatException : InternalErrorException
    {
        public InvalidResponseFormatException()
        {
            message = "Invalid response format";
        }
    }
}
