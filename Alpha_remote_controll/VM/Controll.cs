using Alpha_remote_controll.Model;
using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alpha_remote_controll.VM
{

    public partial class Controll : ObservableObject
    {
        #region Properties
        private string alphaListMethod = "alphaListMethods";

        [ObservableProperty]
        public string _StatusMessage;

        [ObservableProperty]
        private ObservableCollection<DeviceVM> _Devices;

        #endregion

        #region Fields
        private ConnectionVM _connectionVM;
        private ILoggerService _logger;
        private ObservableCollection<ConnectionDetails> _connectedDevices;

        #endregion
        public Controll(ILoggerService logger)
        {
            _connectionVM = Ioc.Default.GetRequiredService<ConnectionVM>(); // Get the connection view model
            ConnectedDevices = _connectionVM.connectedDevices; // Sync the connected devices collection
            _connectionVM.PropertyChanged += ConnectionVM_PropertyChanged; // Subscribe to property changed event
            _logger = logger;
            _connectedDevices.CollectionChanged += ConnectedDevices_CollectionChanged;
            Devices = new ObservableCollection<DeviceVM>();
            foreach (var connection in _connectedDevices)
            {
                Devices.Add(new DeviceVM(connection, _logger, new QueryService()));
            }

        }

        private void ConnectedDevices_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    // Přidání nových prvků do kolekce
                    foreach (ConnectionDetails newConnection in e.NewItems)
                    {
                        Devices.Add(new DeviceVM(newConnection, _logger, new QueryService()));
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    // Odebrání prvků z kolekce
                    foreach (ConnectionDetails oldConnection in e.OldItems)
                    {
                        var deviceVMToRemove = Devices.FirstOrDefault(vm => vm.ConnectionDetails == oldConnection);
                        if (deviceVMToRemove != null)
                        {
                            Devices.Remove(deviceVMToRemove);
                        }
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    // Pokud by došlo k nahrazení prvku (není časté, ale může se stát)
                    foreach (ConnectionDetails oldConnection in e.OldItems)
                    {
                        var deviceVMToRemove = Devices.FirstOrDefault(vm => vm.ConnectionDetails == oldConnection);
                        if (deviceVMToRemove != null)
                        {
                            Devices.Remove(deviceVMToRemove);
                        }
                    }
                    foreach (ConnectionDetails newConnection in e.NewItems)
                    {
                        Devices.Add(new DeviceVM(newConnection, _logger, new QueryService()));
                    }
                    break;

                    // Další akce, které lze řešit (například Reset), ale záleží na scénáři
            }
        }
        #region Properties

        public ObservableCollection<ConnectionDetails> ConnectedDevices
        {
            get => _connectedDevices;
            set => SetProperty(ref _connectedDevices, value);
        }


        #endregion

        private void ConnectionVM_PropertyChanged(object? sender, PropertyChangedEventArgs e) // Event handler for property changed event, sync the connected devices collection
        {
            ConnectedDevices = _connectionVM.connectedDevices; // Sync the connected devices collection
        }


        #region Methods

        #endregion

        #region Commands

        #endregion
    }
}
