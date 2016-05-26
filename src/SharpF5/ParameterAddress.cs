using System;
using System.Collections.Generic;
using SharpF5.Exceptions;

namespace SharpF5
{
    public class ParameterAddress
    {
        protected string address;
        protected byte set;

        public string Address { get{ return address; }}
        public byte SetNo { get{ return set; }}

        protected void Construct(string address, byte set)
        {
            if (address.Length != 4)
                throw new IndexOutOfRangeException("Incorrect address length. Must be four characters length.");

            if (set > 3)
                throw new IndexOutOfRangeException("Set must be from 0 to 3");

            this.address = address;
            this.set = set;
        }

        /// <summary>
        /// Address of the parameter
        /// </summary>
        /// <param name="address"></param>
        /// <param name="set">0..3</param>
        public ParameterAddress(string address, byte set)
        {
            Construct(address, set);
        }
       
        public ParameterAddress(string address)
        {
            Construct(address, 0);
        }
         
        /// <summary>
        /// Convert string representation of parameter address to byte chain as ASCII symbols
        /// </summary>
        /// <param name="parameterAddress">String representation of parameter address for ex.: 'sy54', 'ru19'</param>
        /// <returns>Parameter address as byte array of ASCII symbols</returns>
        public byte[] GetBytes()
        {
            string groupName = address.Substring(0, 2).ToLower();
            string strGroup = GroupNameToHex(groupName);
            char[] chGroup = strGroup.ToCharArray();

            int idx = int.Parse(address.Substring(2, 2));

            char[] chAddress = idx.ToString("X2").ToCharArray();

            byte[] binAddr = new byte[4];

            binAddr[0] = (byte)chGroup[0];
            binAddr[1] = (byte)chGroup[1];
            binAddr[2] =
                groupName == "fb" ||
                groupName == "os" ||
                groupName == "db" ? (byte)'8' : (byte)chAddress[0];
            binAddr[3] = (byte)chAddress[1];

            return binAddr;
        }

        private static Dictionary<string, string> GroupNamesHexPairs =
            new Dictionary<string, string>
            {
                {"sy", "00"},
                {"ru", "02"},
                {"op", "03"},
                {"pn", "04"},
                {"uf", "05"},
                {"dr", "06"},
                {"cn", "07"},
                {"ud", "08"},
                {"fr", "09"},
                {"an", "0A"},
                {"di", "0B"},
                {"do", "0C"},
                {"le", "0D"},
                {"in", "0E"},
                {"cs", "0F"},
                {"ec", "10"},
                {"ds", "11"},
                {"aa", "12"},
                {"ps", "13"},
                {"pp", "33"},
                // operator's panel
                {"os", "01"},
                {"fb", "02"},
                {"db", "06"}
            };

        /// <summary>
        /// Translate group name ('op', 'sy', ...) to hex representation
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <returns>Hex representation of the group name</returns>
        protected string GroupNameToHex(string groupName)
        {
            if (!GroupNamesHexPairs.ContainsKey(groupName))
                throw new InvalidGroupNameException();

            return GroupNamesHexPairs[groupName];
        }
    } // class
}
