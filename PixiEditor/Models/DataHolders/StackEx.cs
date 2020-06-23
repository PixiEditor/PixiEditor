using System.Collections.Generic;

namespace PixiEditor.Models.DataHolders
{
    public class StackEx<T>
    {
        public int Count => items.Count;

        public T First => items[0];
        private readonly List<T> items = new List<T>();

        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        ///     Returns top object without deleting it.
        /// </summary>
        /// <returns>Returns n - 1 item from stack.</returns>
        public T Peek()
        {
            return items[items.Count - 1];
        }

        public void Push(T item)
        {
            items.Add(item);
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T temp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return temp;
            }

            return default;
        }

        public void PushToBottom(T item)
        {
            items.Insert(0, item);
        }

        public void Remove(int itemAtPosition)
        {
            items.RemoveAt(itemAtPosition);
        }
    }
}