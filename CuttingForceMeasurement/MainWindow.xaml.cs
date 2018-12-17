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
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string SerialsEmptyString = "(отсутствуют)";

        private bool isDemoMode = false;
        private bool isSaved = true;
        private Regex doubleRegex = new Regex(@"^-?(\d*)\.?(\d*)$");
        private Regex numberRegex = new Regex(@"\d");
        private SensorsData CurrentSensorsData;
        private string oldGroupName = "";
        private string oldStudentName = "";
        private int itemsCounter = 0;

        public Settings CurrentSettings { get; set; }
        public Settings PrevioslySettings { get; set; }
        public ObservableCollection<SensorDataItem> SensorsData = new ObservableCollection<SensorDataItem>();

        /// <summary>
        /// Здесь проводится загрузка активных COM портов, загрузка настроек приложения, 
        /// а так же устанавливает свойство <c>DataContext</c> для некоторых диалогов
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            LoadSerialPorts();
            CurrentSettings = new Settings();
            CurrentSettings.Load();
            if (CurrentSettings.DemoMode)
            {
                OnDemoMode(null, null);
            }
            SettingsDialog.DataContext = CurrentSettings;
            this.SensorsDataTable.ItemsSource = this.SensorsData;
        }

        /// <summary>
        /// Чтение списка COM портов, выполняет изменение списка портов в представлении приложения
        /// </summary>
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

        /// <summary>
        /// Обработка события выхода из приложения. Если результаты измерения <b>не сохранены</b>, то показывается диалог
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (!isSaved)
            {
                ExitDialog.IsOpen = true;
            } else
            {
                Close();
            }
        }

        /// <summary>
        /// Используется для ручного управления перемещением окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        

        private void DialogHost_ExitDialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (Equals(eventArgs.Parameter, "save"))
            {
                ExportToExcel(true);
            } else if (Equals(eventArgs.Parameter, "close")) {
                Close();
            }
                
        }

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
            LoadSerialPorts();
            this.MessageDialog.DialogClosing -= MessageDialog_ClosingConnectSensors;
            MessageDialogButtonOk.Content = "Хорошо";
        }

        /// <summary>
        /// Останавливает и запускает запись данных. Выполняет проверку полей на наличие названия группы и имени студента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            isSaved = false;
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
                    CurrentSensorsData = new SensorsDataSerial(this, ComPort.SelectedValue.ToString());
                }
                Thread t = new Thread(CurrentSensorsData.Read);
                t.Start();
            }

        }

        /// <summary>
        /// Обновляет результаты измерения из внешенго потока
        /// </summary>
        /// <param name="sensorDataItem">новые данные от датчиков</param>
        public void UpdateSensorsData(SensorDataItem sensorDataItem)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                SensorsData.Add(sensorDataItem);
                if(itemsCounter >= 7)
                {
                    SensorsDataTable.ScrollIntoView(sensorDataItem);
                    itemsCounter = 0;
                }
                itemsCounter++;
            });

        }

        /// <summary>
        /// Выводит сообщение об ошибки при чтении данных датчиков. Останавлиает запись.
        /// </summary>
        /// <param name="e">ошибка</param>
        public void TriggerErrorReading(Exception e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.StopReading();
                this.ShowMessage($"Ошибка {e.GetType().Name}: { e.Message }");
            });

        }

        /// <summary>
        /// Обновление времени записи на представлении
        /// </summary>
        /// <param name="time">текущее время записи</param>
        public void SetTimeReading(double time)
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

        /// <summary>
        /// Очистка результатов записи на представлении
        /// </summary>
        public void ResetAll()
        {
            TimeRecording.Text = "готов";
            SensorsData.Clear();
            StopReading();
            isSaved = true;
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SensorsData.Count() <= 0)
            {
                ShowMessage("Сначала запишите данные датчиков");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Текст (*.txt)|*.txt",
                DefaultExt = "txt"
            };
            var desctopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            saveFileDialog.InitialDirectory = desctopPath;
            saveFileDialog.FileName = $"{GroupName.Text} {StudentName.Text}.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, SerializeToText());
                    isSaved = true;
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
            ExportToExcel(false);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            PrevioslySettings = (Settings)CurrentSettings.Clone();
            SettingsDialog.IsOpen = true;
        }

        private void SettingsDialog_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (Equals(eventArgs.Parameter, false))
            {
                CurrentSettings = (Settings)PrevioslySettings.Clone();
                SettingsDialog.DataContext = CurrentSettings;

            }
            else
            {
                CurrentSettings.Save();
                if (this.SensorsData.Count() > 0)
                {
                    UpdateSensarsDataTableDialog.IsOpen = true;
                }
            }
        }

        /// <summary>
        /// Сохраняет результаты записи в документ Excel
        /// </summary>
        /// <param name="closeOnSaved">закрыть при успешно сохранении</param>
        public void ExportToExcel(bool closeOnSaved)
        {
            if (SensorsData.Count() <= 0)
            {
                ShowMessage("Сначала запишите данные датчиков");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
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
                        isSaved = true;
                        if (closeOnSaved)
                        {
                            Close();
                        } else
                        {
                            ShowMessage($"Файл {saveFileDialog.SafeFileName} успешно сохранен!");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Ошибка сохранения {saveFileDialog.SafeFileName}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Ограничение ввода. Разрешает вводить только дробные числа с точкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Double_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = ((TextBox)sender);
            // то что сейчас введено
            string currentInput = tb.Text;
            // положение каретки
            int caretIndex = tb.CaretIndex;
            // текущий символ ввода
            string inputedSymbol = e.Text;
            // Замена запятой на точку
            if (inputedSymbol == ",")
            {
                inputedSymbol = ".";
            }
            // если есть выбраный текст, то удалить его и изменить положение каретки
            if (tb.SelectedText.Length > 0)
            {
                currentInput = currentInput.Remove(tb.SelectionStart, tb.SelectionLength);
                if (tb.SelectionStart != caretIndex)
                {
                    tb.CaretIndex = tb.SelectionStart;
                    caretIndex = tb.SelectionStart;
                }
            }
            // вставляем введенный символ в строку
            currentInput = currentInput.Insert(caretIndex, inputedSymbol);
            // проверка на валидность ввода, замена текста в поле, установка каретки на нужную позицию
            if (doubleRegex.IsMatch(currentInput))
            {
                
                tb.Text = currentInput;
                tb.CaretIndex = caretIndex+1;
            }
            // прерываем ввод, так как текст в поле изменяется вручную
            e.Handled = true;
        }

        /// <summary>
        /// Ограничение ввода. Разрешает вводить только числа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = ((TextBox)sender);
            if (numberRegex.IsMatch(e.Text))
            {
                var pos = tb.CaretIndex;
                    tb.Text = tb.Text.Insert(pos, e.Text);
                    tb.CaretIndex = pos + 1;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Выполняет обновление результатов записи в случае измения коэффициентов в настройках
        /// </summary>
        private void UpdateSensorsDataTable()
        {
            if (this.SensorsData.Count() <= 0)
            {
                return;
            }
            for(int i = 0; i < SensorsData.Count(); i++)
            {
                var sdi = SensorsData[i];
                sdi.Acceleration *= CurrentSettings.AccelerationCoef / PrevioslySettings.AccelerationCoef;
                sdi.Force *= CurrentSettings.ForceCoef / PrevioslySettings.ForceCoef;
                sdi.Voltage *= CurrentSettings.VoltageCoef / PrevioslySettings.VoltageCoef;
                sdi.Amperage *= CurrentSettings.AmperageCoef / PrevioslySettings.AmperageCoef;
                sdi.Rpm *= CurrentSettings.RpmCoef / PrevioslySettings.RpmCoef;
                // Научится динамически изменять значение
                SensorsData.RemoveAt(i);
                SensorsData.Insert(i, sdi);
            }
        }

        private void UpdateSensarsDataTableDialog_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (!Equals(eventArgs.Parameter, true)) return;
            isSaved = false;
            UpdateSensorsDataTable();
        }

        private void RefreshCOM_Click(object sender, RoutedEventArgs e)
        {
            LoadSerialPorts();
        }

        private void OpenInfo_Click(object sender, RoutedEventArgs e)
        {
            var infoWindow = new InfoWindow();
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }
    }
}
