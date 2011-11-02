using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TrafficController
{
    public enum TrafficLighState
    {
        Green = 1,
        Orange = 2,
        Red = 3,
        Left = 4,
        Right = 5,
        Straight = 6,
        Outage = 7,
        Off = 0
    }

    public enum Vehicle
    {
        NONE,
        CAR,
        PEDESTRIAN,
        BUS,
        BICYCLE
    }

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    class Lane
    {


        private string _id;
        private int _laneNr;
        private Direction _direction;
        private Vehicle _vehicle = Vehicle.NONE;

        private int _priority;
        private int _queueCount = 0;
        private TrafficLighState _state;

        private Stopwatch timer;
        private int _timeout;

        private Server _server;

        public string Id { get { return _id; } }
        public TrafficLighState State { get { return _state; } }
        public int QueueCount { get { return _queueCount; } }
        public Vehicle Vehicle { get { return _vehicle; } }
        public int Timeout { get { return _timeout; } }
        public int TimeLeft { get { return _timeout - (int)timer.ElapsedMilliseconds; } }


        public Lane (string id, Server server, Vehicle type)
        {
            _id = id;
            _laneNr = Convert.ToInt32(id.Substring(1,1));
            _direction = (Direction) Enum.Parse(typeof(Direction),
                Enum.GetNames(typeof(Direction)).First((s) => s.StartsWith(id.Substring(0, 1))), true);

            _server = server;
            _vehicle = type;
            _state = TrafficLighState.Red;
        }

        public void SetTafficLight(TrafficLighState state)
        {
            this._state = state;
            string arg = string.Format("{0},{1}",_id, (int)state);
            _server.RPCSendQueue.Enqueue(new RPCData(){type = 1, arg = arg});
        }

        public void IncreaseQueue()
        {
            _queueCount = this.Vehicle == Vehicle.CAR ? _queueCount + 1 : 1;
        }

        public void DecreaseQueue()
        {
            _queueCount = this.Vehicle == Vehicle.CAR ? _queueCount - 1 : 0;
        }

        public void SetTafficLight(TrafficLighState state, int timeout)
        {
            timer = new Stopwatch();
            timer.Start();
            SetTafficLight(state);
            this._timeout = timeout;
        }

        //compatibility matrix for same-lane types
        public bool[,] _compatibility = new[,] { {true,  true,  true,  true,  true,  true,  true,  true },
                                                 {true,  true,  false, false, false, false, false, false},
                                                 {true,  true,  true,  true,  false, false, false, true },
                                                 {true,  true,  true,  true,  true,  false, false, true },
                                                 {true,  true,  false, true,  true,  false, false, true },
                                                 {true,  false, false, false, false, false, false, true },
                                                 {false, false, false, false, false, false, true,  true },
                                                 {true,  false, true,  true,  true,  true,  true,  true } };
        
        public bool IsCompatible(Lane other)
        {
            if (other == this)
                return true;

            if(_direction == other._direction)
                return _compatibility[_laneNr, other._laneNr];

            //todo: add other lane stuff from different directions
            return false;
        }

        public void Update()
        {
            if (timer == null)
                return;
            if (timer.ElapsedMilliseconds > _timeout)
            {
                if (_state == TrafficLighState.Green)
                    SetTafficLight(TrafficLighState.Orange, 6000);
                else
                {
                    if (Vehicle != Vehicle.CAR)
                        DecreaseQueue();
                    SetTafficLight(TrafficLighState.Red);
                    timer.Reset();
                }
            }
        }


    }
}
