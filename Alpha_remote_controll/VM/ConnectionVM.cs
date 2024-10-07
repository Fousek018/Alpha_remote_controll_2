using Alpha_remote_controll.Model;
using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string _DeviceName;

        [ObservableProperty]
        private int _Port;

        [ObservableProperty]
        public string _StatusMessage;

        [ObservableProperty]
        private bool _IsUdpChecked;
        #endregion

        public ObservableCollection<ConnectionModel> connectedDevices { get; set; } // Collection of connected devices, for multiple connections

        public ConnectionVM(ILoggerService logger)
        {
            
            connectedDevices = new ObservableCollection<ConnectionModel>();
            _logger = logger;
            ServerAddress = "192.168.3.10"; // Default server address
            Port = 1595; // Default port
            IsAddresValid = false;
            OnServerAddressChanged(ServerAddress);

            connectedDevices = new ObservableCollection<ConnectionModel> //temporary data for testing
            {
                new ConnectionModel { DeviceName = "Station CreepTest", ServerAddress = "192.168.1.1", Port = 8080 },
                new ConnectionModel { DeviceName = "Shredder", ServerAddress = "192.168.1.2", Port = 8081, _client = null, _stream = null },
                new ConnectionModel { DeviceName = "Test", ServerAddress = "192.168.1.2", Port = 8081, _client = null, _stream = null },
                new ConnectionModel { DeviceName = "Test", ServerAddress = "192.168.1.2", Port = 8081, _client = null, _stream = null },
                new ConnectionModel { DeviceName = "Test", ServerAddress = "192.168.1.2", Port = 8081, _client = null, _stream = null }
            };
        }
        #region Methods
        //Send message when property changed with value, not working yet
        partial void OnStatusMessageChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(value);
        }
        // Validate IP address on change
        partial void OnServerAddressChanged(string value)
        {
            ValidateProperty(value, nameof(ServerAddress));
            IsAddresValid = GetErrors(nameof(ServerAddress)).Cast<object>().Any() ? false : true;
        }
        partial void OnIsUdpCheckedChanged(bool value) // Log message when UDP is turned on or off
        {
            StatusMessage = value ? "UDP is turn on." : "UDP is turn off.";
            _logger.Log(value ? "UDP is turn on!" : "UDP is turn off!", LogType.Info);
        }
        #endregion

        #region Commands

        //Command for connecting to server
        [RelayCommand]
        public async Task Connect()
        {
            //Test if the connection is already established, validate the IP address
            if (connectedDevices.Any(device => device.ServerAddress == ServerAddress))
            {
                StatusMessage = "You are already connected to this server";
                _logger.Log("You are already connected to  this server", LogType.Info);
            }
            else
            { 
                try
                {
                    _connectionModel = new ConnectionModel();
                    var device = await _connectionModel.ConnectAsync(ServerAddress, Port);
                    connectedDevices.Add(device); // Add the connected device to the collection, for multiple connections.
                    StatusMessage = "Connected to server";
                    _logger.Log("You are connected to Alpha sofware", LogType.Success);
                }
                catch (Exception e)
                {
                    StatusMessage = e.Message;
                    _logger.Log(e.Message, LogType.Error);
                }
            }
        }

        #endregion
    }
}
