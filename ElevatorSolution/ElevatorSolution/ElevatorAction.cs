using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorSolution
{
    public class ElevatorAction
    {
        private string _description;
        public ElevatorAction(int fromFloor, int toFloor)
        {
            _description = $"Moved from {fromFloor} to {toFloor}";
        }

        public ElevatorAction(int doorsOpenedFloor)
        {
            _description = $"DoorsOpened in floor {doorsOpenedFloor}";
        }

        public override string ToString()
        {
            return _description;
        }

        public override bool Equals(object obj)
        {
            return _description.Equals(obj.ToString());
        }
    }
}
