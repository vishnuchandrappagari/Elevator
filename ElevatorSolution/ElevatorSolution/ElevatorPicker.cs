using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElevatorSolution
{

    /// <summary>
    /// Picker the suitable elevator for floor request
    /// </summary>
    public class ElevatorPicker
    {
        public static Elevator GetSutableElevator(int minFloor, int maxfloor, int floorNumber, FloorRequestDirection requestDirection, List<Elevator> elevatorsList)
        {
            Elevator elevatorSelected = null;
            int counter = 0;
            while (floorNumber + counter <= maxfloor || floorNumber - counter >= minFloor)
            {
                if (floorNumber + counter <= maxfloor)
                    elevatorSelected = GetElevatorInFloor(floorNumber, floorNumber + counter, requestDirection, elevatorsList);

                if (elevatorSelected == null)
                {
                    if (floorNumber - counter >= minFloor)
                        elevatorSelected = GetElevatorInFloor(floorNumber, floorNumber - counter, requestDirection, elevatorsList);
                }

                if (elevatorSelected != null)
                    return elevatorSelected;

                counter++;
            }

            return elevatorSelected;
        }


        private static Elevator GetElevatorInFloor(int actualFloor, int currentSearchingFloor, FloorRequestDirection requestDirection, List<Elevator> elevatorsList)
        {
            ElevatorState preferredElevatorState = FloorRequestDirection.Down == requestDirection ? ElevatorState.GoingDown : ElevatorState.GoingUp;

            Elevator elevatorSelected = null;

            //Checking if any elevator moving towards requested floor and in requested direction
            if ((requestDirection == FloorRequestDirection.UP && currentSearchingFloor <= actualFloor)||(requestDirection == FloorRequestDirection.Down && currentSearchingFloor >= actualFloor))
                elevatorSelected = elevatorsList.FirstOrDefault(elevator => (elevator.CurrentFloor == currentSearchingFloor && elevator.State == preferredElevatorState));

            //If no elevator moving in request direction then checking if any elevator in stopped current floor
            if (null == elevatorSelected)
            {
                elevatorSelected = elevatorsList.FirstOrDefault(elevator => (elevator.CurrentFloor == currentSearchingFloor && elevator.State == ElevatorState.Stopped));
            }

            return elevatorSelected;
        }
    }
}
