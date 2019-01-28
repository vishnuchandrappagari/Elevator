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
                       Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions.First());

                       elevatorActionCompletedSignal.Set();
                   });

                 //Closing the door after 10ms
                 Thread.Sleep(100);
                 elevator.CloosDoors();
             });

            //Waiting for elevator to complete the request
            elevatorActionCompletedSignal.WaitOne();
        }

        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedFromFirstToGroundAndThenToFirstFloor()
        {
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);

            //Act
            //Floor groundFloor = building.GetFloor(0);
            //groundFloor.SetMovingUpRequest((elevator) => {

            //    elevator.AddRequest(floor: 1);
            //    var actions = elevator.GetActions();

            //    //Assert
            //    Assert.True(actions.Count() == 2);
            //    Assert.Equal(new ElevatorAction(1, 0), actions.ToArray().ElementAt(0));
            //    Assert.Equal(new ElevatorAction(0, 1), actions.ToArray().ElementAt(0));
            //});
        }
    }
}
