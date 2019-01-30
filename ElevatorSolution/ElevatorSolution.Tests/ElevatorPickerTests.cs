using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Collections;

namespace ElevatorSolution.Tests
{
    public class ElevatorPickerTests
    {

        /// <summary>
        /// Current floor elevator is returned
        /// </summary>
        [Theory]
        [InlineData(1, "Elevator 1")] //Same floor
        [InlineData(0, "Elevator 1")] //Immediate top floor
        [InlineData(3, "Elevator 2")] //Immediate down floor
        public void GivenTwoElevatorsThreeFloorsOneInFirstFloorOneInSecond_WhenRequestIsRaised_ThenNearestOneIsReturned(int floor, string expectedElevator)
        {
            //Arrange
            var floors = new SortedList<int, Floor>() { { 1, new Floor(1) } };

            var elevatorOne = new Mock<Elevator>(floors, "Elevator 1");
            elevatorOne.Setup(elevator => elevator.Name).Returns(() => "Elevator 1");
            elevatorOne.Setup(elevator => elevator.CurrentFloor).Returns(() => 1);
            elevatorOne.Setup(elevator => elevator.State).Returns(() => ElevatorState.Stopped);

            var elevatorTwo = new Mock<Elevator>(floors, "Elevator 2");
            elevatorTwo.Setup(elevator => elevator.Name).Returns(() => "Elevator 2");
            elevatorTwo.Setup(elevator => elevator.CurrentFloor).Returns(() => 2);
            elevatorTwo.Setup(elevator => elevator.State).Returns(() => ElevatorState.Stopped);

            List<Elevator> elevators = new List<Elevator>();
            elevators.Add(elevatorOne.Object);
            elevators.Add(elevatorTwo.Object);

            //Act
            Elevator elevatorReturned = ElevatorPicker.GetSutableElevator(minFloor: 0, maxfloor: 3, floorNumber: floor, requestDirection: FloorRequestDirection.UP, elevatorsList: elevators);


            //Assert
            Assert.Equal(expectedElevator, elevatorReturned.Name);
        }


        /// <summary>
        /// Nearest and preferred state elevator is selected
        /// </summary>
        [Theory]
        [InlineData(3, FloorRequestDirection.Down, 7, 4, -2, "Elevator 2")]
        [InlineData(3, FloorRequestDirection.UP, 7, 4, -2, "Elevator 3")]
        public void GivenMultipleElevatorsInSameFloor_WhenRequestIsRaised_ThenNearestAndPrefrredStateElevatorIsSelected(int floor, FloorRequestDirection floorRequestDirection, int goingUpElevatorFloor, int goingDownElevatorFloor, int stoppedElevatorFloor, string expectedElevator)
        {
            //Arrange
            var floors = new SortedList<int, Floor>() { { 1, new Floor(1) } };

            var elevatorOne = new Mock<Elevator>(floors, "");
            elevatorOne.Setup(elevator => elevator.Name).Returns(() => "Elevator 1");
            elevatorOne.Setup(elevator => elevator.CurrentFloor).Returns(() => goingUpElevatorFloor);
            elevatorOne.Setup(elevator => elevator.State).Returns(() => ElevatorState.GoingUp);

            var elevatorTwo = new Mock<Elevator>(floors, "");
            elevatorTwo.Setup(elevator => elevator.Name).Returns(() => "Elevator 2");
            elevatorTwo.Setup(elevator => elevator.CurrentFloor).Returns(() => goingDownElevatorFloor);
            elevatorTwo.Setup(elevator => elevator.State).Returns(() => ElevatorState.GoingDown);

            var elevatorThree = new Mock<Elevator>(floors, "");
            elevatorThree.Setup(elevator => elevator.Name).Returns(() => "Elevator 3");
            elevatorThree.Setup(elevator => elevator.CurrentFloor).Returns(() => stoppedElevatorFloor);
            elevatorThree.Setup(elevator => elevator.State).Returns(() => ElevatorState.Stopped);

            List<Elevator> elevators = new List<Elevator>();
            elevators.Add(elevatorOne.Object);
            elevators.Add(elevatorTwo.Object);
            elevators.Add(elevatorThree.Object);


            //Act
            Elevator elevatorReturned = ElevatorPicker.GetSutableElevator(minFloor: -2, maxfloor: 10, floorNumber: floor, requestDirection: floorRequestDirection, elevatorsList: elevators);


            //Assert
            Assert.Equal(expectedElevator, elevatorReturned.Name);
        }

    }

}
