using Alpha_remote_controll.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Alpha_remote_controll.Services {
    public interface ILoggerService
    {
        void Log(string message, LogType type);
        ObservableCollection<LogEntry> LogEntries { get; }

    }
    public partial class LoggerService : ObservableObject , ILoggerService
    {
        [ObservableProperty]
        public int _Zk = 0;


        private readonly string _logFilePath;
        public ObservableCollection<LogEntry> _logEntries;

        public LoggerService(string logFilePath)
        {
            _logFilePath = logFilePath;
            _logEntries = new ObservableCollection<LogEntry>();
            LoadLogFromFile();
        }

        public ObservableCollection<LogEntry> LogEntries => _logEntries;

        public void Log(string message, LogType type)
        {
            var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Type = type
                };

            _logEntries.Add(logEntry);
            SaveLogToFile();
            Zk++;
        }
        private void SaveLogToFile()
        {
            var serializer = new XmlSerializer(typeof(ObservableCollection<LogEntry>));
            using (var writer = new StreamWriter(_logFilePath))
            {
                serializer.Serialize(writer, _logEntries);
            }
        }

        private void LoadLogFromFile()
        {
            if (File.Exists(_logFilePath))
            {
                 var serializer = new XmlSerializer(typeof(ObservableCollection<LogEntry>));
                using (var reader = new StreamReader(_logFilePath))
                {
                    var logEntriesFromFile = (ObservableCollection<LogEntry>)serializer.Deserialize(reader);
                    _logEntries.Clear();
                    foreach (var logEntry in logEntriesFromFile)
                    {
                        _logEntries.Add(logEntry);
                    }
                }
            }
        }



    }
}