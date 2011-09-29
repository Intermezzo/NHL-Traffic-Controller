using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficController
{
    [Serializable]
    public class vehicle
    {
        public string type; // BUS/CAR/BICYCLE/PEDESTRIAN
        public int spawnTime; // in milliseconds
        public string location; // N/S/E/W
        public string direction; // N/S/E/W

        public vehicle() { }
        public vehicle(string Type, int SpawnTime, string Location, string Direction)
        {
            type = Type;
            spawnTime = SpawnTime;
            location = Location;
            direction = Direction;
        }
    }
}
