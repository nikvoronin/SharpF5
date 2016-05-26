namespace SharpF5.Exceptions
{
    public class EmptyResponseException : InternalErrorException
    {
        public EmptyResponseException()
        {
            message = "Empty response";
        }
    }
}
