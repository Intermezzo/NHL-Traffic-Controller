using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficController
{
    class Controller
    {
        private XMLData _xmlData;


        //todo: start the new server, and the controller on a new thread
        void Mainloop()
        {

        }

        public void LoadXML(string filePath)
        {
            try
            {
                _xmlData = XMLData.LoadScript(filePath);
                //LoggerControl.Log(LogType.Notice, "succesfully loaded XML (duration: " + _xmlData.vehicles[_xmlData.vehicles.Count - 1] + " ms with "+_xmlData.vehicles.Count+" vehicles)" );
            }
            catch
            {
                //LoggerControl.Log(LogType.Error, "Failed to load File");
            }
        }
    }
}
