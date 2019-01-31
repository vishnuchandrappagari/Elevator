using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace ElevatorSolution.Tests
{
    public class ElevatorTests
    {

        private SortedList<int, Floor> GetFloors()
        {
            SortedList<int, Floor> floors = new SortedList<int, Floor> { { 0, new Floor(0) }, { 1, new Floor(1) }, { 2, new Floor(2) }, { 3, new Floor(3) }, { 4, new Floor(4) } };
            return floors;
        }


        /// <summary>
        /// Moving elevator up one floor
        /// </summary>
        [Fact]
        public void GivenElevatorIsGround_WhenRequestIsAddedToMoveToFirstFloor_ThenElevatorIsMovedToFirstFloor()
        {
            //Arrange
            Elevator elevator = new Elevator(GetFloors(), "Elevator 1");
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);

            //Act
            elevator.AddRequest(1, (elevatorReachedDestination) =>
            {

                //Assert
                var actions = elevator.GetActions();
                Assert.Equal(2, actions.Length);
                Assert.Equal(new ElevatorAction(0, 1), actions[0]);
                Assert.Equal(new ElevatorAction(1), actions[1]);

                signalRequestCompletion.Set();
            });

            signalRequestCompletion.WaitOne();
        }


        /// <summary>
        /// Moving down one floor 
        /// </summary>
        [Fact]
        public void GivenElevatorInFirstFloor_WhenRequetIsAddedToMoveToGround_ThenElevatorIsMovedToGroundFloor()
        {
            //Arrange
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);
            Elevator elevator = new Elevator(GetFloors(), "Elevator 1");
            elevator.AddRequest(1, (elevatorReachedDestination) =>
            {

                //Act
                elevator.AddRequest(0, (elevatorGroundFloor) =>
                {

                    //Assert
                    var actions = elevator.GetActions();
                    Assert.Equal(4, actions.Length);
                    Assert.Equal(new ElevatorAction(0, 1), actions[0]);
                    Assert.Equal(new ElevatorAction(1), actions[1]);
                    Assert.Equal(new ElevatorAction(1, 0), actions[2]);
                    Assert.Equal(new ElevatorAction(0), actions[3]);

                    signalRequestCompletion.Set();
                });
                elevator.CloosDoors();
            });
            signalRequestCompletion.WaitOne();
        }


        /// <summary>
        /// When elevator cross floor any destination requests to that floor are served irrespective of request order 
        /// </summary>
        [Fact]
        public void GivenElevatorInGroundFloor_WhenTwoRequestAreAddedSecondIsPassingFloor_ThenElevatorServesSecondRequetFirst()
        {
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);

            //Arrange
            Elevator elevator = new Elevator(GetFloors(), "Elevator 1");


            //Act            
            elevator.AddRequest(1, (elevatorServedFirstFloorRequest) =>
             {
                 elevatorServedFirstFloorRequest.AddRequest(4, (elevatorReachedFourthFloor) => { signalRequestCompletion.Set(); });
                 elevatorServedFirstFloorRequest.AddRequest(2);
                 elevatorServedFirstFloorRequest.CloosDoors();
             });

            signalRequestCompletion.WaitOne();


            //Assert
            var actions = elevator.GetActions();
            //Only four since second floor is served when passing through that floor
            Assert.Equal(7, actions.Length);
            Assert.Equal(new ElevatorAction(0, 1), actions[0]);
            Assert.Equal(new ElevatorAction(1), actions[1]);
            Assert.Equal(new ElevatorAction(1, 2), actions[2]);
            Assert.Equal(new ElevatorAction(2), actions[3]);
            Assert.Equal(new ElevatorAction(2, 3), actions[4]);
            Assert.Equal(new ElevatorAction(3, 4), actions[5]);
            Assert.Equal(new ElevatorAction(4), actions[6]);
        }

        /// <summary>
        /// IF there is floor request in elevator going direction then elevator is stopped in the floor
        /// </summary>
        [Fact]
        public void GivenElevatorIsGoingUp_WhenThereIsFloorUpRequetInTraversingFloor_ThenElevatorIsStopedInTravesingFloor()
        {
            //Arrange
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);
            SortedList<int, Floor> floors = GetFloors();
            Elevator elevator = new Elevator(floors, "Elevator 1");


            //Act
            floors[1].SetMovingUpRequest(Elevator.CloseDoorsAction);
            elevator.AddRequest(2, (elevatorCompletedRequest) =>
            {
                signalRequestCompletion.Set();
            });

            signalRequestCompletion.WaitOne();

            //Assert
            var actions = elevator.GetActions();
            Assert.Equal(4, actions.Length);
            Assert.Equal(new ElevatorAction(0, 1), actions[0]);
            Assert.Equal(new ElevatorAction(1), actions[1]);
            Assert.Equal(new ElevatorAction(1, 2), actions[2]);
            Assert.Equal(new ElevatorAction(2), actions[3]);

            Assert.False(floors[1].HasDownRequest);
        }


        /// <summary>
        /// IF there is floor request in elevator opposite direction to elevator then elevator is not stopped in the floor
        /// </summary>
        [Fact]
        public void GivenElevatorIsGoingUp_WhenThereIsFloorDownRequetInTraversingFloor_ThenElevatorIsNotStopedInTravesingFloor()
        {
            //Arrange
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);
            SortedList<int, Floor> floors = GetFloors();
            Elevator elevator = new Elevator(floors, "Elevator 1");


            //Act
            floors[1].SetMovingDownRequest(Elevator.CloseDoorsAction);
            elevator.AddRequest(2, (elevatorCompletedRequest) =>
            {
                signalRequestCompletion.Set();
            });

            signalRequestCompletion.WaitOne();

            //Assert
            var actions = elevator.GetActions();
            Assert.Equal(3, actions.Length);
            Assert.Equal(new ElevatorAction(0, 1), actions[0]);
            Assert.Equal(new ElevatorAction(1, 2), actions[1]);
            Assert.Equal(new ElevatorAction(2), actions[2]);
        }


        /// <summary>
        /// IF there is floor request in elevator going direction then elevator is stopped in the floor
        /// </summary>
        [Fact]
        public void GivenElevatorIsGoingDown_WhenThereIsFloorDownRequetInTraversingFloor_ThenElevatorIsStopedInTravesingFloor()
        {
            //Arrange
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);
            SortedList<int, Floor> floors = GetFloors();
            Elevator elevator = new Elevator(floors, "Elevator 1");
            elevator.AddRequest(2, (elevatorAtSecondFloor) =>
            {

                //Act
                floors[1].SetMovingDownRequest(Elevator.CloseDoorsAction);
                elevator.AddRequest(0, (elevatorCompletedRequest) =>
                {
                    signalRequestCompletion.Set();
                });
                elevatorAtSecondFloor.CloosDoors();
            });


            signalRequestCompletion.WaitOne();

            //Assert
            var actions = elevator.GetActions();
            Assert.Equal(7, actions.Length);
            Assert.Equal(new ElevatorAction(0, 1), actions[0]);
            Assert.Equal(new ElevatorAction(1, 2), actions[1]);
            Assert.Equal(new ElevatorAction(doorsOpenedFloor: 2), actions[2]);
            Assert.Equal(new ElevatorAction(2, 1), actions[3]);
            Assert.Equal(new ElevatorAction(1), actions[4]);
            Assert.Equal(new ElevatorAction(1, 0), actions[5]);
            Assert.Equal(new ElevatorAction(0), actions[6]);

            Assert.False(floors[1].HasDownRequest);
        }


        [Fact]
        public void GivenElevatorInGroundFloor_WhenRequestIsRaisedToGroundFloor_ThenElevatorOpensDoorInGroundFloor()
        {
            AutoResetEvent signalRequestCompletion = new AutoResetEvent(false);

            SortedList<int, Floor> floors = GetFloors();
            Elevator elevator = new Elevator(floors, "Elevator 1");
            elevator.AddRequest(0, (elevatorServedRequest) =>
            {
                elevatorServedRequest.CloosDoors();
                signalRequestCompletion.Set();
            });

            signalRequestCompletion.WaitOne();

            var actions = elevator.GetActions();

            Assert.Single(actions);
            Assert.Equal(new ElevatorAction(0), actions[0]);
        }

    }
}
