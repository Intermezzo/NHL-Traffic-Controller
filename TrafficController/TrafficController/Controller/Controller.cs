using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TrafficController.Controller;

namespace TrafficController
{
    class Controller : IDisposable
    {
        private XMLData _xmlData;
        private Thread _main;
        private bool isStopped;

        //todo: start the new server, and the controller on a new thread

        Controller()
        {
            _main = new Thread(mainLoop);
        }

        void mainloop()
        {
            while (!isStopped)
            {
                Thread.Sleep(1);

            }
        }

        public void LoadXML(string filePath)
        {
            try
            {
                _xmlData = XMLData.LoadScript(filePath);
                // log the amount of vehecles + time length of file
            }
            catch
            {
                //LOG-> xml load has failed
            }
        }

        public void Stop()
        {
            isStopped = true;
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
