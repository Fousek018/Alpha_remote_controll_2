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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_remote_controll.VM
{
    public partial class ConnectionVM : ObservableValidator
    {
        #region Fields
        private ILoggerService _logger;
        private IConnectionService _connectionService;
        private ConnectionDetails? _connectionModel;

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

        public ObservableCollection<ConnectionDetails> connectedDevices { get; set; } // Collection of connected devices, for multiple connections

        public ConnectionVM(ILoggerService logger, IConnectionService connectionService)
        {
            _connectionService = connectionService;
            connectedDevices = new ObservableCollection<ConnectionDetails>();
            _logger = logger;
            ServerAddress = "192.168.3.10"; // Default server address
            Port = 1595; // Default port
            IsAddresValid = false;

            //write multiple test device for observable collection connectedDevices
            


            OnServerAddressChanged(ServerAddress);

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
                    _connectionModel = new ConnectionDetails { ServerAddress = ServerAddress, Port = Port, DeviceName = "alphaConnection" };
                    await _connectionService.ConnectAsync(_connectionModel);
                    connectedDevices.Add(_connectionModel); // Add the connected device to the collection, for multiple connections.
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

        [RelayCommand]
        public void Disconnect(ConnectionDetails connectionDetails)
        {
            if (connectedDevices.Contains(connectionDetails))
            {
                try
                {
                    // Odpojení zařízení
                    connectionDetails.Client?.Close(); // Zavření TCP klienta
                    connectionDetails.Stream?.Close(); // Zavření síťového streamu

                    // Odstranění zařízení z kolekce
                    connectedDevices.Remove(connectionDetails);

                    StatusMessage = $"Disconnected from {connectionDetails.DeviceName}";
                    _logger.Log($"Disconnected from {connectionDetails.DeviceName}", LogType.Info);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error disconnecting from {connectionDetails.DeviceName}: {ex.Message}";
                    _logger.Log($"Error disconnecting from {connectionDetails.DeviceName}: {ex.Message}", LogType.Error);
                }
            }
            else
            {
                StatusMessage = "Device not found in connected devices.";
                _logger.Log("Device not found in connected devices.", LogType.Warning);
            }
        }

        #endregion
    }
}
