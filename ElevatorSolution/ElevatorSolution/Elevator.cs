using Stateless;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string _name;

        public ElevatorState State
        {
            get
            {
                return _stateMachine.State;
            }
        }

        public Elevator(SortedList<int, Floor> floors, string name)
        {

            _name = name;
            _stateMachine = new StateMachine<ElevatorState, ElevatorTrigger>(ElevatorState.Stopped, FiringMode.Immediate);

            _stateMachine.Configure(ElevatorState.Stopped)
                .Permit(ElevatorTrigger.OpenDoors, ElevatorState.DoorOpened)
                .OnExit(() =>
                {
                    ElevatorRequest currentFloorDestinationRequest = _requestQueue.FirstOrDefault(request => request.DestinationFloor == _floors[_currentFloor].FloorNumber);

                    if (currentFloorDestinationRequest != null)
                    {
                        _requestQueue.Remove(currentFloorDestinationRequest);
                        Task.Factory.StartNew((callback) => ((ElevatorCallback)(callback))(this), currentFloorDestinationRequest.DoorsOpenedAtDestinationFloor);
                    }
                });


            _stateMachine.Configure(ElevatorState.DoorOpened)
                .Permit(ElevatorTrigger.CloseDoors, ElevatorState.Stopped)
                .OnEntry(() =>
                {
                    //Waiting for doors to close
                    _doorCloseSignaling.WaitOne();
                });

            _stateMachine.Configure(ElevatorState.Stopped)
                .Permit(ElevatorTrigger.GoUp, ElevatorState.GoingUp)
                .Permit(ElevatorTrigger.GoDown, ElevatorState.GoingDown);

            _stateMachine.Configure(ElevatorState.GoingUp)
                .Permit(ElevatorTrigger.Stop, ElevatorState.Stopped)
                .Permit(ElevatorTrigger.ReverseDirection, ElevatorState.GoingDown);


            _stateMachine.Configure(ElevatorState.GoingDown)
                    .Permit(ElevatorTrigger.Stop, ElevatorState.Stopped)
                    .Permit(ElevatorTrigger.ReverseDirection, ElevatorState.GoingUp);



            _stateMachine.OnTransitioned(OnTransitionedAction);


            //Placing new elevator on lowest floor
            _currentFloor = floors.First().Key;

            _floors = floors;

            //Creating elevator dedicated thread
            Thread Thread = new Thread(new ThreadStart(ProcessRequests));
            Thread.Start();
        }

        void OnTransitionedAction(StateMachine<ElevatorState, ElevatorTrigger>.Transition transition)
        {
            ElevatorTrigger trigger = transition.Trigger;
            ElevatorState source = transition.Source;
            ElevatorState dest = transition.Destination;

            Debug.WriteLine($"Elevator:{_name}, Floor:{_currentFloor}, Trigger: {trigger}, {source}-->{dest}");
        }




        public void ProcessRequests()
        {
            while (true)
            {
                if (_requestQueue.Count == 0)
                {
                    _newRequestSignaling.Reset();
                    _newRequestSignaling.WaitOne();
                }

                Floor currentFloor = _floors[_currentFloor];

                if (_stateMachine.State == ElevatorState.Stopped)
                {
                    if (currentFloor.HasUpRequest)
                    {
                        _stateMachine.Fire(ElevatorTrigger.OpenDoors);

                        currentFloor.ResetMovingUpRequest();

                        _stateMachine.Fire(ElevatorTrigger.GoUp);

                        MoveToNextFloor();
                    }
                    else if (currentFloor.HasDownRequest)
                    {
                        _stateMachine.Fire(ElevatorTrigger.OpenDoors);
                        currentFloor.ResetMovingDownRequest();
                        _stateMachine.Fire(ElevatorTrigger.GoDown);
                        MoveToNextFloor();
                    }
                    else if (_requestQueue.Any(request => request.DestinationFloor == _currentFloor))
                    {
                        _stateMachine.Fire(ElevatorTrigger.OpenDoors);
                    }
                    else
                    {
                        //Determine the direction based on request 
                        ElevatorRequest elevatorRequest = _requestQueue.Peek();
                        if (elevatorRequest.DestinationFloor > currentFloor.FloorNumber)
                        {
                            _stateMachine.Fire(ElevatorTrigger.GoUp);
                        }
                        else if (elevatorRequest.DestinationFloor < currentFloor.FloorNumber)
                        {
                            _stateMachine.Fire(ElevatorTrigger.GoDown);
                        }
                    }
                }
                else if (_stateMachine.State == ElevatorState.GoingUp || _stateMachine.State == ElevatorState.GoingDown)
                {
                    if (_requestQueue.Any(request => request.DestinationFloor == _currentFloor))
                    {
                        _stateMachine.Fire(ElevatorTrigger.Stop);
                    }
                    else
                    {
                        MoveToNextFloor();
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
            Thread.Sleep(200);
            _stateMachine.Fire(ElevatorTrigger.CloseDoors);
            _doorCloseSignaling.Set();
        }

        public ElevatorAction[] GetActions()
        {
            return actions.ToArray();
        }

        private void AddAction(int fromFloor, int toFloor)
        {
            actions.Add(new ElevatorAction(fromFloor, toFloor));
        }

        private void MoveToNextFloor()
        {

            if (ElevatorState.GoingUp == State)
            {
                if (_floors.IndexOfKey(_currentFloor + 1) == -1)
                {
                    _stateMachine.Fire(ElevatorTrigger.ReverseDirection);
                }
                else
                {
                    AddAction(_currentFloor, _currentFloor + 1);
                    _currentFloor++;
                }
            }
            else if (ElevatorState.GoingDown == State)
            {
                if (_floors.IndexOfKey(_currentFloor - 1) == -1)
                {
                    _stateMachine.Fire(ElevatorTrigger.ReverseDirection);
                }
                else
                {
                    AddAction(_currentFloor, _currentFloor - 1);
                    _currentFloor--;
                }
            }
        }

    }
}
