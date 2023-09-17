using ArchTorrent.Bencode.Objects;

namespace ArchTorrent.Bencode
{
    public class Parser
    {
        public BencodeObject Root { get; set; }

        public Parser()
        {

        }

        public BencodeObject Parse(string str)
        {
            // 1. take out all strings (remember where they are)
            // 2. get indexes for all i, d, l (number, dictionary, list)
            // 3. get the scopes of each of them by getting the latest i to the latest e
            // you get i d l from the start so you want to reverse the list to begin at the start
            // or just use a queue like a normal person
            // maybe make a struct for index , letter and then you can literally reconstruct the string
            // so create a structure 

            // take out the strings
            return default(BencodeObject);
        }

        private List<parseStr> pullStrings(string str)
        {
            List<parseStr> ret = new List<parseStr>();

            for(int i = 0; i < str.Length; i++)
            {
                // if char is numeric, we count the amount of characters that are numeric currently
                if(char.IsNumber(str[i]))
                {
                    //
                }
            }

            return ret;
        }

        struct parseStr
        {
            int index;
            string str;
        }
    }
}