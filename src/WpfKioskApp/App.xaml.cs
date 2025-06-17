using System.Windows;
using log4net;
using log4net.Config;
using System.IO;

namespace WpfKioskApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            // Initialize log4net configuration
            XmlConfigurator.Configure(new FileInfo("logging.config"));
        }
    }
}
