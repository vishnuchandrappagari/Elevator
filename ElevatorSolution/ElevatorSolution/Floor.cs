using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElevatorSolution
{
    public class Floor
    {       
        public Floor()
        {
        
        }

      

        public ElevatorCallback _upRequestNotification;
        public ElevatorCallback _downRequestNotification;

        public bool _hasUpRequest { get; set; }

        public bool _hasDownRequest { get; set; }

        public int FloorNumber { get; set; }


        //public void PassThrough(Elevator elevator)
        //{
        //    if (_hasDownRequest && elevator.Status == ElevatorStatus.GoingDown)
        //    {
        //        elevator.Stop();
        //        _downRequestNotification(elevator);
        //        ResetMovingDownRequest();
        //    }
        //    else if (_hasUpRequest && elevator.Status == ElevatorStatus.GoingUp)
        //    {
        //        elevator.Stop();
        //        _upRequestNotification(elevator);
        //        ResetMovingUpRequest();
        //    }
        //    else if (_hasUpRequest && elevator.Status == ElevatorStatus.Stopped)
        //    {
        //        _upRequestNotification(elevator);
        //        ResetMovingUpRequest();
        //    }
        //    else if (_hasDownRequest && elevator.Status == ElevatorStatus.Stopped)
        //    {
        //        _downRequestNotification(elevator);
        //        ResetMovingDownRequest();
        //    }
        //}

        //public void AssignElevator()
        //{
        //    var assignedElevator= _elevators.Where(elevator => elevator.CurrentPosition == this.FloorNumber).FirstOrDefault();
        //    PassThrough(assignedElevator);

        //    //_elevators.Where(elevator=> elevator.Status==ElevatorStatus.Stopped).OrderBy()


        //}

        public void SetMovingUpRequest(ElevatorCallback callback)
        {
            _hasUpRequest = true;
            _upRequestNotification += callback;
        }

        private void ResetMovingUpRequest()
        {
            _hasUpRequest = false;
            _upRequestNotification = null;
        }

        public void SetMovingDownRequest(ElevatorCallback callback)
        {
            _hasDownRequest = true;
            _downRequestNotification += callback;
        }

        private void ResetMovingDownRequest()
        {
            _hasDownRequest = false;
            _downRequestNotification = null;
        }
    }
}
