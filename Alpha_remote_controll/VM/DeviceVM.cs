using Alpha_remote_controll.Model;
using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alpha_remote_controll.VM
{
    public partial class DeviceVM : ObservableObject
    {
        #region fields
        
        private ConnectionDetails? _connectionDetails;
        private ILoggerService _logger;
        private IQueryService? _queryService;
        #endregion
        public DeviceVM(ConnectionDetails connectionDetails, ILoggerService logger, IQueryService queryService)
        {
            _connectionDetails = connectionDetails;
            _logger = logger;
            _queryService = queryService;
            _connectionDetails.PropertyChanged += OnConnectionDetailsChanged;
        
        }
        public ConnectionDetails ConnectionDetails => _connectionDetails;

        private void OnConnectionDetailsChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Vyvolání změny ve ViewModelu na základě změny v modelu
            OnPropertyChanged(e.PropertyName);
        }

        #region Properties
        [ObservableProperty]
        string _StatusMessage;

        public string DeviceName
        {
            get => _connectionDetails.DeviceName;
            set
            {
                if (_connectionDetails.DeviceName != value)
                {
                    _connectionDetails.DeviceName = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ServerAddress
        {
            get => _connectionDetails.ServerAddress;
            set
            {
                if (_connectionDetails.ServerAddress != value)
                {
                    _connectionDetails.ServerAddress = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Port
        {
            get => _connectionDetails.Port;
            set
            {
               if (_connectionDetails.Port != value)
                {
                    _connectionDetails.Port = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> MethodNames
        {
            get => _connectionDetails.MethodNames;
            set
            {
                if (_connectionDetails.MethodNames != value)
                {
                    _connectionDetails.MethodNames = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SelectedMethod
        {
            get => _connectionDetails.SelectedMethod;
            set
            {
                if (_connectionDetails.SelectedMethod != value)
                {
                    _connectionDetails.SelectedMethod = value;
                    SwitchMethod(SelectedMethod);
                    OnPropertyChanged();
                }
            }
        }

        private async void SwitchMethod(string selectedMethod)
        {
            // Vytvoření příkazu pro změnu metody, do budoucna lze více automatizovat
            var command = new
            {
                name = "alphaSwitchMethod",
                methodName = selectedMethod,
                distanceToLoadingAxis = (double?)null,  //lze popřípadě doplnit hodnoty
                specimenThickness = (double?)null,      //lze popřípadě doplnit hodnoty
                specimenDiameter = (double?)null        //lze popřípadě doplnit hodnoty
            };
            string serializedCommand = JsonSerializer.Serialize(command);
            var methods = await _queryService.SendQueryAsync<status>(serializedCommand, _connectionDetails);
            StatusMessage = methods.message;
        }



        #endregion

        #region Commands

        [RelayCommand]
        public async Task GetMethods()
        {
           var command = new
           {
               name = "alphaListMethods"
           };

           string serializedCommand = JsonSerializer.Serialize(command);
           var methods= await _queryService.SendQueryAsync<alphaMethodList>(serializedCommand, _connectionDetails);
           MethodNames = methods.methodNames;
        }

        [RelayCommand]
        public void GetValues()
        {

        }
        [RelayCommand]
        public async Task StartListening()
        {
            Task.Run(() => _queryService.StartListeningForEventsAndQueriesAsync(_connectionDetails));

        }
            #endregion


        }
}
