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
                        _server.RPCSendQueue.Enqueue(new RPCData() { type = 0, arg = arg });
                        candidate = null;
                    }

                    RPCData sensorInfo;
                    while (_server.RPCReceiveQueue.TryDequeue(out sensorInfo))
                    {
                        string[] sensorInfoS = sensorInfo.arg.Split(',');
                        laneManager.SetSensor(sensorInfoS[0], sensorInfoS[1], sensorInfoS[2]);
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
