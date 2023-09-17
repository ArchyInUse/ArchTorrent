namespace ArchTorrent.Bencode.Objects
{
    public class BencodeString : BencodeObject
    {
        public string Value { get; set; }

        public BencodeString(string value)
        {
            Value = value;
        }

        public override string Encode()
        {
            return $"{Value.Length}:{Value}";
        }

        public override string ToString()
        {
            return Value;
        }
    }
}