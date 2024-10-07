using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Alpha_remote_controll.VM;
using Alpha_remote_controll.Services;
using System.IO;
using Alpha_remote_controll.Model;

namespace Alpha_remote_controll
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }
        private void ConfigureServices()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            string logFilePath = Path.Combine(logDirectory, "log.xml");

            // Zajištění, že složka data existuje
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            Ioc.Default.ConfigureServices(
             new ServiceCollection()
                  .AddSingleton<ILoggerService>(provider =>
                        new LoggerService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.xml")))
                 .AddSingleton<IMainVM, MainWindowVM>()
                 .AddSingleton<MainWindowVM>()
                 .AddSingleton<ConnectionVM>()
                 .AddSingleton<LogVM>()
                 .AddSingleton<Controll>()
                 .BuildServiceProvider()

             );
        }
    }

}
