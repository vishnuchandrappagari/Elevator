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

            for (int i = minFloor; i <= maxfloor; i++)
                _floors.Add(i, new Floor(i));

            for (int i = 1; i <= numberOfElevators; i++)
                _elevators.Add(new Elevator(_floors, $"Elevator {i}"));


        }


        public void AddFloorRequest(int floorNumber, FloorRequest requestDirection, ElevatorCallback floorRequestAssignedElevator)
        {
            if (requestDirection == FloorRequest.UP)
                _floors[floorNumber].SetMovingUpRequest(floorRequestAssignedElevator);
            else
                _floors[floorNumber].SetMovingDownRequest(floorRequestAssignedElevator);

            Elevator elevatorInRequestFloor = _elevators.FirstOrDefault();

            elevatorInRequestFloor.AddRequest(floorNumber, floorRequestAssignedElevator);
        }
    }
}
