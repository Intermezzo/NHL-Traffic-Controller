﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace SENinputGenerator
{

    [Serializable]
    [XmlInclude(typeof(vehicle))]
    public class XMLData
    {
        public List<vehicle> vehicles;

        public XMLData()
        {
            vehicles = new List<vehicle>();
        }

        public void AddRangeVehicle(List<vehicle> vehiclesToAdd)
        {
            vehicles.AddRange(vehiclesToAdd);
        }

        public static XMLData LoadScript(string FileName)
        {
            XMLData data = null;
            FileStream stream = new FileStream("\\InputFiles\\" + FileName + ".xml", FileMode.Open);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
                data = (XMLData)serializer.Deserialize(stream);
            }
            catch (Exception error)
            {
                System.Windows.Forms.MessageBox.Show(error.Message);
            }
            finally
            {
                stream.Close();
            }
            return data;
        }

        public void SaveDataToFile(string FileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLData));
            Directory.CreateDirectory("\\InputFiles");
            FileStream stream = new FileStream( FileName + ".xml", FileMode.Create);
            serializer.Serialize(stream, this);
            stream.Close();
        }
        
    }
}
