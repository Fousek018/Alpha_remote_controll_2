using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_remote_controll.Model
{
    public partial class ConnectionDetails : ObservableObject
    {
        [ObservableProperty]
        public string? _DeviceName;
        [ObservableProperty]
        public string? _ServerAddress;
        [ObservableProperty]
        public int _Port;
        [ObservableProperty]
        public TcpClient? _Client;
        [ObservableProperty]
        public NetworkStream? _Stream;
        [ObservableProperty]
        public List<string>? _MethodNames;
        [ObservableProperty]
        public string? _SelectedMethod;


    }
}
