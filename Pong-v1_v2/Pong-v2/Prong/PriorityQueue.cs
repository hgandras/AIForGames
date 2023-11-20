using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prong
{
    internal class PriorityQueue<T>
    {
        private List<Item> queue= new List<Item>();
        public int Size { get {  return queue.Count; } }

        public class Item : IComparable<Item>
        {
            public T item { get; set; }
            public float priority { get; set; }

            public int CompareTo(Item other)
            {
                if (other.priority > this.priority)
                    return 1;
                else if (other.priority == this.priority)
                    return 0;
                return -1;
            }
        }

        /// <summary>
        /// Inserts the elements based on the priority. It inserts one element, and than sorts the list, based on priority.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        public void Enqueue(T item, float priority)
        {
            Item element = new Item() { item=item, priority=priority };
            queue.Add(element);
            queue.Sort(); 
        }

        /// <summary>
        /// Pops the first element from the list.
        /// </summary>
        /// <returns>The first item on the list, which has the highest priority.</returns>
        public T Pop()
        {
            T item = Peek();
            queue.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Returns the first element from the list.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if(Empty())
                throw new InvalidOperationException("Queue is emty!");
            return queue[0].item;
        }

        /// <summary>
        /// Returns if the list is empty.
        /// </summary>
        /// <returns></returns>
        public bool Empty()
        { 
            return queue.Count == 0; 
        }
    }
}
