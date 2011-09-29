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
        public ControllerDialog()
        {
            InitializeComponent();
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
            try
            {
                XMLData.LoadScript(xmlFileName);
            }
            catch
            {
                MessageBox.Show("The chosen file was not compatible");
            }

        }

        private void verboseEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
