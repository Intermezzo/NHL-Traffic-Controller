using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficController
{
    [Serializable]
    public class settings
    {
        public DateTime startDate;
        public int minGreenTime;
        public int maxGreenTime;
        public int orangeTime;

        public settings() { }
    }
}
