using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static Alpha_remote_controll.VM.MainWindowVM;

namespace Alpha_remote_controll.VM
{
    public interface IMainVM
    {
    }
    public partial class MainWindowVM : ObservableObject, IMainVM
    {
        [ObservableProperty]
        public string _Status;

        private DispatcherTimer _timer;

        public MainWindowVM()
        {
            Console.WriteLine($"Received message");

            WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
            {
                Console.WriteLine($"Received message: {m}");
                Status = m;
                ResetTimer();
            });

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _timer.Tick += (s, e) => Status = string.Empty;
        }

       

        private void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
        }
    }


}

  

