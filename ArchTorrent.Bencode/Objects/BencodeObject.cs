using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchTorrent.Bencode.Objects
{
    public class BencodeObject
    {
        public List<BencodeObject> Objects { get; set; }
        public static BencodeInteger DecodeInteger(string str)
        {
            return new BencodeInteger(int.Parse(str.Substring(1, str.Length - 2)));
        }

        public static BencodeString DecodeString(string str)
        {
            string[] split = str.Split(':');

            return new BencodeString(split[1]);
        }

        public static BencodeList DecodeList(string str)
        {
            

            BencodeList list = new BencodeList();
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                // BencodeString
                if(char.IsNumber(c))
                {
                    int j = i;
                    while(str[j] != ':')
                    {
                        j++;
                    }

                    int numCharacters = int.Parse(str.Substring(i, j - 1));

                    BencodeString newString = DecodeString(str.Substring(i, j + numCharacters));
                    list.Add(newString);
                    i = j + numCharacters;
                }
                else if(c == 'i')
                {
                    int indexOfNextE = str.IndexOf('e', i);
                    // TODO: add 0 check
                    BencodeInteger newInteger = DecodeInteger(str.Substring(i + 1, indexOfNextE - 1));
                    list.Add(newInteger);
                }
                else if(c == 'l')
                {
                    // here lies the problem, can't call
                }
                else if(c == 'd')
                {

                }
            }

            return list;
        }

        public static BencodeDictionary DecodeDictionary(string str)
        {
            return default(BencodeDictionary);
        }
        public virtual string Encode()
        { return ""; }

        public BencodeObject()
        {
            Objects = new List<BencodeObject>();
        }
    }
}
