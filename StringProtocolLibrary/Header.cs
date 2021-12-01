using System;
using System.Collections.Generic;
using System.Text;

namespace StringProtocolLibrary
{
    public class Header
    {
        public CommandConstants Command { get; set; }

        public int DataLength { get; set; }

        public HeaderType Type { get; set; }

    }
}
