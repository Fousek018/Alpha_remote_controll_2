using Alpha_remote_controll.Model;
using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Alpha_remote_controll.VM
{
    public partial class LogVM : ObservableObject
    {
        #region Properties

        //Checkboxes for filtering log entries
        //Checkboxes for filtering log entries
        private bool isInfoChecked;
        public bool IsInfoChecked
        {
            get => isInfoChecked;
            set
            {
                SetProperty(ref isInfoChecked, value);
                UpdateAllChecked();
            }
        }
        private bool isSuccesChecked;
        public bool IsSuccesChecked
        {
            get => isInfoChecked;
            set
            {
                SetProperty(ref isSuccesChecked, value);
                UpdateAllChecked();
            }
        }

        private bool isErrorChecked;
        public bool IsErrorChecked
        {
            get => isErrorChecked;
            set
            {
                SetProperty(ref isErrorChecked, value);
                UpdateAllChecked();
            }
        }

        private bool isWarningChecked;
        public bool IsWarningChecked
        {
            get => isWarningChecked;
            set
            {
                SetProperty(ref isWarningChecked, value);
                UpdateAllChecked();
            }
        }

        private bool isAllChecked;
        public bool IsAllChecked
        {
            get => isAllChecked;
            set
            {
                SetProperty(ref isAllChecked, value);
                if (isAllChecked)
                {
                    IsInfoChecked = true;
                    IsErrorChecked = true;
                    IsWarningChecked = true;
                }
                else
                {
                    IsInfoChecked = false;
                    IsErrorChecked = false;
                    IsWarningChecked = false;
                }
            }
        }

        private DateTime? selectedDate;
        public DateTime? SelectedDate
        {
            get => selectedDate;
            set
            {
                SetProperty(ref selectedDate, value);
                LogEntriesViewSource.View.Refresh();
            }
        }
        #endregion

        #region fields
        private readonly ILoggerService _loggerService;
        public ObservableCollection<LogEntry> LogEntries { get; set; }
        private CollectionViewSource LogEntriesViewSource;
        public ICollectionView LogEntriesView => LogEntriesViewSource.View;

        #endregion


        public LogVM(ILoggerService loggerService)
        {
            //Initialize logger service
            _loggerService = loggerService;
            LogEntries = new ObservableCollection<LogEntry>(_loggerService.LogEntries.OrderByDescending(entry => entry.Timestamp));
            _loggerService.LogEntries.CollectionChanged += OnLogEntriesChanged;

            //Initialize date
            selectedDate = DateTime.Now;

            //Initialize checkboxes
            IsAllChecked = true;
            IsErrorChecked = true;
            IsInfoChecked = true;
            IsWarningChecked = true;

            //Initialize CollectionViewSource for filtering
            LogEntriesViewSource = new CollectionViewSource { Source = LogEntries };
            LogEntriesViewSource.Filter += ApplyFilter;

            // Subscribe to property changes for filtering
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsInfoChecked) ||
                    e.PropertyName == nameof(IsErrorChecked) ||
                    e.PropertyName == nameof(IsWarningChecked) ||
                    e.PropertyName == nameof(IsSuccesChecked) ||
                    e.PropertyName == nameof(IsAllChecked))
                {
                    LogEntriesViewSource.View.Refresh();
                }
            };

        }
        #region Methods
        //Filter log entries on file change based on selected checkboxes
        private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (LogEntry newItem in e.NewItems)
                {
                    LogEntries.Insert(0, newItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (LogEntry oldItem in e.OldItems)
                {
                    LogEntries.Remove(oldItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                LogEntries.Clear();
            }

            LogEntriesViewSource.View.Refresh();
        }

        //Filter log entries based on selected checkboxes
        private void ApplyFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is LogEntry logEntry)
            {
                // Filter by date
                if (SelectedDate.HasValue && logEntry.Timestamp.Date != SelectedDate.Value.Date)
                {
                    e.Accepted = false;
                    return;
                }

                //Checkboxes selected, show all
                if (IsAllChecked)
                {
                    e.Accepted = true;
                    return;
                }
                //Filter based on selected checkboxes
                e.Accepted = (IsInfoChecked && logEntry.Type == LogType.Info) ||
                             (IsErrorChecked && logEntry.Type == LogType.Error) ||
                             (IsWarningChecked && logEntry.Type == LogType.Warning) ||
                             (IsSuccesChecked && logEntry.Type == LogType.Success);
            }
        }
        //Update All checkbox based on other checkboxes
        private void UpdateAllChecked()
        {
            //if all of them are checked
            if (IsInfoChecked && IsErrorChecked && IsWarningChecked)
            {
                isAllChecked = true;
                OnPropertyChanged(nameof(IsAllChecked));
            }
            //other possibility
            else
            {
                isAllChecked = false;
                OnPropertyChanged(nameof(IsAllChecked));
            }
        }
        #endregion


    }
}



