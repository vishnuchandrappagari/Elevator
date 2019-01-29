using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorSolution
{
    public class ElevatorRequest
    {
        public int DestinationFloor { get; private set; }

        public ElevatorCallback DoorsOpenedAtDestinationFloor { get; private set; }
        public ElevatorCallback DoorOpened { get; private set; }

        public ElevatorRequest(int destinationFloor, ElevatorCallback requestServed)
        {            
            this.DestinationFloor = destinationFloor;
            this.DoorsOpenedAtDestinationFloor = requestServed;
        }
    }

}
