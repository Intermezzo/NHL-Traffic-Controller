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
        List<Lane> _waitingList = new List<Lane>();

        public LaneManager(Server server)
        {
            _server = server;

            //generate all lanes
            foreach (char side in new []{'N', 'S', 'E', 'W' })
            {
                for (int laneNr = 0; laneNr < 8; laneNr++)
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

            //if all active lanes are compatible give the green signal
            if (activeLanes.All((l) => l.IsCompatible(foo)))
                foo.SetTafficLight(TrafficLighState.Green, 20000);
            else
                _waitingList.Add(foo);
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
            }

            //if all active lanes are compatible give the green signal
            var activeLanes = GetActiveLanes();
            foreach (Lane lane in _waitingList)
            {
                if (activeLanes.All((l) => l.IsCompatible(lane)))
                    lane.SetTafficLight(TrafficLighState.Green, 20000);
            }
        }


    }
}
