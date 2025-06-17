using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using Microsoft.Web.WebView2.Wpf;
using WpfKioskApp.Services;
using WpfKioskApp.Interop;

namespace WpfKioskApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));
        private TcpClientManager _tcpClient;
        private WebMessageBridge _bridge;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string host = ConfigurationManager.AppSettings["TcpHost"];
                int port = int.Parse(ConfigurationManager.AppSettings["TcpPort"]);

                _tcpClient = new TcpClientManager(host, port);
                await _tcpClient.StartAsync();

                await webView.EnsureCoreWebView2Async();
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string indexPath = Path.Combine(exePath, "UI", "Assets", "index.html");
                webView.Source = new Uri(indexPath);

                _bridge = new WebMessageBridge(webView, _tcpClient);
            }
            catch (Exception ex)
            {
                Logger.Error("Initialization failed", ex);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _bridge?.Dispose();
            _tcpClient?.Stop();
        }
    }
}
