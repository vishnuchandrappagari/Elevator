using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace ElevatorSolution.Tests
{
    public class BuildingTests
    {

        #region OneElevator


        /// <summary>
        /// Scenario: Elevator in requested floor and move up one floor
        /// Elevators - 1
        /// Floors - 1
        /// E1 in first floor
        /// Request : Ground floor -> First Floor 
        /// Expected : E1 :Ground floor -> First Floor 
        /// </summary>
        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInGroundFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedToFirstFloor()
        {
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);            

            //Act
            Elevator elevatorServedRequest = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            var actions = elevatorServedRequest.GetActions();
            //Assert
            Assert.True(actions.Count() == 1);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
        }



        /// <summary>
        /// Scenario: Elevator in requested floor and move up two floor
        /// Elevators - 1
        /// Floors - 2
        /// E1 in first floor
        /// Request : Ground floor -> Second Floor 
        /// Expected : E1 :Ground floor -> Second Floor 
        [Fact]
        public void GivenTwoFloorAndOneElevator_WhenElevatorIsInGroundFloorAndUpFollowedBySecondFloorIsPressedFromGroundFloor_ThenElevatorIsMovedToSecondFloor()
        {
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 2, numberOfElevators: 1);            

            Elevator elevatorServedRequest = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 2);
            var actions = elevatorServedRequest.GetActions();

            //Assert
            Assert.True(actions.Count() == 2);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
            Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 2), actions[1]);

        }

        /// <summary>
        /// Scenario: Elevator not in requested floor and move up one floor
        /// Elevators - 1
        /// Floors - 1
        /// E1 in Second floor
        /// Request : First floor -> Ground Floor 
        /// Expected : E1 :First floor -> Ground Floor 
        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndUpFollowedByFirstFloorIsPressedFromGroundFloor_ThenElevatorIsMovedFromFirstToGroundAndThenToFirstFloor()
        {    
            //Arrange
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            //Move elevator to First floor
           Elevator elevatorReachedFirstFloor= building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            //Act
            Elevator elevatorServedRequest = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            var actions = elevatorServedRequest.GetActions().ToArray();

            //Assert
            Assert.True(actions.Count() == 3);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
            Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[1]);
        }

        /// <summary>
        /// Scenario: Elevator in requested floor and move down one floor
        /// Elevators - 1
        /// Floors - 1
        /// E1 in first floor
        /// Request : Ground floor -> First Floor 
        /// Expected : E1 :Ground floor -> First Floor 
        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndDownRequestFollowedByGroundFloorRequestFromFirstFloor_ThenElevatorIsMovedFromFirstToGround()
        {
            //Arrange : Elevator in second floor
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP,
                elevatorArrivedAtRequestFloor: (elevatorAssigned) =>
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
            building.AddFloorRequest(floorNumber: 1, requestDirection: FloorRequestDirection.Down, elevatorArrivedAtRequestFloor: (elevatorArrivedAtFirstFloor) =>
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

            //Wait for completing request to ground floor
            elevatorActionCompletedSignal.WaitOne();
        }

        #endregion

        #region TwoElevators


        /// <summary>
        /// Elevators - 2
        /// Floors - 2
        /// E1 in first floor, E2 in second floor
        /// Request : Ground floor -> First Floor 
        /// Expected : E1 Serves the request
        /// </summary>
        [Fact]
        public void GivenTwoElevatorsTwoFloors_WhenOneElevatorIsInFirstFloorAndOtherOneInSecondFloorSecondFloorFirstFloorIsRequestedFromGround_ThenElevatorIsFirstFloorServes()
        {

            AutoResetEvent e1MovedSignal = new AutoResetEvent(false);
            AutoResetEvent e2MovedSignal = new AutoResetEvent(false);

            //Arrange 
            Building building = new Building(minFloor: 0, maxfloor: 2, numberOfElevators: 2);

            Elevator elevatorInFirstFloor;
            //Moving E1 to first floor
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP,
                elevatorArrivedAtRequestFloor: (elevatorAssigned) =>
                {
                    elevatorInFirstFloor = elevatorAssigned;
                    elevatorAssigned.AddRequest(destinationFloor: 1,
                        servedRequestCallback: (elevatorServedRequest) =>
                        {
                            elevatorServedRequest.CloosDoors();
                            e1MovedSignal.Set();
                        });

                    elevatorAssigned.CloosDoors();
                });

            Elevator elevatorInSecondFloor;
            //Moving E2 to Second floor 
            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP,
                elevatorArrivedAtRequestFloor: (elevatorAssigned) =>
                {
                    elevatorInSecondFloor = elevatorAssigned;

                    elevatorAssigned.AddRequest(destinationFloor: 2,
                        servedRequestCallback: (elevatorServedRequest) =>
                        {
                            elevatorServedRequest.CloosDoors();
                            e2MovedSignal.Set();
                        });
                    elevatorAssigned.CloosDoors();
                });

            e1MovedSignal.WaitOne();
            e2MovedSignal.WaitOne();

            building.AddFloorRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, elevatorArrivedAtRequestFloor: (elevatorAssigned) =>
            {
                elevatorAssigned.AddRequest(destinationFloor: 1,
                       servedRequestCallback: (elevatorServedRequest) =>
                       {
                           elevatorServedRequest.CloosDoors();

                           var actions = elevatorServedRequest.GetActions();
                           Assert.True(actions.Count() == 2);
                           Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
                           Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);

                       });
                elevatorAssigned.CloosDoors();
            });

        }

        #endregion 
    }
}
