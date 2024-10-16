using Alpha_remote_controll.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Alpha_remote_controll.Services
{
    public interface IQueryService
    {
        Task<T> SendQueryAsync<T>(string queryName, ConnectionDetails connectionDetails);
        Task StartListeningForEventsAndQueriesAsync(ConnectionDetails connectionDetails);
    }
    public partial class QueryService : ObservableObject ,IQueryService
    {

        #region Fields
        private AlphaResponseBase _responseValue;
        private NetworkStream _stream;
        private readonly ConnectionDetails _connectionModel;
        private TcpClient _client;
        private CancellationTokenSource _listeningCancellationTokenSource;
        private Dictionary<string, Action<string>> _pendingQueries = new Dictionary<string, Action<string>>();
        #endregion

        public QueryService()
        {
        }

 

        //public bool StopListening() //Sto listening for events and queries
        //{
        //    _listeningCancellationTokenSource.Cancel();
        //    return true;
        //}
        //public bool StartListening()
        //{
        //    StartListeningForEventsAndQueriesAsync();
        //    return false;
        //}
        public async Task StartListeningForEventsAndQueriesAsync(ConnectionDetails connectionDetails)
        {
            _client = connectionDetails.Client;
            _stream = connectionDetails.Stream;
            try
            { 
                _client = connectionDetails.Client; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (_client == null || !_client.Connected)
            {
                throw new Exception("Not connected to server");
            }

            using (var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true))
            {
                _listeningCancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _listeningCancellationTokenSource.Token;

                while (!token.IsCancellationRequested)
                {
                    string response = await reader.ReadLineAsync();

                    if (!string.IsNullOrEmpty(response))
                    {
                        await HandleReceivedMessage(response);
                    }
                    else
                    {
                        await Task.Delay(100);  // Malé zpoždění pro snížení zátěže CPU
                    }
                }
            }
        }
        // Send query to the server and wait for the response
        public async Task<T> SendQueryAsync<T>(string serializedCommand, ConnectionDetails connectionDetails)
        {
            // Používáme dynamicky předaný ConnectionDetails
            _client = connectionDetails.Client;
            _stream = connectionDetails.Stream;
            if (_client == null || !_client.Connected)
            {
                throw new Exception("Not connected to server");
            }

            byte[] data = Encoding.UTF8.GetBytes(serializedCommand + "\n");

            await _stream.WriteAsync(data, 0, data.Length);
            var taskCompletionSource = new TaskCompletionSource<T>();
            string typeKey = typeof(T).FullName;
            _pendingQueries.Add(typeKey, response => // Add the query to the pending queries dictionary with the response handler
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(response);
                    taskCompletionSource.SetResult(result); // Set the result of the query
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex); // Set the exception if the query fails
                }
            });

            return await taskCompletionSource.Task; // Wait for the query to complete 
        }

        // Handle received message and deserialize it to the appropriate type or call the appropriate query handler
        private async Task HandleReceivedMessage(string response)
        {
            using (JsonDocument doc = JsonDocument.Parse(response))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("name", out JsonElement nameElement))
                {
                    string messageType = nameElement.GetString();
                    messageType = "Alpha_remote_controll.Model." + messageType;
                    if (_pendingQueries.TryGetValue(messageType, out var queryHandler))
                    {
                        queryHandler(response);
                        _pendingQueries.Remove(messageType);
                    }
                    else
                    {
                        if (Type.GetType(messageType) is Type responseType)
                        {
                            var deserializedResponse = JsonSerializer.Deserialize(response, responseType);
                            
                        }
                        else
                        {
                            Console.WriteLine($"Přijata neznámá událost: {messageType}");
                        }
                    }
                }
            }
        }
    }
}
