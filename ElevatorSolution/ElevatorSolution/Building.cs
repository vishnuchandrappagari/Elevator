using System;
using System.Collections.Generic;
using System.Linq;
using static ElevatorSolution.Floor;

namespace ElevatorSolution
{

    public delegate void ElevatorCallback(Elevator elevator);
    public class Building
    {
        private SortedList<int, Floor> _floors;
        private List<Elevator> _elevators;

        public Building(int minFloor, int maxfloor, int numberOfElevators)
        {
            _elevators = new List<Elevator>();

            _floors = new SortedList<int, Floor>();

            for (int i = minFloor; i < maxfloor; i++)
                _floors.Add(i, new Floor());

            for (int i = 0; i < numberOfElevators; i++)
                _elevators.Add(new Elevator(_floors));


        }


        //public void MoveUpRequest(int floor,int destinationFloor)
        //{
        //    Floor groundFloor = GetFloor(0);
        //    groundFloor.SetMovingUpRequest((elevator) => {

        //        elevator.AddRequest(floor: 1);
        //        var actions = elevator.GetActions();

        //    });
        //}


        public void AddFloorRequest(int floorNumber, FloorRequest requestDirection, ElevatorCallback elevatorArrivedAtFloorcallback)
        {
            if (requestDirection == FloorRequest.UP)
                _floors[floorNumber].SetMovingUpRequest(elevatorArrivedAtFloorcallback);

            Elevator elevatorInRequestFloor = _elevators.FirstOrDefault();

            elevatorArrivedAtFloorcallback(elevatorInRequestFloor);
        }
    }
}
