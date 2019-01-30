using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        //TODO: This is blocking request currently this can be async
        public Elevator AddRequest(int floorNumber, FloorRequestDirection requestDirection, int destinationFloor)
        {
            return AddRequest(floorNumber, requestDirection, destinationFloor, elvatorArrivedAtFloorCallback:(elevator) => elevator.CloosDoors(), servedRequestCallback:(elevator) => elevator.CloosDoors());
        }



        public Elevator AddRequest(int floorNumber, FloorRequestDirection requestDirection, int destinationFloor, ElevatorCallback elvatorArrivedAtFloorCallback, ElevatorCallback servedRequestCallback)
        {
            Elevator elevatorServedRequest = null;
            AutoResetEvent waitforCompletionSignal = new AutoResetEvent(false);

            AddFloorRequest(floorNumber, requestDirection,
                elevatorArrivedAtRequestFloor: (elevatorArrivedAtFloor) =>
                {
                    elevatorArrivedAtFloor.AddRequest(destinationFloor: destinationFloor,
                        servedRequestCallback: (elevator) =>
                        {
                            elevatorServedRequest = elevator;
                            Debug.WriteLine("Closing door after serving request");
                            servedRequestCallback(elevator);
                            waitforCompletionSignal.Set();
                        });

                    Debug.WriteLine("Closing door at arrived floor");

                    elvatorArrivedAtFloorCallback(elevatorArrivedAtFloor);
                });


            waitforCompletionSignal.WaitOne();
            return elevatorServedRequest;
        }


        public void AddFloorRequest(int floorNumber, FloorRequestDirection requestDirection, ElevatorCallback elevatorArrivedAtRequestFloor)
        {
            if (requestDirection == FloorRequestDirection.UP)
                _floors[floorNumber].SetMovingUpRequest(elevatorArrivedAtRequestFloor);
            else
                _floors[floorNumber].SetMovingDownRequest(elevatorArrivedAtRequestFloor);

            Elevator elevatorInRequestFloor = _elevators.FirstOrDefault();

            elevatorInRequestFloor.AddRequest(floorNumber, elevatorArrivedAtRequestFloor);
        }


        private Elevator PickSutableElevator(int floorNumber, FloorRequestDirection floorRequest, SortedList<int, Elevator> elevatorsList)
        {
            return null;
        }
    }
}
