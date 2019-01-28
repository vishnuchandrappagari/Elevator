using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorSolution
{
    public class ElevatorRequest
    {
        public int DestinationFloor { get; private set; }

        public ElevatorCallback ElevatorCallback { get; private set; }

        public ElevatorRequest(int destinationFloor, ElevatorCallback elevatorCallback)
        {
            this.DestinationFloor = destinationFloor;
            this.ElevatorCallback = elevatorCallback;
        }
    }

}
