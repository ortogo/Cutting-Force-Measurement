using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

            LoadSerialPorts();

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

        private void LoadSerialPorts()
        {
            // Loading state
            this.ComPort.IsEnabled = false;
            

            this.ComPort.Items.Clear();
            this.ComPort.Items.Add("Загрузка");
            this.ComPort.SelectedIndex = 0;

            // Get list of available serial ports
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length <= 0)
            {
                // Set empty state for list
                this.ComPort.Items.Clear();
                this.ComPort.Items.Add("Пусто");
                this.ComPort.SelectedIndex = 0;
            } else
            {
                // Set loaded ports
                this.ComPort.IsEnabled = true;
                this.ComPort.Items.Clear();
                foreach (string port in ports) {
                    this.ComPort.Items.Add(port);
                }
                
                this.ComPort.SelectedIndex = 0;
            }
            
            
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
            LoadSerialPorts();
            isDemoMode = false;
            ComPort.IsEnabled = true;
        }

        private async void Record_Click(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() => SensorsDataReader.Read(this),
                                TaskCreationOptions.LongRunning);
        }

        public void UpdateSensorsData(SensorDataItem sensorDataItem)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                SensorsData.Add(sensorDataItem);
            });
            
        }

        public void SetTimeReading(int time)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.TimeRecording.Text = (time.ToString());
            });
            
        }

        class SensorsDataReader
        {
            public static void Read(MainWindow main)
            {
                Random rand = new Random();
                for (var i = 0; i < 20; i++)
                {
                    SensorDataItem se = new SensorDataItem();
                    se.Time = 10+i*2;
                    se.Acceleration = rand.Next(0, 100);
                    se.Force = rand.Next(0, 100);
                    se.Voltage = rand.Next(200, 220);
                    se.Amperage = 3.5 + Math.Round(rand.NextDouble(), 2) * 2.5;
                    se.Rpm = rand.Next(2800, 2975);
                    main.UpdateSensorsData(se);
                    main.SetTimeReading(i);
                    Task.Delay(100).Wait();
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            this.TimeRecording.Text = "готов";
            this.SensorsData.Clear();
        }
    }
}
