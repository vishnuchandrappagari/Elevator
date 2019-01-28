using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ElevatorSolution.Floor;

namespace ElevatorSolution
{
    public class Elevator
    {
        private AutoResetEvent _doorCloseSignaling = new AutoResetEvent(false);
        private AutoResetEvent _newRequestSignaling = new AutoResetEvent(false);

        private StateMachine<ElevatorState, ElevatorTrigger> _stateMachine;
        private Queue<ElevatorRequest> _requestQueue = new Queue<ElevatorRequest>();
        private IList<ElevatorAction> actions = new List<ElevatorAction>();
        private SortedList<int, Floor> _floors;
        private int _currentFloor;


        public string Name { get; set; }

        public Elevator(SortedList<int, Floor> floors)
        {
            _stateMachine = new StateMachine<ElevatorState, ElevatorTrigger>(ElevatorState.Stopped, FiringMode.Immediate);

            _stateMachine.Configure(ElevatorState.Stopped)
                .Permit(ElevatorTrigger.OpenDoors, ElevatorState.DoorOpened)
                .OnExit(() =>
                {
                    ElevatorRequest elevatorRequest = _requestQueue.FirstOrDefault(request => request.DestinationFloor == _floors[_currentFloor].FloorNumber);

                    if (elevatorRequest != null)
                    {
                        _requestQueue.Remove(elevatorRequest);
                        Task.Factory.StartNew((callback) => ((ElevatorCallback)(callback))(this), elevatorRequest.ElevatorCallback);
                    }
                }
                );


            _stateMachine.Configure(ElevatorState.DoorOpened)
                .Permit(ElevatorTrigger.CloseDoors, ElevatorState.Stopped);

            _stateMachine.Configure(ElevatorState.Stopped)
                .Permit(ElevatorTrigger.MoveUp, ElevatorState.GoingUp)
                .Permit(ElevatorTrigger.MoveDown, ElevatorState.GoingDown);

            _stateMachine.Configure(ElevatorState.GoingUp)
                .Permit(ElevatorTrigger.Stop, ElevatorState.Stopped);


            _stateMachine.Configure(ElevatorState.GoingDown)
                    .Permit(ElevatorTrigger.Stop, ElevatorState.Stopped);

            //Placing new elevator on lowest floor
            _currentFloor = floors.First().Key;

            _floors = floors;

            //Creating elevator dedicated thread
            Thread Thread = new Thread(new ThreadStart(ProcessRequests));
            Thread.Start();
        }





        public void ProcessRequests()
        {
            while (true)
            {

                if (_requestQueue.Count == 0)
                    _newRequestSignaling.WaitOne();

                Floor currentFloor = _floors[_currentFloor];

                if (_stateMachine.State == ElevatorState.Stopped)
                {
                    if (currentFloor.HasUpRequest)
                    {
                        _stateMachine.Fire(ElevatorTrigger.OpenDoors);
                        //Waiting for doors to close
                        _doorCloseSignaling.WaitOne();

                        currentFloor.ResetMovingUpRequest();

                        _stateMachine.Fire(ElevatorTrigger.MoveUp);

                        AddAction(_currentFloor, _currentFloor + 1);
                        _currentFloor++;
                    }
                    else if (currentFloor.HasDownRequest)
                    {
                        currentFloor._downRequestNotification(this);
                    }
                    else
                    {
                        //Determine the direction based on request 
                        ElevatorRequest elevatorRequest = _requestQueue.Peek();
                        if (elevatorRequest.DestinationFloor > currentFloor.FloorNumber)
                        {
                            _stateMachine.Fire(ElevatorTrigger.MoveUp);
                        }
                    }
                }
                else if (_stateMachine.State == ElevatorState.GoingUp)
                {
                    if (_requestQueue.Any(request => request.DestinationFloor == _currentFloor))
                    {
                        _stateMachine.Fire(ElevatorTrigger.Stop);

                        _stateMachine.Fire(ElevatorTrigger.OpenDoors);

                        //Waiting for doors to close
                        _doorCloseSignaling.WaitOne();
                    }
                }

            }
        }


        public void AddRequest(int destinationFloor, ElevatorCallback servedRequestCallback)
        {
            _requestQueue.Enqueue(new ElevatorRequest(destinationFloor, servedRequestCallback));
            _newRequestSignaling.Set();
        }

        public void CloosDoors()
        {
            _stateMachine.Fire(ElevatorTrigger.CloseDoors);
            _doorCloseSignaling.Set();
        }

        public IEnumerable<ElevatorAction> GetActions()
        {
            return actions;
        }

        private void AddAction(int fromFloor, int toFloor)
        {
            actions.Add(new ElevatorAction(fromFloor, toFloor));
        }
    }
}
