using System;
using System.Text;

namespace StringProtocolLibrary
{
    public class HeaderHandler
    {
        //Header -> 3 (RES/REQ)
        //CMD -> 2
        //Largo -> 4(entero)
        //Datos -> Variable

        public byte[] GetHeaderBytes(Header header)
        {
            var formatheader = new byte[HeaderConstants.TypeLength + HeaderConstants.CommandLength + HeaderConstants.DataLength];

            var type = (int)header.Type;
            var bytetype = BitConverter.GetBytes(type); 

            var command = (int)header.Command;
            var bytecommand = BitConverter.GetBytes(command); 

            var lengthdata = header.DataLength;
            var bytedata = BitConverter.GetBytes(lengthdata);

            Array.Copy(bytetype,0,formatheader,0,HeaderConstants.TypeLength);
            Array.Copy(bytecommand, 0, formatheader, HeaderConstants.TypeLength, HeaderConstants.CommandLength);
            Array.Copy(bytedata, 0, formatheader, HeaderConstants.CommandLength + HeaderConstants.TypeLength, HeaderConstants.DataLength);

            return formatheader;
        }

        public Header DecodeHeader(byte[] byteheader)
        {
            try
            {
                byte[] type = new byte[HeaderConstants.TypeLength];
                byte[] command = new byte[HeaderConstants.CommandLength];
                byte[] data = new byte[HeaderConstants.DataLength];

                Array.Copy(byteheader, 0, type, 0, HeaderConstants.TypeLength);
                Array.Copy(byteheader, HeaderConstants.TypeLength, command, 0, HeaderConstants.CommandLength);
                Array.Copy(byteheader, HeaderConstants.TypeLength + HeaderConstants.CommandLength, data, 0, HeaderConstants.DataLength);

                HeaderType htype = (HeaderType)BitConverter.ToInt16(type, 0);
                CommandConstants hcommand = (CommandConstants)BitConverter.ToInt16(command, 0);
                int hdatalenght = BitConverter.ToInt32(data, 0);

                return new Header
                {
                    Type = htype,
                    Command = hcommand,
                    DataLength = hdatalenght
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("Error decoding data: " + e.Message);
                return null;
            }

        }
    }
}
