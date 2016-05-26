using System;
using System.Collections.Generic;

namespace SharpF5.Exceptions
{
    /// <summary>
    /// Internal error of a Combivert
    /// </summary>
    public class InternalErrorException : Exception
    {
        protected string message = "Unknown internal error";

        public InternalErrorException()
        {
        }

        public InternalErrorException(byte code)
        {
            if (code > 0)
                message = ErrorCodeToMessage(code);
        }
        
        public InternalErrorException(string message)
        {
            this.message = message;
        }
        
        public override string Message { get{ return message; }}

        private static Dictionary<byte, string> ErrorCodeMessagePair =
            new Dictionary<byte, string>
            {
                { 1, "Not ready" },
                { 2, "Address or password invalid" },
                { 3, "Data invalid" },
                { 4, "Parameter write-protected" },
                { 5, "BCC-error" },
                { 6, "Inverter / Operator busy" },
                { 7, "Service not available" },
                { 8, "Password invalid" },
                { 9, "Telegram-framing error: Wrong number of characters in the telegram" },
                {10, "Transmission error: Overrun-, frame-, parity error of one or several transmitted characters" },
                {11, "Set identification invalid" },
                {12, "Set identification invalid" },
                {13, "Address invalid" },
                {14, "Operation not possible" },
                {15, "Not used at present" }
            };

        public static string ErrorCodeToMessage(byte errorCode)
        {
            return
                ErrorCodeMessagePair.ContainsKey(errorCode) ?
                    ErrorCodeMessagePair[errorCode] :
                    null;
        }
    } // class
}
