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

    class Lane
    {

        private string _id;
        private int _priority;
        TrafficLighState state;
        Stopwatch timer;
        int timeout;
        Server _server;
        Vehicle vehicle = Vehicle.NONE;

        public Lane (string id,Server server)
        {
            _id = id;
            _server = server;
        }

        public void SetTafficLight(TrafficLighState state)
        {
            this.state = state;
            string arg = string.Format("{0},{1}",_id, (int)state);
            _server.RPCSendQueue.Enqueue(new RPCData(){type = 1, arg = arg});
        }

        public void SetTafficLight(TrafficLighState state, int timeout)
        {
            timer = new Stopwatch();
            timer.Start();
            SetTafficLight(state);
            this.timeout = timeout;
        }

        public void Update()
        {
            if (timer.ElapsedMilliseconds > timeout)
            {
                if (state == TrafficLighState.Green)
                    SetTafficLight(TrafficLighState.Orange, 15000);
                else
                {
                    SetTafficLight(TrafficLighState.Red);
                    timer.Reset();
                }
            }
        }


    }
}
