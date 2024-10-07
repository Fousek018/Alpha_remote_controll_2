using Alpha_remote_controll.Model;
using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [ObservableProperty]
        public string _StatusMessage;

        #endregion

        #region Fields
        private ConnectionVM _connectionVM;
        private ILoggerService _logger;
        private QueryModel queryModel;
        private ObservableCollection<ConnectionModel> _connectedDevices;
        public ObservableCollection<ConnectionModel> ConnectedDevices
        {
            get => _connectedDevices;
            set => SetProperty(ref _connectedDevices, value);
        }
        private ObservableCollection<MethodList> Methods = new ObservableCollection<MethodList>();
        #endregion
        public Controll(ILoggerService logger)
        {
            _connectionVM = Ioc.Default.GetRequiredService<ConnectionVM>(); // Get the connection view model
            ConnectedDevices = _connectionVM.connectedDevices; // Sync the connected devices collection
            _connectionVM.PropertyChanged += ConnectionVM_PropertyChanged; // Subscribe to property changed event
            queryModel = new QueryModel();
            _logger = logger;           
        }
        // Event handler for property changed event, sync the connected devices collection
        private void ConnectionVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ConnectedDevices = _connectionVM.connectedDevices;
        }



        #region Methods

        //Send message when property changed with value
        partial void OnStatusMessageChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(value);
        }
        #endregion

        #region Commands
        [RelayCommand]
        public async Task GetMethods(ConnectionModel device)
        {
            try
            {
                // Add the following code inside the GetMethods() method in the Controll class
                var availableMethods = await queryModel.GetResponseByQuery<MethodList>("alphaListMethods");


                if (availableMethods != null)
                {
                    foreach (var method in availableMethods.methodNames)
                    {
                        Methods.Add(new MethodList { name = method });
                    }
                }

            }
            catch (Exception e)
            {
                _logger.Log(e.Message, LogType.Error); 
                StatusMessage = e.Message;
                
            }
            
        }
        #endregion

    }

}
