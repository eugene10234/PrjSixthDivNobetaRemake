using System;
using System.Collections.Generic;

namespace Nobeta.Sam.Pathfinding
{
    public class MinHeap<T>
    {
        private readonly List<(T item, float priority)> _data = new();

        public int Count => _data.Count;

        public void Enqueue(T item, float priority)
        {
            _data.Add((item, priority));
            BubbleUp(_data.Count - 1);
        }

        public T Dequeue()
        {
            if (_data.Count == 0) throw new InvalidOperationException("Heap is empty.");
            T top = _data[0].item;
            int last = _data.Count - 1;
            _data[0] = _data[last];
            _data.RemoveAt(last);
            if (_data.Count > 0) SiftDown(0);
            return top;
        }

        public void Clear() => _data.Clear();

        private void BubbleUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_data[parent].priority <= _data[i].priority) break;
                (_data[i], _data[parent]) = (_data[parent], _data[i]);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            int count = _data.Count;
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < count && _data[left].priority < _data[smallest].priority) smallest = left;
                if (right < count && _data[right].priority < _data[smallest].priority) smallest = right;

                if (smallest == i) break;
                (_data[i], _data[smallest]) = (_data[smallest], _data[i]);
                i = smallest;
            }
        }
    }
}
