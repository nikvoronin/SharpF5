namespace SharpF5.Exceptions
{
    /// <summary>
    /// Possible that connection to Combivert was lost
    /// </summary>
    public class ConnectionLostException : InternalErrorException
    {
        public ConnectionLostException()
        {
            message = "Connection lost";
        }
    }
}
