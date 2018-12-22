using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using OfficeOpenXml;
using MaterialDesignThemes.Wpf;
using CuttingForceMeasurement.Dialogs;
using CuttingForceMeasurement.ViewModels;

namespace CuttingForceMeasurement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string MainIdentifier = "Main";
        const string SerialsEmptyString = "отключен";

        private bool isDemoMode = false;
        private bool isSaved = true;
        private SensorsData CurrentSensorsData;
        private string oldGroupName = "";
        private string oldStudentName = "";
        private int itemsCounter = 0;

        /* dialogs */
        private MessageDialog dialog;
        private ExitDialog exitDialog;
        private SettingsDialog settingsDialog;
        private UpdateResultsTableDialog updateResultsTableDialog;
        public SettingsDialogViewModel settings;
        
        public ObservableCollection<SensorDataItem> SensorsData = new ObservableCollection<SensorDataItem>();

        /// <summary>
        /// Здесь проводится загрузка активных COM портов, загрузка настроек приложения
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            LoadSerialPorts();
            settings = new SettingsDialogViewModel();
            settings.Load();
            if (settings.DemoMode)
            {
                OnDemoMode(null, null);
            }
            this.SensorsDataTable.ItemsSource = this.SensorsData;

            dialog = new MessageDialog();
            exitDialog = new ExitDialog();
            settingsDialog = new SettingsDialog
            {
                DataContext = settings
            };
            settingsDialog.OnDemoMode += OnDemoMode;
            settingsDialog.OffDemoMode += OffDemoMode;
            updateResultsTableDialog = new UpdateResultsTableDialog();
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
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
            if (!isSaved)
            {
                string result = (string)await DialogHost.Show(exitDialog);
                if (Equals(result, "save"))
                {
                    ExportToExcel(true);
                }
                else if (Equals(result, "close"))
                {
                    Close();
                }
            }
            else
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
            DragMove();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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

        private void MessageDialog_ClosingConnectSensors(object sender, DialogClosingEventArgs eventArgs)
        {
            LoadSerialPorts();
        }

        /// <summary>
        /// Останавливает и запускает запись данных. Выполняет проверку полей на наличие названия группы и имени студента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!isDemoMode)
            {
                if (GroupName.Text.Length == 0)
                {
                    dialog.Show("Введите имя группы");
                    return;
                }
                if (StudentName.Text.Length == 0)
                {
                    dialog.Show("Введите Вашу фамилию и инициалы");
                    return;
                }
                if (ComPort.SelectedValue.ToString() == SerialsEmptyString)
                {
                    // CurrentDialog.DialogClosing += MessageDialog_ClosingConnectSensors;
                    dialog.Show(
                        "Устройство не подключено! Подключите и нажмите Продолжить",
                        "Продолжить",
                        MessageDialog_ClosingConnectSensors);
                    return;
                }
            }
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    this.Record.Content = "Запись";
                    CurrentSensorsData.Stop();
                    CurrentSensorsData = null;
                }
            }
            else
            {
                isSaved = false;
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
                if (itemsCounter >= 7)
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
               dialog.Show($"Ошибка {e.GetType().Name}: { e.Message }");
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
            this.TimeRecording.Text = $"{time.ToString()} мс";
            });

        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
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
                    this.Record.Content = "Запись";
                    CurrentSensorsData.Stop();
                    CurrentSensorsData = null;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
            if (SensorsData.Count() <= 0)
            {
                dialog.Show("Сначала запишите данные датчиков");
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
                    dialog.Show($"Файл {saveFileDialog.SafeFileName} успешно сохранен!");
                }
                catch (Exception ex)
                {
                    dialog.Show($"Ошибка сохранения {saveFileDialog.SafeFileName}: {ex.Message}");
                }
            }
        }

        private string SerializeToText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Время, мс\tУскорение ползуна, м/с^2\tУсилие резания, кН\tНапряжение питания ЭД, В\tТок потребления ЭД, А\tЧастота вращения мкс\t");
            foreach (SensorDataItem sdi in SensorsData)
            {
                sb.AppendLine(sdi.ToString());
            }
            return sb.ToString();
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
            ExportToExcel(false);
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
            settings.PrevioslySettings = (Settings)settings.CurrentSettings.Clone();
            var result = await DialogHost.Show(settingsDialog, SettingsDialogHostClosing);
            if (result == null || !(bool)result) return;

            if (SensorsData.Count() > 0)
            {
                var update = await DialogHost.Show(updateResultsTableDialog, MainIdentifier);
                if (update != null && (bool)update)
                {
                    UpdateResultsTable();
                }
            }
        }

        private void SettingsDialogHostClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            CurrentDialog.IsOpen = false;
            bool needSave = false;
            if (Equals(eventArgs.Parameter, true)) {
                needSave = true;
            }
            if (needSave)
            {
                settings.Save();
            }
            else
            {
                // rollback settings
                settings.Set(settings.PrevioslySettings);
            }
        }

        /// <summary>
        /// Сохраняет результаты записи в документ Excel
        /// </summary>
        /// <param name="closeOnSaved">закрыть при успешном сохранении</param>
        public void ExportToExcel(bool closeOnSaved)
        {
            if (SensorsData.Count() <= 0)
            {
                dialog.Show("Сначала запишите данные датчиков");
                return;
            }
            isSaved = false;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
            var desctopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            saveFileDialog.InitialDirectory = desctopPath;
            saveFileDialog.FileName = $"{GroupName.Text} {StudentName.Text}.xlsx";

            Exception exception = null;
            if (saveFileDialog.ShowDialog() == true)
            {
                var result = DialogHost.Show(new ProccesDialog(), MainIdentifier,
                    new DialogOpenedEventHandler((object sender, DialogOpenedEventArgs args) => {
                        DialogSession session = args.Session;

                        using (var p = new ExcelPackage())
                        {
                            var ws = p.Workbook.Worksheets.Add("Результаты");
                            ws.Cells["A1"].Value = "Время, мс";
                            ws.Cells["B1"].Value = "Ускорение ползуна, м / с ^ 2";
                            ws.Cells["C1"].Value = "Усилие резания, кН";
                            ws.Cells["D1"].Value = "Напряжение питания ЭД, В";
                            ws.Cells["E1"].Value = "Ток потребления ЭД, А";
                            ws.Cells["F1"].Value = "Частота вращения, мкс";

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
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                            }
                        }
                        session.Close(false);
                    }));
            }
            
            if (isSaved && exception == null)
            {
                if (closeOnSaved)
                {
                    Close();
                }
                dialog.Show($"Файл {saveFileDialog.SafeFileName} успешно сохранен!");
            } else if (exception != null)
            {
                dialog.Show($"Файл {saveFileDialog.SafeFileName} не был сохранен: { exception.Message }");
            }
        }

        

        /// <summary>
        /// Выполняет обновление результатов записи в случае измения коэффициентов в настройках
        /// </summary>
        private void UpdateResultsTable()
        {
            if (this.SensorsData.Count() <= 0)
            {
                return;
            }
            isSaved = false;
            for (int i = 0; i < SensorsData.Count(); i++)
            {
                var sdi = SensorsData[i];
                sdi.Acceleration *= settings.AccelerationCoef / settings.PrevioslySettings.AccelerationCoef;
                sdi.Force *= settings.ForceCoef / settings.PrevioslySettings.ForceCoef;
                sdi.Voltage *= settings.VoltageCoef / settings.PrevioslySettings.VoltageCoef;
                sdi.Amperage *= settings.AmperageCoef / settings.PrevioslySettings.AmperageCoef;
                sdi.Rpm *= settings.RpmCoef / settings.PrevioslySettings.RpmCoef;
                // Научится динамически изменять значение
                SensorsData.RemoveAt(i);
                SensorsData.Insert(i, sdi);
            }
        }

        private void RefreshCOM_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
            LoadSerialPorts();
        }

        private void OpenInfo_Click(object sender, RoutedEventArgs e)
        {
            // Можно только после остановки записи
            if (CurrentSensorsData != null)
            {
                if (CurrentSensorsData.IsReading)
                {
                    return;
                }
            }
            var infoWindow = new InfoWindow
            {
                Owner = this
            };
            infoWindow.ShowDialog();
        }
    }
}
