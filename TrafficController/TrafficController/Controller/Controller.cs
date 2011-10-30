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
            LaneManager laneManager = new LaneManager(_server); 
            timer.Start();
            while (!isStopped)
            {

                try
                {
                    if (candidate == null)
                        candidate = vehicleQueue.Dequeue();
                }
                catch { }

                if  (candidate != null && candidate.spawnTime < timer.ElapsedMilliseconds)
                {
                    string arg = String.Format("{0},{1},{2}", candidate.type, candidate.location, candidate.direction);
                    _server.RPCSendQueue.Enqueue(new RPCData() { type = 0, arg = arg });
                    candidate = null;
                }

                RPCData sensorInfo;
                while (_server.RPCReceiveQueue.TryDequeue(out sensorInfo))
                {
                    laneManager.SetSensor(sensorInfo.arg.Split(',')[0], "", "");
                }

                laneManager.Update();
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
