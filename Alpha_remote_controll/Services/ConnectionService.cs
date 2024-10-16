using Alpha_remote_controll.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_remote_controll.Services
{
    public interface IConnectionService
    {
        Task ConnectAsync(ConnectionDetails connectionDetails); 
        string Name { get; }
        string ServerAddress { get; }
        int Port { get; }
        TcpClient Client { get; }
        NetworkStream Stream { get; }
        void Disconnect();
    }
    public class ConnectionService : IConnectionService
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public string Name { get; private set; }
        public string ServerAddress { get; private set; }
        public int Port { get; private set; }
        public TcpClient Client => _client;
        public NetworkStream Stream => _stream;

        public async Task ConnectAsync(ConnectionDetails connectionDetails)
        {
            try
            {
                ServerAddress = connectionDetails.ServerAddress;
                Port = connectionDetails.Port;
                _client = new TcpClient();
                await _client.ConnectAsync(ServerAddress, Port);
                _stream = _client.GetStream();
                Name = connectionDetails.DeviceName;

                // Aktualizace modelu na základě vytvořeného spojení
                connectionDetails.Client = _client;
                connectionDetails.Stream = _stream;
            }
            catch (IOException ioEx)
            {
                throw new Exception("Error during communication with server:", ioEx);
            }
            catch (Exception ex)
            {
                _client = null;
                throw new Exception($"Connection failed: {ex.Message}");
            }
        }
        public void Disconnect()
        {
            if (_client?.Connected == true)
            {
                _stream?.Close();
                _client?.Close();
            }
            _client = null;
            _stream = null;
        }
    }
    }
