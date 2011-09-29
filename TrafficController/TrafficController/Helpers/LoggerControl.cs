using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TrafficController
{

    public enum LogType
    {
        Spam,
        Notice,
        Warning,
        Error,
        Critical
    }

    public class LoggerControl
    {
        RichTextBox logView;
        public LoggerControl(RichTextBox logView)
        {
            this.logView = logView;
        }

        public void Log(LogType type, string message)
        {
            string log = string.Format(" [{0}] {1}: {2} \n", DateTime.Now, type, message);

            if (!logView.InvokeRequired)
                logView.AppendText(log);
            else
                logView.Invoke( (Action) (() => logView.AppendText(log)) );

        }

        public void Log(Exception e)
        {
            Log(LogType.Error, e.Message);
        }

    }
}
