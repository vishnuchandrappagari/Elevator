using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorSolution
{
    public enum ElevatorState
    {
        DoorOpened,
        Stopped,                
        GoingUp,
        GoingDown,
        GoingUpDoorOpenedForPickUp,
        GoingDownDoorOpenedForPickUp
    }
}
