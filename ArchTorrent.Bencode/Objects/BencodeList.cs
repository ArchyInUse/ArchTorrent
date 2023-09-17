namespace ArchTorrent.Bencode.Objects
{
    public class BencodeList : BencodeObject
    {
        public List<BencodeObject> Items { get; set; }

        public BencodeList(IEnumerable<BencodeObject> items)
        {
            Items = new List<BencodeObject>(items);
        }

        public BencodeList()
        {
            Items = new List<BencodeObject>();
        }

        public void Add(BencodeObject item) => Items.Add(item);

        public override string Encode()
        {
            string ret = "l";
            foreach (var item in Items)
            {
                ret += item.ToString();
            }
            ret += "e";
            return ret;
        }

        public override string ToString() => Encode();
    }
}