using System;
using System.IO;
using SharpF5.Exceptions;
using System.Globalization;
using SharpF5.Common;

namespace SharpF5
{
    public class F5
    {
        public const byte STX = 0x02;
        public const byte ETX = 0x03;
        public const byte EOT = 0x04;
        public const byte ENQ = 0x05;
        public const byte ACK = 0x06;
        public const byte NAK = 0x15;

        protected Stream stream = null;
        protected byte iid = 0;

        private byte[] setsMask =
            { 
                Parameter.SET_0,
                Parameter.SET_1,
                Parameter.SET_2,
                Parameter.SET_3
            };


        protected byte IID
        {
            get
            {
                iid = (byte)(iid >= 15 ? 1 : iid + 1);
                return iid;
            }
        }
        
        public F5(Stream networkStream)
        {
            stream = networkStream;
        }

        public void SelectInverter(int index)
        {
            byte[] telegram = new byte[10];

            char[] ascIID = IID.ToString("X1").ToCharArray();
            telegram[0] = EOT;
            telegram[1] = (byte)'0';
            telegram[2] = (byte)(index + (byte)'0');

            int bytes = 0;
            byte[] response = new byte[256];
            try
            {
                stream.Write(telegram, 0, telegram.Length);
                bytes = stream.Read(response, 0, response.Length);
            }
            catch
            {
                throw new ConnectionLostException();
            }
        } // SelectInverter()

        public int ReadValue(string parameterName, byte setNo = Parameter.SET_0)
        {
            return
                ReadValue(
                    new Parameter(parameterName, setNo));
        }
                    
        public int ReadValue(Parameter parameter)
        {
            byte[] telegram = new byte[10];

            char[] ascIID = IID.ToString("X1").ToCharArray();
            telegram[0] = (byte)'G';
            telegram[1] = (byte)ascIID[0];
            byte[] ascAddress = parameter.GetAddressBytes();
            ascAddress.CopyTo(telegram, 2);
            telegram[6] = (byte)'0';
            telegram[7] = (byte)(setsMask[parameter.SetNo] + '1');
            telegram[8] = ENQ;
            telegram[9] = BCC(telegram, 0, 9);

            int bytes = 0;
            byte[] response = new byte[256];
            try
            {
                stream.Write(telegram, 0, telegram.Length);
                bytes = stream.Read(response, 0, response.Length);
            }
            catch
            {
                throw new ConnectionLostException();
            }

            int value = 0;

            if (bytes == 0)
                throw new EmptyResponseException();

            if (response[0] == STX)
            {
                value = AsciiValueToInt(response, 3, 8);
                parameter.Value = value;
            }
            else
            {
                byte errorCode = 0;
                try
                {
                    char[] tmp = new char[1];
                    tmp[0] = (char)response[1];
                    string str = new string(tmp);
                    errorCode = byte.Parse(str, NumberStyles.HexNumber);
                }
                finally
                {
                    throw new InternalErrorException(errorCode);
                }
            } // else if
            
            return value;
        } // WriteValue()

        public void WriteValue(int value, string parameterName, byte setNo = Parameter.SET_0)
        {
            WriteValue(
                value,
                new Parameter(parameterName, setNo));
        }

        public void WriteValue(Parameter parameter)
        {
            WriteValue(parameter.Value, parameter);
        }

        public void WriteValue(int value, Parameter parameter)
        {
            byte[] telegram = new byte[19];

            telegram[0] = STX;
            telegram[1] = (byte)'G';

            char[] ascIID = IID.ToString("X1").ToCharArray();
            telegram[2] = (byte)ascIID[0];

            byte[] ascAddress = parameter.GetAddressBytes();
            ascAddress.CopyTo(telegram, 3);

            char[] ascValue = value.ToString("X8").ToCharArray();
            for (int i = 0; i < ascValue.Length; i++)
                telegram[i + 7] = (byte)ascValue[i];

            telegram[15] = (byte)'0';
            telegram[16] = (byte)(setsMask[parameter.SetNo] + '1');
            telegram[17] = ETX;
            telegram[18] = BCC(telegram, 1, 17);

            int bytes = 0;
            byte[] response = new byte[255];
            try
            {
                stream.Write(telegram, 0, telegram.Length);
                bytes = stream.Read(response, 0, response.Length);
            }
            catch
            {
                throw new ConnectionLostException();
            }

            if (bytes == 0)
                throw new EmptyResponseException();

            if (response[1] != ACK)
            {
                byte errorCode = 0;
                try
                {
                    char[] tmp = new char[1];
                    tmp[0] = (char)response[1];
                    string str = new string(tmp);
                    errorCode = byte.Parse(str, NumberStyles.HexNumber);
                }
                finally
                {
                    throw new InternalErrorException(errorCode);
                }
            } // if
        } // ReadValue()

        public DisplayStandart GetDisplayStandart(string parameterName, byte setNo = Parameter.SET_0)
        {
            return
                GetDisplayStandart(
                    new Parameter(parameterName, setNo));
        }

        public DisplayStandart GetDisplayStandart(Parameter parameter)
        {
            byte[] telegram = new byte[8];

            char[] ascIID = IID.ToString("X1").ToCharArray();
            telegram[0] = (byte)'L';
            telegram[1] = (byte)ascIID[0];
            byte[] ascAddress = parameter.GetAddressBytes();
            ascAddress.CopyTo(telegram, 2);
            telegram[6] = ENQ;
            telegram[7] = BCC(telegram, 0, 7);

            int bytes = 0;
            byte[] response = new byte[256];
            try
            {
                stream.Write(telegram, 0, telegram.Length);
                bytes = stream.Read(response, 0, response.Length);
            }
            catch
            {
                throw new ConnectionLostException();
            }

            if (bytes == 0)
                throw new EmptyResponseException();

            DisplayStandart standart = DisplayStandart.TRANSPARENT;
            if (response[0] == STX)
            {
                standart = DisplayStandart.TRANSPARENT;
                standart.Divisor = AsciiValueToInt(response, 3, 4);
                standart.Multiplier = AsciiValueToInt(response, 7, 4);
                standart.Offset = AsciiValueToInt(response, 11, 4);
                standart.Flags = AsciiValueToInt(response, 15, 4);
            }
            else
            {
                byte errorCode = 0;
                try
                {
                    char[] tmp = new char[1];
                    tmp[0] = (char)response[1];
                    string str = new string(tmp);
                    errorCode = byte.Parse(str, NumberStyles.HexNumber);
                }
                finally
                {
                    throw new InternalErrorException(errorCode);
                }
            } // else if

            return standart;
        } // GetDisplayStandart()

        /// <summary>
        /// Convert ASCII representation of a parameter value to integer
        /// </summary>
        /// <param name="buffer">Byte buffer of a responsed telegram</param>
        /// <param name="offset">Start convert from this buffer index</param>
        /// <param name="length">Length of the ASCII value</param>
        /// <returns></returns>
        public static int AsciiValueToInt(byte[] buffer, byte offset, byte length)
        {
            char[] ascData = new char[length];
            for (int i = 0; i < length; i++)
                ascData[i] = (char)buffer[i + offset];

            return
                int.Parse(
                    new string(ascData),
                    NumberStyles.HexNumber);
        }

        /// <summary>
        /// Block Check Character (BCC) Calculation
        /// </summary>
        /// <param name="buffer">Input buffer for calculation</param>
        /// <param name="offset">Start calculation from this buffer index</param>
        /// <param name="length">Number of bytes to calculate</param>
        /// <returns>BCC sum</returns>
        public static byte BCC(byte[] buffer, int offset, int length)
        {
            if (length > buffer.Length ||
                offset >= buffer.Length)
            {
                throw new IndexOutOfRangeException("Incorrect length/offset values");
            }

            byte bcc = 0;

            int endTo = offset + length;
            for (int i = offset; i < endTo; i++)
                bcc ^= buffer[i];

            return (byte)(bcc < 0x20 ? bcc + 0x20 : bcc);
        }

        /// <summary>
        /// Digital Inputs
        /// </summary>
        public enum DI : ushort
        {
            ST  = 1,
            RST = 2,
            F   = 4,
            R   = 8,
            I1  = 16,
            I2  = 32,
            I3  = 64,
            I4  = 128,
            IA  = 256,
            IB  = 512,
            IC  = 1024,
            ID  = 2048,
            ResetAll = 0x0fff
        }

        /// <summary>
        /// Digital Outputs
        /// </summary>
        public enum DO : ushort
        {
            O1 = 0x01,
            O2 = 0x02,
            R1 = 0x04,
            R2 = 0x08,
            OA = 0x10,
            OB = 0x20,
            OC = 0x40,
            OD = 0x80,
            NoOutput = 0x00ff
        }
    } // class
}
