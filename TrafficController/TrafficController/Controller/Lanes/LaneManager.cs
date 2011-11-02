using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficController
{
    public enum LaneType
    {
        Pedestrian,
        Car,
        Bus,
        Bike
    }


    class LaneManager
    {
        Server _server;
        Dictionary<string, Lane> _lanes = new Dictionary<string,Lane>();
        LinkedList<Lane> _waitingList = new LinkedList<Lane>();

        public LaneManager(Server server)
        {
            _server = server;

            //generate all lanes
            foreach (char side in new []{'N', 'S', 'E', 'W' })
            {
                for (int laneNr = 1; laneNr <= 8; laneNr++)
                {
                    string id = String.Format("{0}{1}", side, laneNr);
                    _lanes[id] = LaneFactory.FromLaneNr(id, laneNr, _server);
                }
            }
        }

        public void SetSensor(string stoplichtID, string distance, string direction)
        {
            Lane foo;
            if (!_lanes.TryGetValue(stoplichtID, out foo))
                throw new ArgumentException("stoplichtID not valid");

            //we presume a robin hood distribution for now
            //get active lanes
            var activeLanes = GetActiveLanes();

            switch(distance)
            {
                case "100":
                    foo.IncreaseQueue();
                    
                    //if all active lanes are compatible give the green signal
                    if (activeLanes.All((l) => l.IsCompatible(foo)))
                    {
                        //todo check off state
                        if (foo.State == TrafficLighState.Red)
                            foo.SetTafficLight(TrafficLighState.Green, 15000);
                    }
                    else
                    {
                        _waitingList.AddLast(foo);
                    }
                    break;
                 case "1":
                    if (foo.Vehicle != Vehicle.CAR)
                        throw new ArgumentException("Nearing sensor should only be used for cars.");
                    foo.DecreaseQueue();
                    if (foo.QueueCount == 0)
                        _waitingList.Remove(foo);
                    break;
                default:
                    throw new ArgumentException(string.Format("{0} not a valid distance", distance));
            }


        }

        public IEnumerable<Lane> GetActiveLanes()
        {
            return _lanes.Values.Where(
                (l) => (l.State == TrafficLighState.Green || l.State == TrafficLighState.Orange));
        }

        public void Update()
        {

            foreach (KeyValuePair<string, Lane> lane in _lanes)
            {
                lane.Value.Update();
                if (lane.Value.QueueCount == 0)
                    _waitingList.Remove(lane.Value);
            }

            //if all active lanes are compatible give the green signal
            var activeLanes = GetActiveLanes();

            var node = _waitingList.First;
            while (node != null)
            {
                var nextNode = node.Next;
                if (activeLanes.All((l) => l.IsCompatible(node.Value)))
                {
                    node.Value.SetTafficLight(TrafficLighState.Green, 20000);
                    _waitingList.Remove(node);
                }
                node = nextNode;
            }
        }
    }
}
