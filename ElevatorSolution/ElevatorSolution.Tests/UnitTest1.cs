using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace ElevatorSolution.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInGroundFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedToFirstFloor()
        {
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);

            //Act            
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP, elevatorArrivedAtFloorcallback: (elevator) =>
             {            
                 elevator.AddRequest(destinationFloor: 1, servedRequestCallback: (elevatorServedRequest) =>
                   {
                       var actions = elevatorServedRequest.GetActions();

                       //Assert
                       Assert.True(actions.Count() == 1);
                       Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);

                       elevatorActionCompletedSignal.Set();
                   });

                 //Closing the door after 100ms
                 Thread.Sleep(100);
                 elevator.CloosDoors();
             });

            //Waiting for elevator to complete the request
            elevatorActionCompletedSignal.WaitOne();
        }

        [Fact]
        public void GivenTwoFloorAndOneElevator_WhenElevatorIsInGroundFloorAndUpFollowedBySecondFloorIsPressedFromGroundFloor_ThenElevatorIsMovedToSecondFloor()
        {
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 2, numberOfElevators: 1);
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);

            //Act            
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP, elevatorArrivedAtFloorcallback: (elevator) =>
            {
                elevator.AddRequest(destinationFloor: 2, servedRequestCallback: (elevatorServedRequest) =>
                {
                    var actions = elevatorServedRequest.GetActions();

                    //Assert
                    Assert.True(actions.Count() == 2);
                    Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
                    Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 2), actions[1]);

                    elevatorActionCompletedSignal.Set();
                });

                //Closing the door after 100ms
                Thread.Sleep(100);
                elevator.CloosDoors();
            });

            //Waiting for elevator to complete the request
            elevatorActionCompletedSignal.WaitOne();
        }


        //[Fact]
        //public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedFromFirstToGroundAndThenToFirstFloor()
        //{
        //    //Arrange
        //    Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
        //    AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);            
        //    building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP, elevatorArrivedAtFloorcallback: (elevator) =>
        //    {
        //        elevator.AddRequest(destinationFloor: 1, servedRequestCallback: (elevatorServedRequest) =>
        //        {                   
        //            elevatorActionCompletedSignal.Set();
        //            elevatorServedRequest.CloosDoors();
        //        });

        //        //Closing the door after 100ms
        //        Thread.Sleep(100);
        //        elevator.CloosDoors();
        //    });
        //    //Waiting for elevator to move to first floor
        //    elevatorActionCompletedSignal.WaitOne();


        //    //Act            
        //    building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP, elevatorArrivedAtFloorcallback: (elevator) =>
        //    {
        //        Assert.Equal(ElevatorState.Stopped, elevator.State);
        //        elevator.AddRequest(destinationFloor: 1, servedRequestCallback: (elevatorServedRequest) =>
        //        {
        //            var actions = elevatorServedRequest.GetActions().ToArray();

        //            //Assert
        //            Assert.True(actions.Count() == 3);
        //            Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);
        //            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[1]);

        //            elevatorActionCompletedSignal.Set();
        //        });

        //        //Closing the door after 100ms
        //        Thread.Sleep(100);
        //        elevator.CloosDoors();
        //    });

        //    elevatorActionCompletedSignal.WaitOne();
        //}
    }
}
