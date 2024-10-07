using Alpha_remote_controll.Services;
using System.IO;
using System.Net.Sockets;

namespace Alpha_remote_controll.Model
{
    public partial class ConnectionModel
    {
        public string ServerAddress { get;  set; }
        public int Port { get;  set; }
        public string DeviceName { get;  set; }
        private ILoggerService _logger;
        public NetworkStream _stream { get;  set; }
        public TcpClient _client { get;  set; }

        public ConnectionModel()
        {

        }
        //Main method for connection, passing server address and port as a parametr
        public async Task<ConnectionModel> ConnectAsync(string serverAddress, int port)
        {
              try
              {
                ServerAddress = serverAddress; //set server address
                Port = port;//set port
                _client = new TcpClient(); // Create a new client for TCP connection
                await _client.ConnectAsync(ServerAddress, Port); // Connect to the server
                _stream = _client.GetStream(); // Get the stream for communication, we will use it for sending and receiving messages
                DeviceName = "Apha connection";
                return this;
              }
              catch (IOException ioEx)
              {
                  throw new Exception("Error during communication with server:", ioEx);
              }
              catch (Exception ex)
               {
                 _client = null; // Release the client
                 throw new Exception($"Connection failed: {ex.Message}");

               }
        }
    }
}
