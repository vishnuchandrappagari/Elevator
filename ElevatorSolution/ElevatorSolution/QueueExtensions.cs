using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElevatorSolution
{
    public static class QueueExtensions
    {
        public static void Remove<T>(this Queue<T> queue, T itemToRemove) where T : class
        {
            var list = queue.Where(item => item != itemToRemove).ToList();
            queue.Clear();
            list.ForEach(item => queue.Enqueue(item));
        }
    }
}
