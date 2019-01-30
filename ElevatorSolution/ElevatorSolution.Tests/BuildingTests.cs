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
        /// Scenario: Elevator is in requested floor and move up one floor
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
        /// Scenario: Elevator is in requested floor and move up two floor
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
            Elevator elevatorReachedFirstFloor = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            //Act
            Elevator elevatorServedRequest = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            var actions = elevatorServedRequest.GetActions().ToArray();

            //Assert
            Assert.True(actions.Count() == 3);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
            Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[2]);
        }

        /// <summary>
        /// Scenario: Elevator is in requested floor and move down one floor
        /// Elevators - 1
        /// Floors - 1
        /// E1 in first floor
        /// Request : Ground floor -> First Floor 
        /// Expected : E1 :Ground floor -> First Floor 
        [Fact]
        public void GivenOneFloorAndOneElevator_WhenElevatorIsInFirstFloorAndDownRequestFollowedByGroundFloorRequestFromFirstFloor_ThenElevatorIsMovedFromFirstToGround()
        {
            //Arrange
            AutoResetEvent elevatorActionCompletedSignal = new AutoResetEvent(false);
            Building building = new Building(minFloor: 0, maxfloor: 1, numberOfElevators: 1);
            building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            //Act
            Elevator elevatorReachedFirstFloor = building.AddRequest(floorNumber: 1, requestDirection: FloorRequestDirection.Down, destinationFloor: 0);

            var actions = elevatorReachedFirstFloor.GetActions().ToArray();

            //Assert
            Assert.True(actions.Count() == 2);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
            Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);
        }

        #endregion

        #region TwoElevators


        /// <summary>
        ///  Scenario: Nearest elevator serves the request 
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

            //Moving E1 to first floor
            Elevator elevatorOne = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            //Moving E2 to first floor
            Elevator elevatorTwo = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            Elevator elevatorServedRequest = building.AddRequest(floorNumber: 0, requestDirection: FloorRequestDirection.UP, destinationFloor: 1);

            Assert.Equal(elevatorOne.Name, elevatorServedRequest.Name);
            var actions = elevatorServedRequest.GetActions();
            Assert.True(actions.Count() == 2);
            Assert.Equal(new ElevatorAction(fromFloor: 0, toFloor: 1), actions[0]);
            Assert.Equal(new ElevatorAction(fromFloor: 1, toFloor: 0), actions[1]);
        }

        #endregion
    }
}
