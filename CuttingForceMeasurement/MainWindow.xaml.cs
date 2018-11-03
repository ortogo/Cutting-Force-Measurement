using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDemoMode = false;
        public ObservableCollection<SensorDataItem> SensorsData = new ObservableCollection<SensorDataItem>();

        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = this;
            this.SensorsDataTable.ItemsSource = this.SensorsData;
            SensorDataItem se = new SensorDataItem();
            se.Time = 10;
            se.Acceleration = 100;
            se.Force = 120;
            se.Voltage = 220;
            se.Amperage = 4.5;
            se.Rpm = 2000;
            Console.WriteLine(se);
            SensorsData.Add(se);
        }

        private  void Exit_Click(object sender, RoutedEventArgs e)
        {
            ExitDialog.IsOpen = true;
        }

        private void NavigationBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (!Equals(eventArgs.Parameter, true)) return;
            Close();
        }

        private void OnDemoMode(object sender, RoutedEventArgs e)
        {
            isDemoMode = true;
            ComPort.IsEnabled = false;
        }

        private void OffDemoMode(object sender, RoutedEventArgs e)
        {
            isDemoMode = false;
            ComPort.IsEnabled = true;
        }

    }
}
