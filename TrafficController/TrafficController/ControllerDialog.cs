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
            string xmlFileName = dialog.FileName;

            //controller.loadXML(xmlFileName);
        }

        private void verboseEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dumpEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoggerControl.Log(LogType.Spam, "When finished we'll  dump devents here");
        }
    }
}
