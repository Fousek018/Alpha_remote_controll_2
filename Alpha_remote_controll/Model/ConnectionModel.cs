using Alpha_remote_controll.Services;
using System.IO;
using System.Net.Sockets;

namespace Alpha_remote_controll.Model
{
    public partial class ConnectionModel
    {
        private ILoggerService _logger;
        public NetworkStream _stream { get; private set; }
        public TcpClient _client { get; private set; }

        public ConnectionModel()
        {
            
        }
        public async Task Connect(string ServerAddress, int Port)
        {
            if (_client == null)
            {
                try
                {
                    //New client
                    _client = new TcpClient();
                    await _client.ConnectAsync(ServerAddress, Port);
                    _stream = _client.GetStream();
                    //myslet na barvy u výstupu
                   

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
            else
            {
                throw new Exception("Already connected");            
            }
        }
    }
}
