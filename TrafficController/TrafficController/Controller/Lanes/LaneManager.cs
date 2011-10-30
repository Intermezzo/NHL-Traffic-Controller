using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficController
{
    class LaneManager
    {
        Server _server;
        Dictionary<string, Lane> _lanes = new Dictionary<string,Lane>();
        public LaneManager(Server server)
        {
            _server = server;
        }

        public void SetSensor(string stoplichtID, string distance, string direction)
        {
            Lane foo;
            if (!_lanes.TryGetValue(stoplichtID, out foo))
                _lanes[stoplichtID] = foo = new Lane(stoplichtID, _server);

            foo.SetTafficLight(TrafficLighState.Green, 20000);
        }

        public void Update()
        {
            foreach (KeyValuePair<string, Lane> lane in _lanes)
            {
                lane.Value.Update();
            }
        }


    }
}
