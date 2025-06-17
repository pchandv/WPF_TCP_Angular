using log4net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfKioskApp
{
    /// <summary>
    /// Manages TCP connection and message dispatching.
    /// </summary>
    public class TcpClientManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TcpClientManager));

        private readonly string _host;
        private readonly int _port;
        private readonly Action<string> _onMessage;
        private readonly Action<string> _onStatus;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public TcpClientManager(string host, int port, Action<string> onMessage, Action<string> onStatus)
        {
            _host = host;
            _port = port;
            _onMessage = onMessage;
            _onStatus = onStatus;
        }

        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            await ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host, _port);
                    _stream = _client.GetStream();
                    _onStatus?.Invoke("Connected");
                    Logger.Info("Connected to TCP server");
                    await ReadLoop(_cts.Token);
                }
                catch (Exception ex)
                {
                    _onStatus?.Invoke("Disconnected");
                    Logger.Error("TCP connection error", ex);
                    await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token); // retry delay
                }
            }
        }

        private async Task ReadLoop(CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0)
                    {
                        throw new Exception("Disconnected");
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _onMessage?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error reading from TCP", ex);
                    _stream?.Close();
                    _client?.Close();
                    break;
                }
            }
        }

        public async Task SendAsync(string message)
        {
            if (_stream == null || !_client.Connected) return;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending to TCP", ex);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _stream?.Close();
            _client?.Close();
        }
    }
}
