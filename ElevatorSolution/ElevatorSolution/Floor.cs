using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElevatorSolution
{
    public class Floor
    {

        public ElevatorCallback UpRequestNotification;
        public ElevatorCallback DownRequestNotification;

        public bool HasUpRequest { get; private set; }

        public bool HasDownRequest { get; private set; }

        public int FloorNumber { get; private set; }


        public Floor(int floorNumber)
        {
            FloorNumber = floorNumber;
        }


        public void SetMovingUpRequest(ElevatorCallback callback = null)
        {
            HasUpRequest = true;
            UpRequestNotification += callback;
        }

        public void ResetMovingUpRequest()
        {
            HasUpRequest = false;
            UpRequestNotification = null;
        }

        public void SetMovingDownRequest(ElevatorCallback callback = null)
        {
            HasDownRequest = true;
            DownRequestNotification += callback;
        }

        public void ResetMovingDownRequest()
        {
            HasDownRequest = false;
            DownRequestNotification = null;
        }
    }
}
