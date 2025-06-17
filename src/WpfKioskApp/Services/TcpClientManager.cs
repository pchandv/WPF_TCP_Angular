using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace WpfKioskApp.Services
{
    /// <summary>
    /// Asynchronous TCP client with reconnect and event callbacks.
    /// </summary>
    public class TcpClientManager : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TcpClientManager));

        private readonly string _host;
        private readonly int _port;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1,1);
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> StatusChanged;

        public TcpClientManager(string host, int port)
        {
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Starts the connection loop.
        /// </summary>
        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            await Task.Run(() => RunAsync(_cts.Token));
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_host, _port);
                    _stream = _client.GetStream();
                    StatusChanged?.Invoke(this, "Connected");
                    Logger.Info("Connected to TCP server");
                    await ReadLoop(token);
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke(this, "Disconnected");
                    Logger.Error("TCP connection error", ex);
                    Cleanup();
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), token);
                    }
                    catch (TaskCanceledException) { }
                }
            }
        }

        private async Task ReadLoop(CancellationToken token)
        {
            var buffer = new byte[1024];
            while (!token.IsCancellationRequested && _client.Connected)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) throw new Exception("Disconnected");
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Logger.Info($"Received: {msg}");
                    MessageReceived?.Invoke(this, msg);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error reading from TCP", ex);
                    break;
                }
            }
        }

        public async Task SendAsync(string message)
        {
            if (_stream == null || !_client.Connected) return;
            await _sendLock.WaitAsync();
            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                Logger.Info($"Sent: {message}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending to TCP", ex);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            Cleanup();
        }

        private void Cleanup()
        {
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
        }

        public void Dispose()
        {
            Stop();
            _sendLock.Dispose();
            _cts?.Dispose();
        }
    }
}
