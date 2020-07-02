using System.Collections.Generic;

#nullable disable

// Shitty SortedList-based priority queue
namespace Industropolis.Sim
{
    public class PriorityQueue<T1, T2>
    {

        private SortedList<T2, Queue<T1>> _data = new SortedList<T2, Queue<T1>>();
        private int _count = 0;

        public int Count => _count;

        public void Enqueue(T1 item, T2 key)
        {
            Queue<T1> entry;
            _data.TryGetValue(key, out entry);
            if (entry == null)
            {
                entry = new Queue<T1>();
                _data[key] = entry;
            }
            entry.Enqueue(item);
            _count++;
        }

        public T1 Dequeue()
        {
            if (_count > 0)
            {
                var key = _data.Keys[0];
                var entry = _data[key];
                T1 item = entry.Dequeue();
                _count--;
                if (entry.Count == 0) _data.Remove(key);
                return item;
            }
            else throw new System.InvalidOperationException("Queue is empty");
        }

        public void Clear()
        {
            _data.Clear();
            _count = 0;
        }
    }
}
