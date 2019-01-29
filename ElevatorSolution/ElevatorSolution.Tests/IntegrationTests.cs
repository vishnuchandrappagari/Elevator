using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace ElevatorSolution.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInGroundFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedToFirstFloor()
        {
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);

            //Act            
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP,
                floorRequestAssignedElevator: (elevatorServingFloorRequest) =>
             {
                 elevatorServingFloorRequest.AddRequest(destinationFloor: 1,
                     servedRequestCallback: (elevatorServedRequest) =>
                   {
                       var actions = elevatorServedRequest.GetActions();

                       //Assert
                       Assert.True(actions.Count() == 1);
                       Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);

                       elevatorActionCompletedSignal.Set();
                   });

                 Thread.Sleep(100);
                 elevatorServingFloorRequest.CloosDoors();
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
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP,
                floorRequestAssignedElevator: (elevatorServingFloorRequest) =>
            {
                elevatorServingFloorRequest.AddRequest(destinationFloor: 2,
                    servedRequestCallback: (elevatorServedRequest) =>
                {
                    var actions = elevatorServedRequest.GetActions();

                    //Assert
                    Assert.True(actions.Count() == 2);
                    Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
                    Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 2), actions[1]);

                    elevatorActionCompletedSignal.Set();
                });

                Thread.Sleep(100);
                elevatorServingFloorRequest.CloosDoors();
            });

            //Waiting for elevator to complete the request
            elevatorActionCompletedSignal.WaitOne();
        }


        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedFromFirstToGroundAndThenToFirstFloor()
        {
            //Arrange : Elevator in second floor
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP,
                floorRequestAssignedElevator: (elevatorAssigned) =>
            {
                elevatorAssigned.AddRequest(destinationFloor: 1,
                    servedRequestCallback: (elevatorArrivedAtGrounfFloor) =>
                {
                    Thread.Sleep(100);
                    elevatorArrivedAtGrounfFloor.CloosDoors();
                    elevatorActionCompletedSignal.Set();
                });

                Thread.Sleep(100);
                elevatorAssigned.CloosDoors();
            });
            //Waiting for elevator to move to first floor
            elevatorActionCompletedSignal.WaitOne();


            //Act :  Request to second floor from first floor
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP, floorRequestAssignedElevator: (elevatorArrivedAtGrounfFloor) =>
               {
                   elevatorArrivedAtGrounfFloor.AddRequest(destinationFloor: 0,
                  servedRequestCallback: (elevatorServedRequest) =>
                  {
                      var actions = elevatorServedRequest.GetActions().ToArray();

                      //Assert
                      Assert.True(actions.Count() == 2);
                      Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
                      Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);

                      elevatorActionCompletedSignal.Set();
                  });

                   Thread.Sleep(100);
                   elevatorArrivedAtGrounfFloor.CloosDoors();
               });

            //Wait for completing request to second floor
            elevatorActionCompletedSignal.WaitOne();
        }


        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndDownRequestFollowedByGroundFloorRequestFromFirstFloor_ThenElevatorIsMovedFromFirstToGround()
        {
            //Arrange : Elevator in second floor
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequest.UP,
                floorRequestAssignedElevator: (elevatorAssigned) =>
                {
                    elevatorAssigned.AddRequest(destinationFloor: 1,
                        servedRequestCallback: (elevatorArrivedAtGrounfFloor) =>
                        {
                            Thread.Sleep(100);
                            elevatorArrivedAtGrounfFloor.CloosDoors();
                            elevatorActionCompletedSignal.Set();
                        });

                    Thread.Sleep(100);
                    elevatorAssigned.CloosDoors();
                });
            //Waiting for elevator to move to first floor
            elevatorActionCompletedSignal.WaitOne();


            //Act :  Request to ground floor from first floor
            building.AddFloorRequest(floorNumber: 1, requestDirection: FloorRequest.Down, floorRequestAssignedElevator: (elevatorArrivedAtFirstFloor) =>
            {
                elevatorArrivedAtFirstFloor.AddRequest(destinationFloor: 0,
               servedRequestCallback: (elevatorServedRequest) =>
               {
                   var actions = elevatorServedRequest.GetActions().ToArray();

                      //Assert
                      Assert.True(actions.Count() == 2);
                   Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
                   Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);

                   elevatorActionCompletedSignal.Set();
               });

                Thread.Sleep(100);
                elevatorArrivedAtFirstFloor.CloosDoors();
            });

            //Wait for completing request to second floor
            elevatorActionCompletedSignal.WaitOne();
        }
    }
}
