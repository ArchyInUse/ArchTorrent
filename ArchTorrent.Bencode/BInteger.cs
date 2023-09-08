using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Bencode
{
    internal class BInteger
    {
        public UInt64 Value {  get; set; }

        public BInteger(UInt64 value)
        {
            Value = value;
        }

        public BInteger(int value)
        {
            Value = Convert.ToUInt64(value);
        }

        public string Encode()
        {
            return $"i{Value}e";
        }

        public static BInteger Decode(string str)
        {
            if(!str.StartsWith('i') || !str.EndsWith('e'))
            {
                throw new ArgumentException($"Parameter `str` does not start with i and ends with e\nFull string: {str}", "str");
            }

            return new BInteger(Convert.ToUInt64(str[1..^2]));
        }
    }
}
