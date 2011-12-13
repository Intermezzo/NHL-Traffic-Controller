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


    public class LaneManager
    {
        Server _server;
        Dictionary<string, Lane> _lanes = new Dictionary<string,Lane>();
        LinkedList<Lane> _waitingList = new LinkedList<Lane>();
        private settings _settings;

        public LaneManager(Server server, settings Settings)
        {
            _server = server;
            _settings = Settings;

            _settings.orangeTime = Math.Max(_settings.orangeTime, 5);

            //generate all lanes
            foreach (char side in new []{'N', 'S', 'E', 'W' })
            {
                for (int laneNr = 1; laneNr <= 8; laneNr++)
                {
                    string id = String.Format("{0}{1}", side, laneNr);
                    _lanes[id] = LaneFactory.FromLaneNr(id, laneNr, _server, _settings);
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
                    if (foo.Vehicle == Vehicle.BUS)
                    {
                        WindDirection _direction = (WindDirection)Enum.Parse(typeof(WindDirection),
                            Enum.GetNames(typeof(WindDirection)).First((s) => s.StartsWith(direction.Substring(0, 1))), true);

                        foo.BusDirection = foo.GetRelativeDirection(_direction);
                    }

                    //if all active lanes are compatible give the green signal
                    //if (activeLanes.All((l) => l.IsCompatible(foo)) && foo.State != TrafficLightState.Outage)
                    //{
                        ////todo check off state
                        //if (foo.State == TrafficLightState.Red)
                        //{
                        //    if (foo.Vehicle == Vehicle.BUS)
                        //    {
                        //        TrafficLightState newState;
                        //        switch (foo.BusDirection)
                        //        {
                        //            case Direction.Left:
                        //                newState = TrafficLightState.Left;
                        //                break;
                        //            case Direction.Right:
                        //                newState = TrafficLightState.Right;
                        //                break;
                        //            default:
                        //                newState = TrafficLightState.Straight;
                        //                break;
                        //        }
                        //        foo.SetTafficLight(newState, _settings.maxGreenTime);
                        //    }
                        //    else
                        //        foo.SetTafficLight(TrafficLightState.Green, _settings.maxGreenTime);
                        //}
                    //}
                    //else
                    //{
                    //    _waitingList.AddLast(foo);
                    //}
                    break;
                 case "1":
                    if (foo.Vehicle != Vehicle.CAR && foo.Vehicle != Vehicle.BUS)
                        throw new ArgumentException("Passing sensors should only be used for cars or busses.");
                    if (foo.Vehicle != Vehicle.CAR && foo.Vehicle != Vehicle.BUS)
                        return;

                    foo.DecreaseQueue();
                    //if (foo.QueueCount == 0)
                    //    _waitingList.Remove(foo);
                    break;
                default:
                    throw new ArgumentException(string.Format("{0} not a valid distance", distance));
            }


        }

        public void SetAnyTrafficLights(Predicate<Lane> predicate, int timeOut, TrafficLightState state)
        {
            var disableList = _lanes.Values.ToList().FindAll(predicate);
            disableList.ForEach((l) => l.SetTafficLight(state, timeOut));
        }

        public IEnumerable<Lane> GetActiveLanes()
        {
            return _lanes.Values.Where(
                (l) => (l.State == TrafficLightState.Green || 
                        l.State == TrafficLightState.Orange || 
                        l.State == TrafficLightState.Right ||
                        l.State == TrafficLightState.Left ||
                        l.State == TrafficLightState.Straight));
        }

        public void Update()
        {


            //if all active lanes are compatible give the green signal
            var activeLanes = GetActiveLanes().ToList();

            foreach (KeyValuePair<string, Lane> lane in _lanes)
            {
                lane.Value.Update();
                if (lane.Value.QueueCount == 0 && _waitingList.Contains(lane.Value))
                    _waitingList.Remove(lane.Value);

                if(lane.Value.QueueCount > 0 && !_waitingList.Contains(lane.Value) && !activeLanes.Contains(lane.Value))
                    _waitingList.AddLast(lane.Value);

                //if (lane.Value.Vehicle == Vehicle.BUS && 
                //    activeLanes.Contains(lane.Value) &&
                //    (lane.Value.QueueCount == 0 || lane.Value.BusDirection != lane.Value.BusDirections.Peek()))

            }

            var waitingList = _waitingList.OrderByDescending((l) => l.Priority);
            foreach (Lane lane in waitingList)
            {
                if (lane.State == TrafficLightState.Orange || lane.State == TrafficLightState.Outage)
                    continue;

                if (lane.QueueCount == 0)
                {
                    _waitingList.Remove(lane);
                    continue;
                }

                if (activeLanes.All((l) => l.IsCompatible(lane)))
                {
                    if (lane.Vehicle == Vehicle.BUS)
                    {
                        TrafficLightState newState;
                        switch (lane.BusDirection)
                        {
                            case Direction.Left:
                                newState = TrafficLightState.Left;
                                break;
                            case Direction.Right:
                                newState = TrafficLightState.Right;
                                break;
                            default:
                                newState = TrafficLightState.Straight;
                                break;
                        }
                        lane.SetTafficLight(newState, _settings.maxGreenTime);
                    }
                    else
                        lane.SetTafficLight(TrafficLightState.Green, _settings.maxGreenTime);

                    activeLanes.Add(lane);
                    _waitingList.Remove(lane);
                }
                else
                {
                    //find the lowest priority candidates which are passed the minimal green time and 
                    //would make this lane compatible if not active.
                    var foo = activeLanes.FindAll((l) => (l.State == TrafficLightState.Green ||
                                                        l.State == TrafficLightState.Right ||
                                                        l.State == TrafficLightState.Left ||
                                                        l.State == TrafficLightState.Straight) &&
                                                        l.TimeElapsed > _settings.minGreenTime * 1000 &&
                                                        l.Priority < lane.Priority &&
                                                        !lane.IsCompatible(l));
                    bool removingGreentimeMakesCompatible = activeLanes.TrueForAll((l) => lane.IsCompatible(l) || foo.Contains(l));

                    if (removingGreentimeMakesCompatible)
                    {
                        foreach (Lane bar in foo)
                            bar.SetTafficLight(TrafficLightState.Orange, _settings.orangeTime);
                    }
                }
            }

            /*var node = _waitingList.First;
            /while (node != null)
            {
                var nextNode = node.Next;
                if (activeLanes.All((l) => l.IsCompatible(node.Value)))
                {
                    node.Value.SetTafficLight(TrafficLighState.Green, _settings.maxGreenTime);
                    _waitingList.Remove(node);
                }
                node = nextNode;
            }*/
        }
    }
}
