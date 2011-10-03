using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TrafficController
{
    public partial class ControllerDialog : Form
    {
        private LoggerControl _loggerControl; 
        private XMLData _xmlData;
        private Controller _controller;

        public LoggerControl LoggerControl
        {
            get { return _loggerControl; }
        }

        public ControllerDialog()
        {
            InitializeComponent();
            _loggerControl = new LoggerControl(logView);
        }
                
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // load xml
            FileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            string xmlFilePath = dialog.FileName;

            try
            {
                _xmlData = XMLData.LoadScript(xmlFilePath);
                LoggerControl.Log(LogType.Notice, "Succesfully loaded XML (Last vehicle spawn on: " + _xmlData.vehicles[_xmlData.vehicles.Count - 1].spawnTime + " ms; Containing :" + _xmlData.vehicles.Count + " vehicles)");
            }
            catch
            {
                LoggerControl.Log(LogType.Error, "Failed to load File");
            }
        }

        private void verboseEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dumpEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoggerControl.Log(LogType.Spam, "When finished we'll  dump devents here");
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (_xmlData == null)
            {
                _loggerControl.Log(LogType.Warning, "Can't start without a loaded XML file");
                return;
            }
            _controller = new Controller(this, _xmlData);
            _loggerControl.Log(LogType.Notice, "Controller succesfully started!");            
        }
    }
}
