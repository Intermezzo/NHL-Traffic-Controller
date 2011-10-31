using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficController
{
    static class LaneFactory
    {
        static public Lane FromLaneNr(string id, int laneNr, Server server)
        {
            switch (laneNr)
            {
                case 1:
                case 2:
                    return new Lane(id, server, Vehicle.PEDESTRIAN);
                case 3:
                case 4:
                case 5:
                    return new Lane(id, server, Vehicle.CAR);
                case 6:
                    return new Lane(id, server, Vehicle.BUS);
                case 7:
                case 8:
                    return new Lane(id, server, Vehicle.BICYCLE);
                default:
                    throw new ArgumentException("laneNr");


            }
        }
    }
}
