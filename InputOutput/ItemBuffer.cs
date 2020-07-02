namespace Industropolis.Sim
{
    public class ItemBuffer
    {
        public int Buffer { get; set; }
        public int BufferSize { get; private set; }
        public Item Item { get; private set; }

        public bool CanInsert => Buffer < BufferSize;
        public bool CanRemove => Buffer > 0;

        public ItemBuffer(int bufferSize, Item item)
        {
            BufferSize = bufferSize;
            Item = item;
        }

        public void Insert()
        {
            if (CanInsert) Buffer++;
        }

        public void Remove()
        {
            if (CanRemove) Buffer--;
        }
    }
}
