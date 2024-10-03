using Alpha_remote_controll.Model;
using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_remote_controll.VM
{
    public partial class ConnectionVM : ObservableValidator
    {
        #region Fields
        private ILoggerService _logger;
        private ConnectionModel? _connectionModel;

        #endregion

        #region Properties
        [ObservableProperty]
        private bool _IsAddresValid;

        [ObservableProperty]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Invalid IP address format.")]
        public string _ServerAddress;


        [ObservableProperty]
        private int _Port;

        [ObservableProperty]
        public string _StatusMessage;

        [ObservableProperty]
        private bool _IsUdpChecked;
        #endregion

        public ConnectionVM(ILoggerService logger)
        {
            _connectionModel = Ioc.Default.GetService<ConnectionModel>();
            _logger = logger;
            ServerAddress = "192.168.3.10"; // Default server address
            Port = 1595; // Default port
            IsAddresValid = false;
            OnServerAddressChanged(ServerAddress);
        }
        #region Methods
        //Send message when property changed with value
        partial void OnStatusMessageChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(value);
        }
        // Validate IP address
        partial void OnServerAddressChanged(string value)
        {
            ValidateProperty(value, nameof(ServerAddress));
            IsAddresValid = GetErrors(nameof(ServerAddress)).Cast<object>().Any() ? false : true;
        }
        partial void OnIsUdpCheckedChanged(bool value)
        {
            if (value)
            {
                StatusMessage = "UDP is turn on.";
                _logger.Log("UDP is turn on!", LogType.Info);
            }
            else
            {
                StatusMessage = "UDP is turn off.";
                _logger.Log("UDP is turn of!", LogType.Info);

            }
        }
        #endregion

        #region Commands

        //Connect to server
        [RelayCommand]
        public async Task Connect()
        {
            try
            {
                await _connectionModel.Connect(ServerAddress, Port);
                StatusMessage = "Connected to server";
                _logger.Log("You are connected to Alpha sofware", LogType.Success);
            }
            catch (Exception e)
            {
                StatusMessage = e.Message;
                _logger.Log(e.Message, LogType.Error);  
            }
        }

        #endregion
    }
}
