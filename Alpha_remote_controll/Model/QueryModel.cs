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
using Utility.Messaging;
using Utils.Logger;
using MediatR;

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
        private CancellationTokenSource _listeningCancellationTokenSource;
        #endregion

        public QueryModel()
        {
            _connectionModel = Ioc.Default.GetService<ConnectionModel>();
            
            
        }
        // Parse the response from server, return the result
        private T ParseMethodListResponse<T>(string jsonResponse)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonResponse); // Deserialize the response, using response from server as parameter
            }
            catch (JsonException jsonEx)
            {
                throw;
            }

        }

        #region Methods
        // Ask server and wait for response
        public async Task<T> GetResponseByQueryc<T>(string queryName)
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

        public async Task StartListeningForEventsAndQueriesAsync()
        {
            _client = _connectionModel._client;
            if (_client == null || !_client.Connected)
            {
                throw new Exception("Not connected to server");
            }

            // Pouze jeden StreamReader
            using (var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true))
            {
                _listeningCancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _listeningCancellationTokenSource.Token;

                while (!token.IsCancellationRequested)
                {
                    if (reader.Peek() >= 0)
                    {
                        string response = await reader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(response))
                        {
                            // Rozlišíme, zda jde o odpověď na dotaz nebo event
                            HandleReceivedMessage(response);
                        }
                    }
                    else
                    {
                        await Task.Delay(100);  // Malé zpoždění pro snížení zátěže CPU
                    }
                }
            }
        }

        // Metoda pro odesílání dotazů serveru
        public async Task<T> SendQueryAsync<T>(string queryName)
        {
            _client = _connectionModel._client;
            if (_client == null || !_client.Connected)
            {
                throw new Exception("Not connected to server");
            }

            string queryCommand = $@"{{""name"": ""{queryName}""}}";
            byte[] data = Encoding.UTF8.GetBytes(queryCommand + "\n");

            // Odeslání dotazu
            await _stream.WriteAsync(data, 0, data.Length);

            // Odpověď bude zpracována ve stejné smyčce, kde se naslouchá
            var taskCompletionSource = new TaskCompletionSource<T>();

            // Přidáme logiku, aby odpověď byla zpracována pomocí TaskCompletionSource
            _pendingQueries.Add(queryName, response =>
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(response);
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            // Vrátíme odpověď až po dokončení úkolu (který se vyřeší při přijetí odpovědi)
            return await taskCompletionSource.Task;
        }

        private Dictionary<string, Action<string>> _pendingQueries = new Dictionary<string, Action<string>>();

        // Zpracování přijatých zpráv
        private void HandleReceivedMessage(string response)
        {
            // Pokud odpověď obsahuje nějaké jméno, můžeme identifikovat, zda jde o odpověď na dotaz nebo event
            using (JsonDocument doc = JsonDocument.Parse(response))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("name", out JsonElement nameElement))
                {
                    string messageType = nameElement.GetString();

                    // Ověříme, zda jde o odpověď na dotaz, a vyvoláme zpracování uložené v _pendingQueries
                    if (_pendingQueries.TryGetValue(messageType, out var queryHandler))
                    {
                        queryHandler(response);
                        _pendingQueries.Remove(messageType);  // Odstraníme záznam po vyřešení
                    }
                    else
                    {
                        // Zpracování události (event)
                        Console.WriteLine($"Přijata událost: {messageType}");
                    }
                }
            }
        }

        #endregion
    }

}
