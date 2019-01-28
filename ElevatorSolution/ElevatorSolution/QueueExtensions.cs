using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElevatorSolution
{
    public static class QueueExtensions
    {
        public static void Remove<T>(this Queue<T> queue, T itemToRemove) where T : class
        {
            var list = queue.ToList(); 
            queue.Clear();
            foreach (var item in list)
            {
                if (item == itemToRemove)
                    continue;

                queue.Enqueue(item);
            }
        }
    }
}
