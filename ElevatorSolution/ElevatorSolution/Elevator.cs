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
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private StateMachine<ElevatorState, ElevatorTrigger> _state = new StateMachine<ElevatorState, ElevatorTrigger>(ElevatorState.Stopped);
        private SortedList<int, ElevatorCallback> _stops = new SortedList<int, ElevatorCallback>();
        private IList<ElevatorAction> actions = new List<ElevatorAction>();
        private SortedList<int, Floor> _floors;
        private int _currentFloor;
        private ElevatorCallback _doorOpenedCallback;

        public string Name { get; set; }

        public Elevator(SortedList<int, Floor> floors)
        {
            _state = ConfigureStateMachine();
            _currentFloor = floors.First().Key;

            _floors = floors;

            //Creating elevator dedicated thread
            Thread Thread = new Thread(new ThreadStart(ProcessRequests));
            Thread.Start();
        }

        private StateMachine<ElevatorState, ElevatorTrigger> ConfigureStateMachine()
        {
            _state.Configure(ElevatorState.Stopped)
                .PermitIf(ElevatorTrigger.OpenDoors, ElevatorState.DoorOpened)
                .OnEntry(() =>
                {
                    Task.Factory.StartNew(() => _doorOpenedCallback(this));

                    //Waiting for doors to close
                    _autoResetEvent.WaitOne();
                });

            _state.Configure(ElevatorState.DoorOpened)
                .PermitIf(ElevatorTrigger.CloosDoors, ElevatorState.Stopped);


            _state.Configure(ElevatorState.Stopped)
                .PermitIf(ElevatorTrigger.MoveUp, ElevatorState.GoingUp)
                .PermitIf(ElevatorTrigger.MoveDown, ElevatorState.GoingDown);

            _state.Configure(ElevatorState.GoingUp)
                .PermitIf(ElevatorTrigger.PickUp, ElevatorState.GoingUpDoorOpenedForPickUp)
                .PermitIf(ElevatorTrigger.Stop, ElevatorState.Stopped);

            _state.Configure(ElevatorState.GoingDown)
                .PermitIf(ElevatorTrigger.PickUp, ElevatorState.GoingDownDoorOpenedForPickUp)
                .PermitIf(ElevatorTrigger.Stop, ElevatorState.Stopped);

            _state.Configure(ElevatorState.GoingUpDoorOpenedForPickUp)
                .PermitIf(ElevatorTrigger.CloosDoors, ElevatorState.GoingUp)
                .OnExit(() => _autoResetEvent.Set());

            _state.Configure(ElevatorState.GoingDownDoorOpenedForPickUp)
                .PermitIf(ElevatorTrigger.CloosDoors, ElevatorState.GoingDown)
                .OnExit(() => _autoResetEvent.Set());

            return _state;
        }




        public void ProcessRequests()
        {
            //int nextFloor;

            //if (_state.State==State.GoingUp)
            //{
            //     nextFloor = _currentFloor++;
            //    AddAction(_currentFloor, nextFloor);
            //    _currentFloor = nextFloor;
            //}

            //Initial wait 
            _autoResetEvent.WaitOne();

            Floor currentFloor = _floors[_currentFloor];

            if (_state.State == ElevatorState.Stopped)
            {
                if (currentFloor._hasUpRequest)
                {
                    currentFloor._upRequestNotification(this);

                    _state.Fire(ElevatorTrigger.OpenDoors);

                    MoveUp();

                }
                else if (currentFloor._hasDownRequest)
                    currentFloor._downRequestNotification(this);

            }
        }


        public void MoveUp()
        {
            AddAction(_currentFloor, _currentFloor + 1);
            _currentFloor++;

            //If any drop request in new floor
            if (_stops.ContainsKey(_currentFloor))
            {
                _stops[_currentFloor](this);

            }
        }

        private bool hasRequests => _stops.Count() > 1;

        public void AddRequest(int destinationFloorNumber, ElevatorCallback doorsOpenedCallBack, ElevatorCallback servedRequestCallback)
        {
            _doorOpenedCallback += doorsOpenedCallBack;
            if (!_stops.ContainsKey(destinationFloorNumber))
                _stops.Add(destinationFloorNumber, servedRequestCallback);
            //else
            //    _stops[destinationFloorNumber] += callback;

            _autoResetEvent.Set();
        }

        public void CloosDoors()
        {
            _state.Fire(ElevatorTrigger.CloosDoors);
        }

        public void NotifyNewRequest()
        {
            _autoResetEvent.Set();
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
