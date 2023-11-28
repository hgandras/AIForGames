using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prong
{

    /// <summary>
    /// Priority queue implementation by https://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;
        public int Size { get { return data.Count; } }

        public PriorityQueue()
        {
            data=new List<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (data[ci].CompareTo(data[pi]) >= 0)
                    break;
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            // Assumes pq isn't empty
            int li = data.Count - 1;
            T frontItem = data[0];
            data[0] = data[li];
            data.RemoveAt(li);

            --li;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0)
                    ci = rc;
                if (data[pi].CompareTo(data[ci]) <= 0) break;
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
                pi = ci;
            }
            return frontItem;
        }



    }
}
