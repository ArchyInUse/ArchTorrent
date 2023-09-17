namespace ArchTorrent.Bencode.Objects
{
    public class BencodeInteger : BencodeObject
    {
        public int Value { get; set; }
        public BencodeInteger(int value)
        {
            Value = value;
        }

        public override string Encode()
        {
            return $"i{Value}e";
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}