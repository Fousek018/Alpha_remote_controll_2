using Alpha_remote_controll.VM;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alpha_remote_controll.View
{
    /// <summary>
    /// Interakční logika pro ControllView.xaml
    /// </summary>
    public partial class ControllView : UserControl
    {
        public ControllView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<Controll>();
        }
    }
}
