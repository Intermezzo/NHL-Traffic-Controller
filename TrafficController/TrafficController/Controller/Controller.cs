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
                // log the amount of vehecles + time length of file
            }
            catch
            {
                //LOG-> xml load has failed
            }
        }
    }
}
