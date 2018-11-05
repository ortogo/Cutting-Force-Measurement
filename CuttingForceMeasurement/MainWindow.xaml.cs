using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OfficeOpenXml;
using MaterialDesignThemes.Wpf;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string SerialsEmptyString = "(отсутствуют)";

        private bool isDemoMode = false;
        private SensorsData CurrentSensorsData;
        public Settings CurrentSettings { get; set; }

        public ObservableCollection<SensorDataItem> SensorsData = new ObservableCollection<SensorDataItem>();

        public MainWindow()
        {
            InitializeComponent();

            LoadSerialPorts();
            CurrentSettings = new Settings();
            CurrentSettings.Load();
            SettingsDialog.DataContext = CurrentSettings;
            this.SensorsDataTable.ItemsSource = this.SensorsData;
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
                this.ComPort.Items.Add(SerialsEmptyString);
                this.ComPort.SelectedIndex = 0;
            }
            else
            {
                // Set loaded ports
                this.ComPort.IsEnabled = true;
                this.ComPort.Items.Clear();
                foreach (string port in ports)
                {
                    this.ComPort.Items.Add(port);
                }

                this.ComPort.SelectedIndex = 0;
            }


        }

        private void Exit_Click(object sender, RoutedEventArgs e)
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

        private string oldGroupName = "";
        private string oldStudentName = "";

        private void OnDemoMode(object sender, RoutedEventArgs e)
        {
            isDemoMode = true;
            ComPort.IsEnabled = false;
            oldGroupName = GroupName.Text;
            oldStudentName = StudentName.Text;
            GroupName.Text = "Demo";
            StudentName.Text = "Student I";

        }

        private void OffDemoMode(object sender, RoutedEventArgs e)
        {
            LoadSerialPorts();
            isDemoMode = false;
            ComPort.IsEnabled = true;
            GroupName.Text = oldGroupName;
            StudentName.Text = oldStudentName;
        }

        private void ShowMessage(string text)
        {
            this.TextMessageDialog.Text = text;
            this.MessageDialog.IsOpen = true;
        }

        private void MessageDialog_ClosingConnectSensors(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            this.MessageDialog.DialogClosing -= MessageDialog_ClosingConnectSensors;
            MessageDialogButtonOk.Content = "Хорошо";
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!isDemoMode)
            {
                if (GroupName.Text.Length == 0)
                {
                    ShowMessage("Введите имя группы");
                    return;
                }
                if (StudentName.Text.Length == 0)
                {
                    ShowMessage("Введите Вашу фамилию и инициалы");
                    return;
                }
                if (ComPort.SelectedValue.ToString() == SerialsEmptyString)
                {
                    this.TextMessageDialog.Text = "Устройство не подключено! Подключите и нажмите Продолжить";
                    this.MessageDialog.DialogClosing += MessageDialog_ClosingConnectSensors;
                    MessageDialogButtonOk.Content = "Продолжить";
                    this.MessageDialog.IsOpen = true;
                    return;
                }
            }
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    this.Record.Content = "Запустить";
                    CurrentSensorsData.Stop();
                    CurrentSensorsData = null;
                }
            }
            else
            {
                this.ResetAll();
                this.Record.Content = "Остановить";
                if (isDemoMode)
                {
                    CurrentSensorsData = new SensorsDataRandom(this);
                }
                else
                {
                    /* var tsw = new TestSerialWriter();
                    Thread ts = new Thread(tsw.Write);
                    ts.Start();
                    */
                    CurrentSensorsData = new SensorsDataSerial(this, ComPort.SelectedValue.ToString());
                }
                Thread t = new Thread(CurrentSensorsData.Read);
                t.Start();
            }

        }

        public void UpdateSensorsData(SensorDataItem sensorDataItem)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                SensorsData.Add(sensorDataItem);
            });

        }

        public void TriggerErrorReading(Exception e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.StopReading();
                this.ShowMessage("Проблемы: " + e.Message);
            });

        }

        public void SetTimeReading(int time)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.TimeRecording.Text = (time.ToString());
            });

        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
        }

        private void ResetAll()
        {
            this.TimeRecording.Text = "готов";
            this.SensorsData.Clear();
            this.StopReading();
        }

        private void StopReading()
        {
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    this.Record.Content = "Запустить";
                    CurrentSensorsData.Stop();
                    CurrentSensorsData = null;
                }
            }
        }

        class TestSerialWriter
        {
            private Random Rand;
            private SerialPort Serial;
            private int count = 0;

            public TestSerialWriter()
            {
                Rand = new Random();
                Serial = new SerialPort();
                Serial.PortName = "COM1";
                Serial.Open();
            }

            public void Stop()
            {
                Serial.Close();
            }

            public void Write()
            {
                while (true)
                {
                    SensorDataItem se = new SensorDataItem();
                    se.Time = count;
                    se.Acceleration = Rand.Next(0, 100);
                    se.Force = Rand.Next(0, 100);
                    se.Voltage = Rand.Next(200, 220);
                    se.Amperage = 3.5 + Math.Round(Rand.NextDouble(), 2) * 2.5;
                    se.Rpm = Rand.Next(2800, 2975);
                    Serial.WriteLine(se.ToString());
                    count++;
                    Task.Delay(50).Wait();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SensorsData.Count() <= 0)
            {
                ShowMessage("Сначала запишите данные датчиков");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текст (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            var desctopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            saveFileDialog.InitialDirectory = desctopPath;
            saveFileDialog.FileName = $"{GroupName.Text} {StudentName.Text}.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, SerializeToText());
                    ShowMessage($"Файл {saveFileDialog.SafeFileName} успешно сохранен!");
                }
                catch (Exception ex)
                {
                    ShowMessage($"Ошибка сохранения {saveFileDialog.SafeFileName}: {ex.Message}");
                }
            }
        }

        private string SerializeToText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Время, мс\tУскорение, м/с^2\tУсилие, кН\tНапряжение, В\tТок, А\tЧастота об/микросек\t");
            foreach (SensorDataItem sdi in SensorsData)
            {
                sb.AppendLine(sdi.ToString());
            }
            return sb.ToString();
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (SensorsData.Count() <= 0)
            {
                ShowMessage("Сначала запишите данные датчиков");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Книга Excel (*.xlsx)|*.xlsx";
            saveFileDialog.DefaultExt = "xlsx";
            var desctopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            saveFileDialog.InitialDirectory = desctopPath;
            saveFileDialog.FileName = $"{GroupName.Text} {StudentName.Text}.xlsx";

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var p = new ExcelPackage())
                {
                    var ws = p.Workbook.Worksheets.Add("Результаты");
                    ws.Cells["A1"].Value = "Время, мс";
                    ws.Cells["B1"].Value = "Ускорение, м / с ^ 2";
                    ws.Cells["C1"].Value = "Усилие, кН";
                    ws.Cells["D1"].Value = "Напряжение, В";
                    ws.Cells["E1"].Value = "Ток, А";
                    ws.Cells["F1"].Value = "Частота об/ микросек";

                    for (int i = 2; i < SensorsData.Count() + 2; i++)
                    {
                        var sdi = SensorsData[i - 2];
                        ws.Cells[i, 1].Value = sdi.Time;
                        ws.Cells[i, 2].Value = sdi.Acceleration;
                        ws.Cells[i, 3].Value = sdi.Force;
                        ws.Cells[i, 4].Value = sdi.Voltage;
                        ws.Cells[i, 5].Value = sdi.Amperage;
                        ws.Cells[i, 6].Value = sdi.Rpm;
                    }
                    try
                    {
                        p.SaveAs(new FileInfo(saveFileDialog.FileName));
                        ShowMessage($"Файл {saveFileDialog.SafeFileName} успешно сохранен!");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Ошибка сохранения {saveFileDialog.SafeFileName}: {ex.Message}");
                    }
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog.IsOpen = true;
        }

        private void SettingsDialog_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            CurrentSettings.Save();
        }
    }
}
