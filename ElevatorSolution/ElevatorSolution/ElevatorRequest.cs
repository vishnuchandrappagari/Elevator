using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorSolution
{
    /// <summary>
    /// Once person gets in to elevator this is used for specifying destination floor 
    /// </summary>
    public class ElevatorRequest
    {
        public int DestinationFloor { get; private set; }

        public ElevatorCallback DoorsOpenedAtDestinationFloor { get; private set; }

        public ElevatorRequest(int destinationFloor, ElevatorCallback requestServed)
        {            
            this.DestinationFloor = destinationFloor;
            this.DoorsOpenedAtDestinationFloor = requestServed;
        }
    }

}
