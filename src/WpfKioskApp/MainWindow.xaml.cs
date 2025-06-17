using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using log4net;

namespace WpfKioskApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));
        private readonly TcpClientManager _tcpClientManager;

        public MainWindow()
        {
            InitializeComponent();
            _tcpClientManager = new TcpClientManager("127.0.0.1", 9000, OnTcpMessageReceived, OnTcpStatusChanged);
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string indexPath = Path.Combine(exePath, "index.html");
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.WebMessageReceived += WebMessageReceived;
                webView.Source = new Uri(indexPath);

                await _tcpClientManager.StartAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Initialization failed", ex);
            }
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                Logger.Info($"Message from frontend: {message}");
                _tcpClientManager.SendAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Error processing message from frontend", ex);
            }
        }

        private void OnTcpMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    webView.CoreWebView2?.PostWebMessageAsString(message);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error sending message to frontend", ex);
                }
            });
        }

        private void OnTcpStatusChanged(string status)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    webView.CoreWebView2?.PostWebMessageAsString($"STATUS:{status}");
                }
                catch (Exception ex)
                {
                    Logger.Error("Error sending status to frontend", ex);
                }
            });
        }
    }
}
