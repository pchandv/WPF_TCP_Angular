using System;
using log4net;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using WpfKioskApp.Services;

namespace WpfKioskApp.Interop
{
    /// <summary>
    /// Bridges messages between WebView2 frontend and TcpClientManager.
    /// </summary>
    public class WebMessageBridge : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WebMessageBridge));
        private readonly WebView2 _webView;
        private readonly TcpClientManager _tcpClient;

        public WebMessageBridge(WebView2 webView, TcpClientManager tcpClient)
        {
            _webView = webView;
            _tcpClient = tcpClient;
            _webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                _webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                _tcpClient.MessageReceived += TcpClient_MessageReceived;
                _tcpClient.StatusChanged += TcpClient_StatusChanged;
            }
            else
            {
                Logger.Error("WebView2 initialization failed: " + e.InitializationException);
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.TryGetWebMessageAsString();
                Logger.Info($"From JS: {message}");
                _tcpClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Error processing message from JS", ex);
            }
        }

        private void TcpClient_MessageReceived(object sender, string e)
        {
            try
            {
                _webView.Dispatcher.Invoke(() => _webView.CoreWebView2?.PostWebMessageAsString(e));
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending TCP message to JS", ex);
            }
        }

        private void TcpClient_StatusChanged(object sender, string e)
        {
            try
            {
                _webView.Dispatcher.Invoke(() => _webView.CoreWebView2?.PostWebMessageAsString($"STATUS:{e}"));
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending status to JS", ex);
            }
        }

        public void Dispose()
        {
            if (_webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.WebMessageReceived -= CoreWebView2_WebMessageReceived;
            }
            _tcpClient.MessageReceived -= TcpClient_MessageReceived;
            _tcpClient.StatusChanged -= TcpClient_StatusChanged;
        }
    }
}
