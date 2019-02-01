using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ElevatorSolution;

namespace ElevatorSolution.Tests
{
    public class QueueExtensionsTests
    {
        [Fact]
        public void GivenThreeItemsInQueue_WhenRemoveAtIsCalledWithSecondItem_ThenUpdatedQueueIsReturned()
        {
            //Arrange
            Queue<string> queue = new Queue<string>();
            queue.Enqueue("item 1");
            queue.Enqueue("item 2");
            queue.Enqueue("item 3");

            //Act
            queue.Remove("item 2");

            //Assert
            Assert.Equal(2, queue.Count);
            Assert.Equal("item 1", queue.Dequeue());
            Assert.Equal("item 3", queue.Dequeue());
        }


        [Fact]
        public void GivenThreeItemsInQueue_WhenRemoveAtIsCalledWithNonExistingItem_ThenSameQueueIsReturned()
        {
            //Arrange
            Queue<string> queue = new Queue<string>();
            queue.Enqueue("item 1");
            queue.Enqueue("item 2");
            queue.Enqueue("item 3");

            //Act
            queue.Remove("item 4");

            //Assert
            Assert.Equal(3, queue.Count);
            Assert.Equal("item 1", queue.Dequeue());
            Assert.Equal("item 2", queue.Dequeue());
            Assert.Equal("item 3", queue.Dequeue());
        }
    }
}
