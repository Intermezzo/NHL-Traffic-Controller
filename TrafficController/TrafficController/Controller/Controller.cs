using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            while (!isStopped)
            {
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
