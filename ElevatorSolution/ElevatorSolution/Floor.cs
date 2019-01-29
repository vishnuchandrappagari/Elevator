using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElevatorSolution
{
    public class Floor
    {

        public ElevatorCallback _upRequestNotification;
        public ElevatorCallback _downRequestNotification;

        public bool HasUpRequest { get; private set; }

        public bool HasDownRequest { get; private set; }

        public int FloorNumber { get; private set; }


        public Floor(int floorNumber)
        {
            FloorNumber = floorNumber;
        }
      

        public void SetMovingUpRequest(ElevatorCallback callback)
        {
            HasUpRequest = true;
            _upRequestNotification += callback;
        }

        public void ResetMovingUpRequest()
        {
            HasUpRequest = false;
            _upRequestNotification = null;
        }

        public void SetMovingDownRequest(ElevatorCallback callback)
        {
            HasDownRequest = true;
            _downRequestNotification += callback;
        }

        public void ResetMovingDownRequest()
        {
            HasDownRequest = false;
            _downRequestNotification = null;
        }
    }
}
