using System;

namespace SharpF5
{
    /// <summary>
    /// Container for display standarts of a combivert parameter
    /// </summary>
    public class DisplayStandart
    {
        public int Divisor = 1;
        public int Multiplier = 1;
        public int Offset = 0;
        public int Flags = 0;

        public static string[] FLAG_UNITS = { "", "mm", "cm", "m", "km", "g", "kg", "us", "ms", "s", "h", "Nm", "kNm", "m/s", "m/s²", "m/s³", "km/h", "1/min", "Hz", "kHz", "mV", "V", "kV", "mW", "W", "kW", "VA", "kVA", "mA", "A", "kA", "°C", "K", "mΩ", "Ω", "kΩ", "INC", "%", "kWh", "mH", "", "", "in", "ft", "yd", "oz", "lb", "lbft", "lbin", "in/s", "ft/s", "ft/min", "ft/s²", "ft/s³", "MPH", "hp", "psi", "°F", "", "", "", "", "", "" };

        /// <summary>
        /// Convert value to display represenation
        /// </summary>
        /// <param name="parameterValue">Raw value of the parameter</param>
        /// <returns>Display representation with units of the parameter value</returns>
        public string ToDisplayValue(double parameterValue)
        {
            double dispVal = parameterValue;
            switch ((Flags & 0xC0) >> 6)    // bit 6..7
            {
                case 0:
                    dispVal = (dispVal + Offset) * Multiplier / Divisor;
                    break;
                case 1:
                    dispVal = Multiplier / ((dispVal + Offset) * Divisor);
                    break;
            }

            string dispStr = string.Empty;

            switch ((Flags & 0x0F00) >> 8)    // bit 8..11
            {
                case 0:
                    dispStr = Math.Truncate(dispVal).ToString();
                    break;
                case 1:
                    dispStr = dispVal.ToString("F1");
                    break;
                case 2:
                    dispStr = dispVal.ToString("F2");
                    break;
                case 3:
                    dispStr = dispVal.ToString("F3");
                    break;
                case 4:
                    dispStr = dispVal.ToString("F4");
                    break;
                case 5:
                    dispStr = dispVal.ToString();
                    break;
                case 6:
                    dispStr = ((int)dispVal).ToString("X4");
                    break;
                case 7:
                    dispStr = dispVal.ToString();
                    break;
            }

            return string.Concat(dispStr, " ", FLAG_UNITS[Flags & 0x3F]);
        }

        /// <summary>
        /// Convert display representation of the value to combivert representation
        /// </summary>
        /// <param name="parameterValue">Display representation of the parameter value</param>
        /// <returns></returns>
        public int ToParameterValue(double parameterValue)
        {
            double rawVal = (double)parameterValue;
            switch ((Flags & 0xC0) >> 6)    // bit 6..7
            {
                case 0:
                    rawVal = rawVal * Divisor / Multiplier - Offset;
                    break;
                case 1:
                    rawVal = 1 / rawVal;
                    rawVal = rawVal * Multiplier / Divisor - Offset;
                    break;
            }

            return (int)rawVal;
        }

        /// <summary>
        /// Parameter no need conversation
        /// </summary>
        public static DisplayStandart TRANSPARENT { get{ return new DisplayStandart(); }}
    } // class
}
