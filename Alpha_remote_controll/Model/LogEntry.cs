using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_remote_controll.Model
{

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogType Type { get; set; } // "Error", "Success", "Other"
    }
    public enum LogType
    {
        Error,
        Success,
        Warning,
        Info
    }
}
