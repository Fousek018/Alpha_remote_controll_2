using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alpha_remote_controll.Model
{
    public class MethodList
    {
        public List<string> methodNames { get; set; }
        public string name { get; set; }
    }
    public class MethodInfo
    {
        public string name { get; set; }
        public string axisDistance { get; set; }
        public string specimenThicknes { get; set; }
        public List<string> probeInfos { get; set; }
    }
    public class Value
    {
        public string name { get; set; }
        public List<string> values { get; set; }
    }
    public class Records
    {
        public string name { get; set; }
        public List<Value> values { get; set; }
    }
    public class ApiVersion
    {
        public string name { get; set; }
        public string majorVersion { get; set; }
        public string minorVersion { get; set; }
    }
    public class messageList
    {
        public string name { get; set; }
        public string messageNames { get; set; }
    }
    public partial class QueryModel
    {
        #region Properties
        public string StatusMessage;
        #endregion

        #region Fields
        private NetworkStream _stream => _connectionModel._stream;
        private ConnectionModel _connectionModel;
        private TcpClient _client;
        public MethodList AvailableMethods { get; private set; } = new MethodList();
        #endregion

        public QueryModel()
        {
            _connectionModel = Ioc.Default.GetService<ConnectionModel>();
        }
        // Parse response from server
        private T ParseMethodListResponse<T>(string jsonResponse)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonResponse);
            }
            catch (JsonException jsonEx)
            {
                throw;
            }

        }

        #region Methods
        // Ask server and wait for response
        public async Task<T> GetResponseByQuery<T>(string queryName)
        {
            _client = _connectionModel._client;
            if (_client == null || !_client.Connected)
            {
                throw new Exception("Not connected to server");
            }

            try
            {
                string queryCommand = $@"{{""name"": ""{queryName}""}}";
                byte[] data = Encoding.UTF8.GetBytes(queryCommand + "\n");

                // Send command to server asynchronously
                await _stream.WriteAsync(data, 0, data.Length);

                // Receive response asynchronously
                using (var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true))
                {
                    string response = await reader.ReadLineAsync();
                    try 
                    {
                        T result = ParseMethodListResponse<T>(response);
                        return result;
                    }
                    catch(Exception ex)
                    {
                        throw;
                    }
                }
            }
            catch (IOException ioEx)
            {
                throw new Exception("Error during communication with server:", ioEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error: {ex.Message}"); // Not specific exception, this will log the error and rethrow it
            }
        }
        #endregion
    }

}
