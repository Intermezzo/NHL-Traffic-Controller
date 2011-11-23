﻿using System;
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

    public enum WindDirection
    {
        North,
        East,
        South,
        West
    }

    public enum Direction
    {
        Self = 0,
        Left = 1,
        Right = 3,
        Forward = 2
    }

    class Lane
    {


        private string _id;
        private int _laneNr;
        private WindDirection _direction;
        
        private Vehicle _vehicle = Vehicle.NONE;

        private int _priority;
        private int _queueCount = 0;
        private TrafficLighState _state;

        private Stopwatch timer;
        private Stopwatch waitTimer = new Stopwatch();
        private int _timeout;

        private Server _server;


        public Direction BusDirection { get; set; }
        public string Id { get { return _id; } }
        public TrafficLighState State { get { return _state; } }
        public int QueueCount { get { return _queueCount; } }
        public Vehicle Vehicle { get { return _vehicle; } }
        public int Timeout { get { return _timeout; } }
        public int TimeLeft { get { return _timeout - (int)timer.ElapsedMilliseconds; } }
        public int TimeElapsed { get { return (int)timer.ElapsedMilliseconds; } }
        public int CompatLane
        {
            get 
            {
                int initialLane = _laneNr;
                if (_vehicle == Vehicle.BICYCLE)
                    return initialLane + 2;

                if (_vehicle == Vehicle.BUS)
                {
                    switch (BusDirection)
                    {
                        case Direction.Forward:
                            return initialLane + 1;
                        case Direction.Right:
                            return initialLane + 2;
                        default:
                            return initialLane;
                    }
                }
                return initialLane;
            }

        }

        public int Priority 
        { 
            get 
            { return _priority + Math.Max(QueueCount, 30) + 
                Math.Max(0, Math.Min((int)waitTimer.ElapsedMilliseconds/1000 - _priority, 500)); 
            } 
        }


        public Lane (string id, Server server, Vehicle type, int orangeTime)
        {
            _id = id;
            _laneNr = Convert.ToInt32(id.Substring(1,1));
            _direction = (WindDirection) Enum.Parse(typeof(WindDirection),
                Enum.GetNames(typeof(WindDirection)).First((s) => s.StartsWith(id.Substring(0, 1))), true);

            _server = server;
            _vehicle = type;
            _state = TrafficLighState.Red;
            _orangeTime = orangeTime;



            int windPriority = _direction == WindDirection.North || _direction == WindDirection.South ? 10 : 0;
            switch (type)
            {
                case Vehicle.PEDESTRIAN:
                    _priority = 73;
                    break;
                case Vehicle.BICYCLE:
                    _priority = 100;
                    break;
                case Vehicle.CAR:
                    _priority = 200 + windPriority;
                    break;
                case Vehicle.BUS:
                    _priority = 300 + windPriority;
                    break;
            }

            _compatibilityList = new bool[][,] { _compatibilitySelf, _compatibilityLeft, _compatibilityStraight, _compatibilityRight };
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

            if(!waitTimer.IsRunning)
                waitTimer.Start();
        }

        public void DecreaseQueue()
        {
            _queueCount = this.Vehicle == Vehicle.CAR ? _queueCount - 1 : 0;

            if (_queueCount == 0)
                waitTimer.Reset();
            
        }

        public void SetTafficLight(TrafficLighState state, int timeout)
        {
            timer = new Stopwatch();
            timer.Start();
            SetTafficLight(state);
            this._timeout = timeout * 1000;
        }

        //compatibility matrix for same-lane types    P1     P2     CL     CS     CR     BL     BS     BR     F      FR
        private bool[,] _compatibilitySelf = new[,]{    {true,  true,  true,  true,  true,  true,  true,  true,  false, true },
                                                        {true,  true,  false, false, false, false, false, false, false, false},
                                                        {true,  false, true,  true,  true,  false, true,  true,  false, true },
                                                        {true,  false, true,  true,  true,  false, false, true,  false, true },
                                                        {true,  false, false, true,  true,  false, false, false, false, true },
                                                        {true,  false, false, false, false, true,  false, false, false, true },
                                                        {true,  false, true,  false, false, false, true,  false, false, true },
                                                        {true,  false, true,  true,  false, false, false, true,  false, true },
                                                        {false, false, false, false, false, false, false, false, true,  true }, 
                                                        {true,  false, true,  true,  true,  true,  true,  true,  true,  true } };
        
        // TOP =  Which light is Green on the other lane 
        // LEFT = The lane you are checking


        //compatibility matrix for left-lane types        P1     P2     CL     CS     CR     BL     BS     BR     F      FR       
        private bool[,] _compatibilityLeft = new[,]{    {true,  true,  true,  true,  false, true,  true,  false, false, false},
                                                        {true,  true,  true,  true,  true,  true,  true,  true,  false, true },
                                                        {false, true,  false, false, true,  false, false, true,  false, true },
                                                        {true,  true,  false, false, true,  false, false, true,  false, true },
                                                        {true,  true,  true,  false, true,  true,  false, true,  false, true },
                                                        {false, true,  false, false, true,  false, false, true,  false, true },
                                                        {true,  true,  false, false, true,  false, false, true,  false, true },
                                                        {true,  true,  true,  false, true,  true,  false, true,  false, true },
                                                        {false, false, false, false, false, false, false, false, true,  true }, 
                                                        {true,  true,  true,  true,  true,  true,  true,  true,  true,  true } };

        //compatibility matrix for right-lane types       P1     P2     CL     CS     CR     BL     BS     BR     F      FR
        private bool[,] _compatibilityRight = new[,]{   {true,  true,  false, true,  true,  false, true,  true,  false, true },
                                                        {true,  true,  true,  true,  true,  true,  true,  true,  false, true },
                                                        {true,  true,  false, false, true,  false, false, true,  false, true },
                                                        {true,  true,  false, false, true,  false, false, true,  false, true },
                                                        {false, true,  true,  false, true,  true,  false, true,  false, true },
                                                        {true,  true,  false, false, true,  false, false, true,  false, true },
                                                        {true,  true,  false, false, true,  false, false, true,  false, true },
                                                        {false, true,  true,  false, true,  true,  false, true,  false, true },
                                                        {false, false, false, false, false, false, false, false, true,  true }, 
                                                        {false, true,  true,  true,  true,  true,  true,  true,  true,  true } };

        
        //compatibility matrix for straight-lane types    P1     P2     CL     CS     CR     BL     BS     BR     F      FR
        private bool[,] _compatibilityStraight=new[,]{  {true,  true,  true,  false, true,  true,  false, true,  false, true },
                                                        {true,  true,  true,  true,  true,  true,  true,  true,  false, true },
                                                        {true,  true,  true,  false, false, true,  false, false, false, true },
                                                        {false, true,  false, true,  true,  false, true,  true,  false, true },
                                                        {true,  true,  false, true,  true,  false, true,  true,  false, true },
                                                        {true,  true,  true,  false, false, true,  false, false, false, true },
                                                        {false, true,  false, true,  true,  false, true,  true,  false, true },
                                                        {true,  true,  false, true,  true,  false, true,  true,  false, true },
                                                        {false, false, false, false, false, false, false, false, true,  true }, 
                                                        {true,  true,  true,  true,  true,  true,  true,  true,  true,  true } };

        private Direction[] _W2Direction = { Direction.Self, Direction.Left, Direction.Forward, Direction.Right };
        private bool[][,] _compatibilityList;
        private int _orangeTime;
        public bool IsCompatible(Lane other)
        {
            //if (other == this)
            //    return true;

            Direction dir = _W2Direction[(int)(other._direction + 4 - _direction) % 4];
            bool[,] isCompat = _compatibilityList[(int)dir];

            return isCompat[CompatLane, other.CompatLane];

            //todo: add other lane stuff from different directions
            return false;
        }

        public Direction GetRelativeDirection(WindDirection other)
        {
            return _W2Direction[(int)(other + 4 - _direction) % 4];
        }

        public void Update()
        {
            if (timer == null)
                return;
            if (timer.ElapsedMilliseconds > _timeout)
            {
                if (_state == TrafficLighState.Green)
                    SetTafficLight(TrafficLighState.Orange, _orangeTime);
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
