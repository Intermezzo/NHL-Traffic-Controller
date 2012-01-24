using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;


namespace TrafficController
{
    class Controller : IDisposable
    {
        private ControllerDialog _controllerDialog;
        private XMLData _xmlData;
        private Thread _main;
        private Server _server;

        private bool isStopped;

        bool isDisabled;
        bool isDisabledCars;
        bool outageDisabled;

        static TimeSpan CAR_OUTAGE_START = new TimeSpan(0);
        static TimeSpan CAR_OUTAGE_TIME = new TimeSpan(2, 0, 0);

        static TimeSpan ALL_DISABLED_START = new TimeSpan(0, 0, 0);
        static TimeSpan ALL_DISABLED_TIME = new TimeSpan(4, 0, 0);

        static TimeSpan CAR_DISABLED_START = new TimeSpan(2, 0, 0);
        static TimeSpan CAR_DISABLED_TIME = new TimeSpan(2, 0, 0);
        //todo: start the new server, and the controller on a new thread

        public Controller(ControllerDialog controllerDialog, XMLData xmlData)
        {
            _main = new Thread(mainloop);
            _server = new Server(controllerDialog.LoggerControl);
            _controllerDialog = controllerDialog;
            _xmlData = xmlData;

            _main.Start();
        }

        private void mainloop()
        {
            Stopwatch timer = new Stopwatch();
            Queue<vehicle> vehicleQueue = new Queue<vehicle>(_xmlData.vehicles);
            vehicle candidate = null;
            LaneManager laneManager = new LaneManager(_server,_xmlData.settings); 
            
            //todo: wait on incoming connections first
            while (!isStopped)
            {
                if (_server.IsStopped)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (!timer.IsRunning)
                    timer.Start();

                try
                {
                    if (candidate == null)
                        candidate = vehicleQueue.Dequeue();
                }
                catch { }

                try
                {
                    if (candidate != null && candidate.spawnTime < timer.ElapsedMilliseconds)
                    {
                        string arg = String.Format("{0},{1},{2}", candidate.type, candidate.location, candidate.direction);
                    //    if (candidate.type != "PEDESTRIAN" && candidate.type != "BICYCLE")
                        _server.RPCSendQueue.Enqueue(new RPCData() { type = 0, arg = arg });
                        //if (candidate.location == "N" && candidate.type == "PEDESTRIAN")
                        //    _controllerDialog.LoggerControl.Log(LogType.Spam, String.Format("Vehicle {0} spawned on {1}", candidate.type, candidate.location));
                        candidate = null;
                    }

                    RPCData sensorInfo;
                    while (_server.RPCReceiveQueue.TryDequeue(out sensorInfo))
                    {
                        string[] sensorInfoS = sensorInfo.arg.Split(',');
                        laneManager.SetSensor(sensorInfoS[0], sensorInfoS[1], sensorInfoS[2]);
                    }

                    DateTime currentDate = _xmlData.settings.startDate + new TimeSpan(timer.ElapsedTicks);

                    if (currentDate.TimeOfDay > ALL_DISABLED_START && !isDisabled && currentDate.TimeOfDay < ALL_DISABLED_START + ALL_DISABLED_TIME)
                    {
                        laneManager.SetAnyTrafficLights((l) => l.Vehicle != Vehicle.CAR, (int) ALL_DISABLED_TIME.TotalMilliseconds, TrafficLightState.Off);
                        isDisabled = true;
                    }

                    if (currentDate.TimeOfDay > CAR_OUTAGE_START && !outageDisabled && currentDate.TimeOfDay < CAR_OUTAGE_START + CAR_OUTAGE_TIME)
                    {
                        laneManager.SetAnyTrafficLights((l) => l.Vehicle == Vehicle.CAR, (int)CAR_OUTAGE_TIME.TotalMilliseconds, TrafficLightState.Outage);
                        outageDisabled = true;
                    }

                    if (currentDate.TimeOfDay > CAR_DISABLED_START && !isDisabledCars && currentDate.TimeOfDay < CAR_DISABLED_START + CAR_DISABLED_TIME)
                    {
                        laneManager.SetAnyTrafficLights((l) => l.Vehicle == Vehicle.CAR, (int)CAR_DISABLED_TIME.TotalMilliseconds, TrafficLightState.Off);
                        isDisabledCars = true;
                    }

                    laneManager.Update();
                }
#if DEBUG
                catch (EncoderFallbackException e)
#else
                catch (Exception e)
#endif
                {
                    _controllerDialog.LoggerControl.Log(e);
                }

                Thread.Sleep(1);
            }
        }

        public void Stop()
        {
            isStopped = true;
            _server.Stop();
            _main.Join();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
