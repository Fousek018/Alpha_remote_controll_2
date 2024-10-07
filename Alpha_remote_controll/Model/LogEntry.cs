using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_remote_controll.Model
{

    public class LogEntry
    {
        //public LogEntry(DateTime timestamp, string message, LogType type)
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogType Type { get; set; } // "Error", "Success", "Other"
    }
    public enum LogType
    {
        //Enum for different types of log entries
        Error,
        Success,
        Warning,
        Info
    }
}
