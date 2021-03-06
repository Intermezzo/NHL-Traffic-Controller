﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace TrafficController
{

    [Serializable]
    [XmlInclude(typeof(vehicle))]
    [XmlInclude(typeof(settings))]
    [XmlRoot(ElementName = "trafficlightSimulator")]
    public class XMLData
    {
        public settings settings;
        public List<vehicle> vehicles;

        public XMLData()
        {
            vehicles = new List<vehicle>();
        }

        public static XMLData LoadScript(string FileName)
        {
            XMLData data = null;
            FileStream stream = new FileStream(FileName, FileMode.Open);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
                data = (XMLData)serializer.Deserialize(stream);
                if (!data.vehicles.TrueForAll((l) => l.location != l.direction))
                    throw new Exception("Invalid XML file: Vehicles exist which have equal direction and location.");
            }
            catch (Exception error)
            {
                System.Windows.Forms.MessageBox.Show(error.Message);
                data = null;
            }
            finally
            {
                stream.Close();
            }
            return data;
        }       
    }
}
