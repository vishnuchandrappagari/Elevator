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
        #region Private variables

        private AutoResetEvent _doorCloseSignaling = new AutoResetEvent(false);
        private AutoResetEvent _newRequestSignaling = new AutoResetEvent(false);
        private StateMachine<ElevatorState, ElevatorTrigger> _stateMachine;

        //TODO: Can be replaced with concurrent queue
        private Queue<ElevatorRequest> _requestQueue = new Queue<ElevatorRequest>();
        private IList<ElevatorAction> actions = new List<ElevatorAction>();
        private SortedList<int, Floor> _floors;

        #endregion

        #region Public Properties

        public static ElevatorCallback CloseDoorsAction => (elevator) => elevator.CloosDoors();

        public virtual int CurrentFloor { get; private set; }
        public virtual string Name { get; private set; }

        public virtual ElevatorState State
        {
            get
            {
                return _stateMachine.State;
            }
        }

        #endregion

        public Elevator(SortedList<int, Floor> floors, string name)
        {

            Name = name;
            _floors = floors;

            InitilizeStateMachine();
            //Placing new elevator on lowest floor
            CurrentFloor = floors.First().Key;

            //Creating elevator dedicated thread
            Thread Thread = new Thread(new ThreadStart(ProcessRequests));
            Thread.IsBackground = true; 
            Thread.Start();
        }

        #region Private methods

        private void InitilizeStateMachine()
        {
            _stateMachine = new StateMachine<ElevatorState, ElevatorTrigger>(ElevatorState.Stopped, FiringMode.Immediate);

            _stateMachine.Configure(ElevatorState.Stopped)
                .Permit(ElevatorTrigger.OpenDoors, ElevatorState.DoorOpened)
                 .Permit(ElevatorTrigger.GoUp, ElevatorState.GoingUp)
                .Permit(ElevatorTrigger.GoDown, ElevatorState.GoingDown);



            _stateMachine.Configure(ElevatorState.DoorOpened)
                .Permit(ElevatorTrigger.CloseDoors, ElevatorState.Stopped)
                .OnEntry(() =>
                {
                    actions.Add(new ElevatorAction(CurrentFloor));
                    //Waiting for doors to close
                    _doorCloseSignaling.WaitOne();
                });


            _stateMachine.Configure(ElevatorState.GoingUp)
                .Permit(ElevatorTrigger.Stop, ElevatorState.Stopped)
                .Permit(ElevatorTrigger.ReverseDirection, ElevatorState.GoingDown);


            _stateMachine.Configure(ElevatorState.GoingDown)
                    .Permit(ElevatorTrigger.Stop, ElevatorState.Stopped)
                    .Permit(ElevatorTrigger.ReverseDirection, ElevatorState.GoingUp);

            _stateMachine.OnTransitioned(OnTransitionedAction);
        }

        private void OnTransitionedAction(StateMachine<ElevatorState, ElevatorTrigger>.Transition transition)
        {
            ElevatorTrigger trigger = transition.Trigger;
            ElevatorState source = transition.Source;
            ElevatorState dest = transition.Destination;

            Debug.WriteLine($"Elevator:{Name}, Floor:{CurrentFloor}, Trigger: {trigger}, {source}-->{dest}");
        }

        private void ProcessRequests()
        {
            while (true)
            {
                //This loop repeats for every one floor movement 

                if (_requestQueue.Count == 0)
                {
                    //Wait when no request until signaled
                    _newRequestSignaling.Reset();
                    _newRequestSignaling.WaitOne();
                }


                if (ElevatorState.Stopped == State)
                {
                    ElevatorRequest elevatorRequest = _requestQueue.Peek();
                    if (elevatorRequest.DestinationFloor > CurrentFloor)
                    {
                        _stateMachine.Fire(ElevatorTrigger.GoUp);
                    }
                    else if (elevatorRequest.DestinationFloor < CurrentFloor)
                    {
                        _stateMachine.Fire(ElevatorTrigger.GoDown);
                    }
                    else if (elevatorRequest.DestinationFloor == CurrentFloor)
                    {

                        OpenDoors();
                    }
                }

                MoveToNextFloor();

            }
        }

        private void EnteredNewFloor()
        {

            //If current floor is destination for any request in queue
            if (_requestQueue.Any(request => request.DestinationFloor == CurrentFloor))
            {
                _stateMachine.Fire(ElevatorTrigger.Stop);
                OpenDoors();
            }

            Floor currentFloor = _floors[CurrentFloor];

            lock (currentFloor)
            {
                if (currentFloor.HasUpRequest && ElevatorState.GoingUp == State)
                {
                    _stateMachine.Fire(ElevatorTrigger.Stop);
                    Task.Factory.StartNew((callback) => ((ElevatorCallback)(callback))(this), currentFloor.UpRequestNotification ?? CloseDoorsAction);
                    currentFloor.ResetMovingUpRequest();
                    _stateMachine.Fire(ElevatorTrigger.OpenDoors);


                }
                else if (currentFloor.HasDownRequest && ElevatorState.GoingDown == State)
                {
                    _stateMachine.Fire(ElevatorTrigger.Stop);
                    Task.Factory.StartNew((callback) => ((ElevatorCallback)(callback))(this), currentFloor.DownRequestNotification ?? CloseDoorsAction);
                    currentFloor.ResetMovingDownRequest();
                    _stateMachine.Fire(ElevatorTrigger.OpenDoors);
                }
            }

        }

        private void MoveToNextFloor()
        {

            if (ElevatorState.GoingUp == State)
            {
                if (_floors.IndexOfKey(CurrentFloor + 1) == -1)
                {
                    _stateMachine.Fire(ElevatorTrigger.ReverseDirection);
                }
                else
                {
                    AddAction(CurrentFloor, CurrentFloor + 1);
                    CurrentFloor++;
                    EnteredNewFloor();
                }
            }
            else if (ElevatorState.GoingDown == State)
            {
                if (_floors.IndexOfKey(CurrentFloor - 1) == -1)
                {
                    _stateMachine.Fire(ElevatorTrigger.ReverseDirection);
                }
                else
                {
                    AddAction(CurrentFloor, CurrentFloor - 1);
                    CurrentFloor--;
                    EnteredNewFloor();
                }
            }
        }

        private void AddAction(int fromFloor, int toFloor)
        {
            actions.Add(new ElevatorAction(fromFloor, toFloor));
        }

        private void OpenDoors()
        {
            lock (_requestQueue)
            {
                ElevatorRequest currentFloorDestinationRequest = _requestQueue.FirstOrDefault(request => request.DestinationFloor == _floors[CurrentFloor].FloorNumber);

                if (currentFloorDestinationRequest != null)
                {
                    _requestQueue.Remove(currentFloorDestinationRequest);
                    Task.Factory.StartNew((callback) => ((ElevatorCallback)(callback))(this), currentFloorDestinationRequest.DoorsOpenedAtDestinationFloor);
                }
            }

            _stateMachine.Fire(ElevatorTrigger.OpenDoors);
        }

        #endregion

        #region Public Methods

        public void AddRequest(int destinationFloor, ElevatorCallback servedRequestCallback)
        {
            lock (_requestQueue)
            {
                _requestQueue.Enqueue(new ElevatorRequest(destinationFloor, servedRequestCallback));
            }
            _newRequestSignaling.Set();
        }



        public void AddRequest(int destinationFloor)
        {
            //By default closing the doors
            AddRequest(destinationFloor, CloseDoorsAction);
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

        #endregion
    }
}
